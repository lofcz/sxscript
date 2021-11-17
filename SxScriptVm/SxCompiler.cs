using SxScript;

namespace SxScriptVm;

public enum SxPrecedenceTypes
{
    None,
    Assignment,
    Or,
    And,
    Equality,
    Comparison,
    Term,
    Factor,
    Unary,
    Call,
    Primary
}

public class SxParseRule
{
    public Delegate? Prefix { get; set; }
    public Delegate? Infix { get; set; }
    public SxPrecedenceTypes Precedence { get; set; }

    public SxParseRule(Delegate? prefix, Delegate? infix, SxPrecedenceTypes precedenceType)
    {
        Prefix = prefix;
        Infix = infix;
        Precedence = precedenceType;
    }
}

public class SxCompiler
{
    private List<SxToken> Tokens { get; set; }
    private int Index { get; set; }
    private List<string> Errors { get; set; }
    private bool Working { get; set; }
    private SxChunk Chunk { get; set; }

    private SxParseRule[] Rules { get; set; }

    public SxCompiler()
    {
        Rules = new SxParseRule[(int)SxTokenTypes.Length];
        Rules[(int) SxTokenTypes.LeftParen] = new SxParseRule(Grouping, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.RightParen] = new SxParseRule(null, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.RightBrace] = new SxParseRule(null, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Comma] = new SxParseRule(null, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Dot] = new SxParseRule(null, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Minus] = new SxParseRule(Unary, Binary, SxPrecedenceTypes.Term);
        Rules[(int) SxTokenTypes.Plus] = new SxParseRule(Unary, Binary, SxPrecedenceTypes.Term);
        Rules[(int) SxTokenTypes.Semicolon] = new SxParseRule(null, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Slash] = new SxParseRule(null, Binary, SxPrecedenceTypes.Factor);
        Rules[(int) SxTokenTypes.Star] = new SxParseRule(null, Binary, SxPrecedenceTypes.Factor);
        Rules[(int) SxTokenTypes.Exclamation] = new SxParseRule(null, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Number] = new SxParseRule(Number, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Eof] = new SxParseRule(null, null, SxPrecedenceTypes.None);
    }

    public SxChunk Compile(string code)
    {
        Chunk = new SxChunk();

        if (code.IsNullOrWhiteSpace())
        {
            return Chunk;
        }

        SxLexer lexer = new SxLexer(code);
        List<SxToken> tokens = lexer.Tokenize();
        Tokens = tokens;
        Index = 0;
        Working = true;
        Errors = new List<string>();
        
        Expression();
        Consume(SxTokenTypes.Eof, "Očekáván konec skriptu");
        Emit(OpCodes.OP_RETURN);

        return Chunk;
    }

    void Number()
    {
        object val = PreviousToken.Literal;
        Constant(val);
    }

    void Constant(object value)
    {
        AddConstant(value);
    }

    void Expression()
    {
        ParsePrecedence(SxPrecedenceTypes.Assignment);
    }

    void Grouping()
    {
        Expression();
        Consume(SxTokenTypes.RightParen, "Očekáván ) za výrazem");
    }

    void Unary()
    {
        SxTokenTypes type = PreviousToken.Type;
        ParsePrecedence(SxPrecedenceTypes.Unary);

        switch (type)
        {
            case SxTokenTypes.Minus:
            {
                Emit(OpCodes.OP_NEGATE);
                break;
            }
            default:
            {
                return;
            }
        }
    }

    SxParseRule GetRule(SxTokenTypes type)
    {
        return Rules[(int) type];
    }

    void Binary()
    {
        SxTokenTypes op = PreviousToken.Type;
        SxParseRule rule = GetRule(op);
        ParsePrecedence(rule.Precedence + 1);

        switch (op)
        {
            case SxTokenTypes.Plus:
            {
                Emit(OpCodes.OP_ADD);
                break;
            }
            case SxTokenTypes.Minus:
            {
                Emit(OpCodes.OP_SUBSTRACT);
                break;
            }
            case SxTokenTypes.Slash:
            {
                Emit(OpCodes.OP_DIVIDE);
                break;
            }
            case SxTokenTypes.Star:
            {
                Emit(OpCodes.OP_MULTIPLY);
                break;
            }
            default:
            {
                return;
            }
        }
    }

    void ParsePrecedence(SxPrecedenceTypes precedenceType)
    {
        Advance();
        SxParseRule rule = GetRule(PreviousToken.Type);
        rule?.Prefix?.Method.Invoke(this, null);
        
        while (precedenceType <= GetRule(CurrentToken.Type).Precedence)
        {
            Advance();
            SxParseRule infixRule = GetRule(PreviousToken.Type);
            infixRule?.Infix?.Method.Invoke(this, null);
        }
    }

    int AddConstant(object value)
    {
        return CurrentChunk.PushConstant(value);
    }

    SxChunk CurrentChunk => Chunk;

    void Emit(params OpCodes[] opCodes)
    {
        foreach (OpCodes opCode in opCodes)
        {
            CurrentChunk.PushOpCode(opCode);   
        }
    }
    
    void Emit(OpCodes opCode)
    {
        CurrentChunk.PushOpCode(opCode);
    }

    void Emit(byte opCode)
    {
        CurrentChunk.PushOpCode(opCode);
    }

    void Emit(short opCode)
    {
        CurrentChunk.PushOpCode(opCode);
    }

    void Emit(int opCode)
    {
        CurrentChunk.PushOpCode(opCode);
    }

    bool Advance()
    {
        Index++;
        if (Index > Tokens.Count)
        {
            Index--;
            Working = false;
            return true;
        }
        
        if (PreviousToken.Type == SxTokenTypes.Error)
        {
            Error("Nalezen token TK_ERROR");
            return false;
        }

        return true;
    }

    void Consume(SxTokenTypes type, string msg)
    {
        if (CurrentToken.Type == type)
        {
            Advance();
        }
        else
        {
            Error(msg);
        }
    }

    SxToken CurrentToken => Tokens[Index];

    SxToken PreviousToken
    {
        get
        {
            if (Index < 1)
            {
                return null;
            }

            if (Index >= Tokens.Count)
            {
                return Tokens[Tokens.Count - 1];
            }
        
            return Tokens[Index - 1];   
        }
    }

    void Error(string msg, bool halt = true)
    {
        Errors.Add(msg);
        if (halt)
        {
            Working = false;
        }
    }
}