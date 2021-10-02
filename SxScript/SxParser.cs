using SxScript.SxStatements;

namespace SxScript;

public class SxParser<T>
{
    private List<SxToken> Tokens { get; set; }
    private int Current { get; set; }
    
    public SxParser(List<SxToken> tokens)
    {
        Tokens = tokens;
    }

    public List<SxStatement> Parse()
    {
        List<SxStatement> statements = new List<SxStatement>();
        while (!IsAtEnd())
        {
            statements.Add(Statement());
        }

        return statements;
    }
    
    /*
        expression     → statement * EOF ;
        statement      → exprStmt
                         | printStmt ;
        printStmt      → "print" expression ";"? ;
        exprStmt       → expression ";"?      
        expression     → ternary ";" ;      
        ternary        → equality "?" expression ":" expression
                         | equality ;
        equality       → comparison ( ( "!=" | "==" ) comparison )* ;
        comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
        term           → factor ( ( "-" | "+" ) factor )* ;
        factor         → unary ( ( "/" | "*" ) unary )* ;
        unary          → ( "!" | "-" ) unary
                       | primary ;
        primary        → NUMBER | STRING | "true" | "false" | "nil"
                       | "(" expression ")" ;
     */

    SxStatement Statement()
    {
        if (Match(SxTokenTypes.KeywordPrint))
        {
            return PrintStmt();
        }

        return ExprStmt();
    }

    SxStatement PrintStmt()
    {
        SxExpression expr = Expression();

        if (Check(SxTokenTypes.Semicolon))
        {
            Consume(SxTokenTypes.Semicolon, "Očekáván ;");   
        }

        return new SxPrintStatement(expr);
    }

    SxStatement ExprStmt()
    {
        SxExpression expr = Expression();
        if (Check(SxTokenTypes.Semicolon))
        {
            Consume(SxTokenTypes.Semicolon, "Očekáván ;");   
        }

        return new SxExpressionStatement(expr);
    }

    // expression → ternary ;
    SxExpression Expression()
    {
        return Ternary();
    }
    
    // ternary → equality ? expression : expression
    // | equality
    SxExpression Ternary()
    {
        SxExpression expr = Equality();
        if (Match(SxTokenTypes.Question))
        {
            SxExpression caseTrue = Expression();
            Consume(SxTokenTypes.Colon, "V ternárním operátoru chybí :");
            SxExpression caseFalse = Expression();
            return new SxTernaryExpression(expr, caseTrue, caseFalse);
        }

        return expr;
    }

    // equality → comparison ( ( "!=" | "==" ) comparison )* ;
    SxExpression Equality()
    {
        SxExpression expr = Comparison();
        while (Match(SxTokenTypes.ExclamationEqual, SxTokenTypes.EqualEqual))
        {
            SxToken op = Previous();
            SxExpression right = Comparison();
            expr = new SxBinaryExpression(expr, op, right);
        }

        return expr;
    }

    // comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
    SxExpression Comparison()
    {
        SxExpression expr = Term();
        while (Match(SxTokenTypes.Greater, SxTokenTypes.GreaterEqual, SxTokenTypes.Less, SxTokenTypes.LessEqual))
        {
            SxToken op = Previous();
            SxExpression right = Term();
            expr = new SxBinaryExpression(expr, op, right);
        }

        return expr;
    }
    
    // term → factor ( ( "-" | "+" ) factor )* ;
    SxExpression Term()
    {
        SxExpression expr = Factor();
        while (Match(SxTokenTypes.Minus, SxTokenTypes.Plus))
        {
            SxToken op = Previous();
            SxExpression right = Factor();
            expr = new SxBinaryExpression(expr, op, right);
        }

        return expr;
    }

    // factor → unary ( ( "/" | "*" ) unary )* ;
    SxExpression Factor()
    {
        SxExpression expr = Unary();
        while (Match(SxTokenTypes.Slash, SxTokenTypes.Star))
        {
            SxToken op = Previous();
            SxExpression right = Unary();
            expr = new SxBinaryExpression(expr, op, right);
        }

        return expr;
    }
    
    // unary → ( "!" | "-" | "+" ) unary
    // | primary ;
    SxExpression Unary()
    {
        if (Match(SxTokenTypes.Exclamation, SxTokenTypes.Minus, SxTokenTypes.Plus))
        {
            SxToken op = Previous();
            SxExpression right = Unary();
            return new SxUnaryExpression(op, right);
        }
        
        return Primary();
    }
    
    // primary → NUMBER | STRING | "true" | "false" | "nil" 
    // | "(" expression ")" ;
    SxExpression Primary()
    {
        if (Match(SxTokenTypes.Number, SxTokenTypes.String))
        {
            object val = Previous().Literal;
            return new SxLiteralExpression(val);
        }

        if (Match(SxTokenTypes.True))
        {
            return new SxLiteralExpression(true);
        }

        if (Match(SxTokenTypes.False))
        {
            return new SxLiteralExpression(false);
        }

        if (Match(SxTokenTypes.Nill))
        {
            return new SxLiteralExpression(null);
        }

        if (Match(SxTokenTypes.LeftParen))
        {
            SxExpression expr = Expression();
            Consume(SxTokenTypes.RightParen, "Očekávána ) k ukončení páru kulatých závorek");
            return new SxGroupingExpression(expr);
        }

        return null;
    }


    SxToken Consume(SxTokenTypes type, string errMsg)
    {
        if (Check(type))
        {
            return Step();
        }

        return null;
    }

    bool Match(params SxTokenTypes[] types)
    {
        foreach (SxTokenTypes type in types)
        {
            if (Check(type))
            {
                Step();
                return true;
            }
        }

        return false;
    }

    bool Check(SxTokenTypes type)
    {
        if (IsAtEnd())
        {
            return false;
        }

        return Peek().Type == type;
    }

    SxToken Step()
    {
        if (!IsAtEnd())
        {
            Current++;
        }

        return Previous();
    }

    bool IsAtEnd()
    {
        return Peek().Type == SxTokenTypes.Eof;
    }

    SxToken Peek()
    {
        return Tokens[Current];
    }

    SxToken Previous()
    {
        return Tokens[Current - 1];
    }
}