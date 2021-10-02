namespace SxScript;

public class SxParser<T>
{
    private List<SxToken> Tokens { get; set; }
    private int Current { get; set; }
    
    public SxParser(List<SxToken> tokens)
    {
        Tokens = tokens;
    }

    public SxExpression<T> Parse()
    {
        return Expression();
    }

    // expression → equality ;
    SxExpression<T> Expression()
    {
        return Equality();
    }

    // equality → comparison ( ( "!=" | "==" ) comparison )* ;
    SxExpression<T> Equality()
    {
        SxExpression<T> expr = Comparison();
        while (Match(SxTokenTypes.ExclamationEqual, SxTokenTypes.EqualEqual))
        {
            SxToken op = Previous();
            SxExpression<T> right = Comparison();
            expr = new SxBinaryExpression<T>(expr, op, right);
        }

        return expr;
    }

    // comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
    SxExpression<T> Comparison()
    {
        SxExpression<T> expr = Term();
        while (Match(SxTokenTypes.Greater, SxTokenTypes.GreaterEqual, SxTokenTypes.Less, SxTokenTypes.LessEqual))
        {
            SxToken op = Previous();
            SxExpression<T> right = Term();
            expr = new SxBinaryExpression<T>(expr, op, right);
        }

        return expr;
    }
    
    // term → factor ( ( "-" | "+" ) factor )* ;
    SxExpression<T> Term()
    {
        SxExpression<T> expr = Factor();
        while (Match(SxTokenTypes.Minus, SxTokenTypes.Plus))
        {
            SxToken op = Previous();
            SxExpression<T> right = Factor();
            expr = new SxBinaryExpression<T>(expr, op, right);
        }

        return expr;
    }

    // factor → unary ( ( "/" | "*" ) unary )* ;
    SxExpression<T> Factor()
    {
        SxExpression<T> expr = Unary();
        while (Match(SxTokenTypes.Slash, SxTokenTypes.Star))
        {
            SxToken op = Previous();
            SxExpression<T> right = Unary();
            expr = new SxBinaryExpression<T>(expr, op, right);
        }

        return expr;
    }
    
    // unary → ( "!" | "-" ) unary
    // | primary ;
    SxExpression<T> Unary()
    {
        if (Match(SxTokenTypes.Exclamation, SxTokenTypes.Minus))
        {
            SxToken op = Previous();
            SxExpression<T> right = Unary();
            return new SxUnaryExpression<T>(op, right);
        }
        
        return Primary();
    }
    
    // primary → NUMBER | STRING | "true" | "false" | "nil" 
    // | "(" expression ")" ;
    SxExpression<T> Primary()
    {
        if (Match(SxTokenTypes.Number, SxTokenTypes.String))
        {
            object val = Previous().Literal;
            return new SxLiteralExpression<T>(val);
        }

        if (Match(SxTokenTypes.True))
        {
            return new SxLiteralExpression<T>(true);
        }

        if (Match(SxTokenTypes.False))
        {
            return new SxLiteralExpression<T>(false);
        }

        if (Match(SxTokenTypes.Nill))
        {
            return new SxLiteralExpression<T>(null);
        }

        if (Match(SxTokenTypes.LeftParen))
        {
            SxExpression<T> expr = Expression();
            Consume(SxTokenTypes.RightParen, "Očekávána ) k ukončení páru kulatých závorek");
            return new SxGroupingExpression<T>(expr);
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