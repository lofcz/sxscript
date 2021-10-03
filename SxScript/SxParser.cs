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
            statements.Add(Declaration());
        }

        return statements;
    }
    
    /*
        parse          → declaration * EOF ;
        declaration    → varDeclr
                         | statement ;
        varDeclr       → "var"? IDENTIFIER ("=" expression)? ";"? ;                   
        statement      → exprStmt
                         | ifStmt
                         | whileStmt
                         | printStmt 
                         | block ;
        whileStmt      → "while" "("? expression ")"? statement ;            
        ifStmt         → "if" "("? expression ")"? statement
                         ( "else" statement )? ;                 
        block          → "{" declaration * "}" ;        
        printStmt      → "print" expression ";"? ;
        exprStmt       → expression ";"? ;     
        expression     → assignment ";" ;
        assignment     → IDENTIFIER "=" assignment
                         | logicOr 
                         | logicOr "?" ternary ;
        logicOr        → logicAnd ( "or" logicAnd )* ;
        logicAnd       → equality ( "and" equality )* ;                    
        ternary        → expression ":" expression ;
        equality       → comparison ( ( "!=" | "==" ) comparison )* ;
        comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
        term           → factor ( ( "-" | "+" ) factor )* ;
        factor         → unary ( ( "/" | "*" ) unary )* ;
        unary          → ( "!" | "-" ) unary
                       | primary ;
        primary        → NUMBER | STRING | IDENTIFIER | "true" | "false" | "nil"
                       | "(" expression ")" ;
     */

    SxStatement Declaration()
    {
        if (Match(SxTokenTypes.KeywordVar))
        {
            return VarDeclr();
        }

        /*if (Check(SxTokenTypes.Identifier) && CheckNth(1, SxTokenTypes.Equal))
        {
            return VarDeclr();
        }*/

        return Statement();
    }

    SxStatement VarDeclr()
    {
        SxToken identifier = Consume(SxTokenTypes.Identifier, "Očekáván název proměnné");
        SxExpression initialVal = null;
        
        if (Match(SxTokenTypes.Equal))
        {
            initialVal = Expression();
        }

        if (Check(SxTokenTypes.Semicolon))
        {
            Consume(SxTokenTypes.Semicolon, "Očekáván ;");
        }

        return new SxVarStatement(initialVal, identifier);
    }

    SxStatement Statement()
    {
        if (Match(SxTokenTypes.KeywordPrint))
        {
            return PrintStmt();
        }

        if (Match(SxTokenTypes.LeftBrace))
        {
            return new SxBlockStatement(Block());
        }

        if (Match(SxTokenTypes.KeywordIf))
        {
            return IfStmt();
        }

        if (Match(SxTokenTypes.KeywordWhile))
        {
            return WhileStmt();
        }

        return ExprStmt();
    }

    SxStatement WhileStmt()
    {
        if (Match(SxTokenTypes.LeftParen))
        {
            // (    
        }

        SxExpression expr = Expression();

        if (Match(SxTokenTypes.RightParen))
        {
            // )
        }

        SxStatement statement = Statement();
        return new SxWhileStatement(expr, statement);
    }
    
    SxStatement IfStmt()
    {
        if (Match(SxTokenTypes.LeftParen))
        {
            // (    
        }

        SxExpression expr = Expression();

        if (Match(SxTokenTypes.RightParen))
        {
            // )
        }

        SxStatement thenBranch = Statement();
        SxStatement elseBranch = null!;

        if (Match(SxTokenTypes.KeywordElse))
        {
            elseBranch = Statement();
        }

        return new SxIfStatement(expr, thenBranch, elseBranch);
    }

    List<SxStatement> Block()
    {
        List<SxStatement> statements = new List<SxStatement>();

        while (!Check(SxTokenTypes.RightBrace) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(SxTokenTypes.RightBrace, "Očekávána }");
        return statements;
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
        return Assignment();
    }
    
    SxExpression Assignment()
    {
        SxExpression expr = LogicalOr();
        
        if (Match(SxTokenTypes.Equal))
        {
            SxToken equals = Previous();
            SxExpression value = Assignment();

            if (expr is SxVarExpression varExpr)
            {
                SxToken name = varExpr.Name;
                return new SxAssignExpression(name, value);
            }
            
            // [todo] error, neplatný cíl pro přiřazení
        }

        if (Match(SxTokenTypes.Question))
        {
            return Ternary(expr);
        }

        return expr;
    }

    SxExpression LogicalOr()
    {
        SxExpression expr = LogicalAnd();
        
        while (Match(SxTokenTypes.KeywordOr))
        {
            SxToken op = Previous();
            SxExpression right = LogicalAnd();
            expr = new SxLogicalExpression(expr, op, right);
        }

        return expr;
    }

    SxExpression LogicalAnd()
    {
        SxExpression expr = Equality();
        while (Match(SxTokenTypes.KeywordAnd))
        {
            SxToken op = Previous();
            SxExpression right = Equality();
            return new SxLogicalExpression(expr, op, right);
        }

        return expr;
    }

    // ternary → equality ? expression : expression
    // | equality
    SxExpression Ternary(SxExpression expr)
    {
        SxExpression caseTrue = Expression();
        Consume(SxTokenTypes.Colon, "V ternárním operátoru chybí :");
        SxExpression caseFalse = Expression();
        return new SxTernaryExpression(expr, caseTrue, caseFalse);
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
    
    // primary → NUMBER | STRING | IDENTIFIER | "true" | "false" | "nil" 
    // | "(" expression ")" ;
    SxExpression Primary()
    {
        if (Match(SxTokenTypes.Number, SxTokenTypes.String))
        {
            object val = Previous().Literal;
            return new SxLiteralExpression(val);
        }

        if (Match(SxTokenTypes.Identifier))
        {
            return new SxVarExpression(Previous());
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
    
    bool CheckNth(int n, SxTokenTypes type)
    {
        if (IsAtEnd())
        {
            return false;
        }
        
        return PeekNth(n)?.Type == type;
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
    
    SxToken? PeekNth(int n = 1)
    {
        if (Tokens.Count >= Current + n)
        {
            return Tokens[Current + n];
        }

        return null;
    }

    SxToken Previous()
    {
        return Tokens[Current - 1];
    }
}