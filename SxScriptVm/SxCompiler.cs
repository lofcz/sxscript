using SxScript;

namespace SxScriptVm;

public enum SxPrecedenceTypes
{
    None,
    Assignment,
    Conditional,
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
    private List<SxError> Errors { get; set; }
    private bool Working { get; set; }
    private SxChunk Chunk { get; set; }
    private bool PanicMode { get; set; }
    private SxParseRule[] Rules { get; set; }
    private bool CanAssign { get; set; }
    private bool HasError { get; set; }

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
        Rules[(int) SxTokenTypes.Exclamation] = new SxParseRule(Unary, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Number] = new SxParseRule(Number, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Eof] = new SxParseRule(null, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Question] = new SxParseRule(null, Conditional, SxPrecedenceTypes.Conditional);
        Rules[(int) SxTokenTypes.Colon] = new SxParseRule(null, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.True] = new SxParseRule(Literal, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.False] = new SxParseRule(Literal, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Nill] = new SxParseRule(Literal, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.ExclamationEqual] = new SxParseRule(null, Binary, SxPrecedenceTypes.Equality);
        Rules[(int) SxTokenTypes.Equal] = new SxParseRule(null, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.EqualEqual] = new SxParseRule(null, Binary, SxPrecedenceTypes.Equality);
        Rules[(int) SxTokenTypes.Greater] = new SxParseRule(null, Binary, SxPrecedenceTypes.Comparison);
        Rules[(int) SxTokenTypes.GreaterEqual] = new SxParseRule(null, Binary, SxPrecedenceTypes.Comparison);
        Rules[(int) SxTokenTypes.Less] = new SxParseRule(null, Binary, SxPrecedenceTypes.Comparison);
        Rules[(int) SxTokenTypes.LessEqual] = new SxParseRule(null, Binary, SxPrecedenceTypes.Comparison);
        Rules[(int) SxTokenTypes.String] = new SxParseRule(StringFn, null, SxPrecedenceTypes.None);
        Rules[(int) SxTokenTypes.Identifier] = new SxParseRule(Variable, null, SxPrecedenceTypes.None);
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
        Errors = new List<SxError>();
        PanicMode = false;
        CanAssign = false;
        HasError = false;

        while (!Match(SxTokenTypes.Eof) && Working)
        {
            Entry();
        }

        Emit(OpCodes.OP_RETURN);

        if (HasError)
        {
            Chunk.CompileErrors = Errors;
            Chunk.HasCompileErrors = true;
        }

        return Chunk;
    }

    void Entry()
    {
        Declaration();

        if (PanicMode)
        {
            Synchronize();
        }
    }

    void Declaration()
    {
        if (Match(SxTokenTypes.KeywordVar))
        {
            VarDeclaration();
        }
        else
        {
            Statement();   
        }
    }

    void VarDeclaration()
    {
        int varIndex = ParseVariable("Očekáván název proměnné");
        if (Match(SxTokenTypes.Equal))
        {
            Expression();
        }
        else
        {
            Emit(OpCodes.OP_NULL);
        }

        ConsumeIfMatch(SxTokenTypes.Semicolon, "Očekáván ; na konci deklarace proměnné");
        DefineVariable(varIndex);
    }

    void Variable()
    {
        NamedVariable(PreviousToken);
    }

    void NamedVariable(SxToken token)
    {
        int varIndex = IdentifierConstant(token);

        if (Match(SxTokenTypes.Equal))
        {
            Expression();
            
            if (CanAssign)
            {
                Emit(OpCodes.OP_SET_GLOBAL_8);   
            }
            else
            {
                Error("Neplatný cíl pro přiřazení");
            }
        }
        else
        {
            Emit(OpCodes.OP_GET_GLOBAL_8);
        }

        if (varIndex <= byte.MaxValue)
        {
            Emit(varIndex);
        }
    }

    void DefineVariable(int index)
    {
        if (index <= byte.MaxValue)
        {
            Emit(OpCodes.OP_DEFINE_GLOBAL_8);
            Emit(index);
        }
    }
    
    int ParseVariable(string msg)
    {
        Consume(SxTokenTypes.Identifier, msg);
        return IdentifierConstant(PreviousToken);
    }

    int IdentifierConstant(SxToken token)
    {
        return CurrentChunk.PushConstant(token.Lexeme);
    }

    void Statement()
    {
        if (Match(SxTokenTypes.KeywordPrint))
        {
            PrintStatement();
        }
        else
        {
            ExpressionStatement();
        }
    }

    void ExpressionStatement()
    {
        Expression();
        ConsumeIfMatch(SxTokenTypes.Semicolon);
        Emit(OpCodes.OP_POP);
    }
    
    void PrintStatement()
    {
        Expression();
        ConsumeIfMatch(SxTokenTypes.Semicolon);
        Emit(OpCodes.OP_PRINT);
    }

    bool ConsumeIfMatch(SxTokenTypes token, string msg = "")
    {
        if (!Check(token))
        {
            return false;
        }

        Advance();
        return true;
    }

    void Synchronize()
    {
        PanicMode = false;
        
        while (CurrentToken.Type != SxTokenTypes.Eof)
        {
            if (PreviousToken.Type == SxTokenTypes.Semicolon)
            {
                return;
            }

            switch (CurrentToken.Type)
            {
                case SxTokenTypes.KeywordClass:
                case SxTokenTypes.KeywordFunction:
                case SxTokenTypes.KeywordVar:
                case SxTokenTypes.KeywordFor:
                case SxTokenTypes.KeywordIf:
                case SxTokenTypes.KeywordWhile:
                case SxTokenTypes.KeywordPrint:
                case SxTokenTypes.KeywordReturn:
                    return;

                default:
                    break;
            }

            Advance();
        }
    }

    bool Match(SxTokenTypes type)
    {
        if (!Check(type))
        {
            return false;
        }

        Advance();
        return true;
    }

    bool Check(SxTokenTypes type)
    {
        return CurrentToken.Type == type;
    }

    void StringFn()
    {
        object val = PreviousToken.Literal;
        Constant(val);
    }

    void Literal()
    {
        switch (PreviousToken.Type)
        {
            case SxTokenTypes.False:
            {
                Emit(OpCodes.OP_FALSE);
                break;
            }
            case SxTokenTypes.True:
            {
                Emit(OpCodes.OP_TRUE);
                break;
            }
            case SxTokenTypes.Nill:
            {
                Emit(OpCodes.OP_NULL);
                break;
            }
        }
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

    void Conditional()
    {
        ParsePrecedence(SxPrecedenceTypes.Conditional);
        Consume(SxTokenTypes.Colon, "Očekávána : za první částí ternárního operátoru");
        ParsePrecedence(SxPrecedenceTypes.Assignment);
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
            case SxTokenTypes.Plus:
            {
                break;
            }
            case SxTokenTypes.Exclamation:
            {
                Emit(OpCodes.OP_NOT);
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
            case SxTokenTypes.EqualEqual:
            {
                Emit(OpCodes.OP_EQUAL);
                break;
            }
            case SxTokenTypes.ExclamationEqual:
            {
                Emit(OpCodes.OP_NOT_EQUAL);
                break;
            }
            case SxTokenTypes.Greater:
            {
                Emit(OpCodes.OP_GREATER);
                break;
            }
            case SxTokenTypes.Less:
            {
                Emit(OpCodes.OP_LESS);
                break;
            }
            case SxTokenTypes.GreaterEqual:
            {
                Emit(OpCodes.OP_EQUAL_GREATER);
                break;
            }
            case SxTokenTypes.LessEqual:
            {
                Emit(OpCodes.OP_EQUAL_LESS);
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

        bool canAssign = precedenceType <= SxPrecedenceTypes.Assignment;
        CanAssign = canAssign;
        
        while (precedenceType <= GetRule(CurrentToken.Type).Precedence)
        {
            Advance();
            SxParseRule infixRule = GetRule(PreviousToken.Type);
            infixRule?.Infix?.Method.Invoke(this, null);
        }

        CanAssign = false;
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
        HasError = true;
        Errors.Add(new SxError($"{msg}, token \"{PreviousToken.Lexeme}\"", PreviousToken.Line));
        if (halt)
        {
            Working = false;
        }
    }
}