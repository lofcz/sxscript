namespace SxScript
{
    public enum SxTokenTypes
    {
        LeftParen, // (
        RightParen, // )
        LeftBracket, // [
        RightBracket, // ]
        LeftBrace, // {
        RightBrace, // }
        Comma, // ,
        Dot, // .
        Minus, // -
        Plus, // +
        Percent, // %
        Semicolon, // ;
        Slash, // /
        Star, // *
        Question, // ?
        Colon, // :
        Exclamation, // !
        ExclamationEqual, // !=
        Equal, // =
        EqualEqual, // ==
        Greater, // >
        Less, // <
        GreaterEqual, // >=
        LessEqual, // <=
        Identifier,
        String,
        True,
        False,
        Nill, // null
        Number,
        KeywordClass,
        KeywordElse,
        KeywordFunction,
        KeywordFor,
        KeywordIf,
        KeywordAnd, // and, &&
        KeywordOr, // or, ||
        KeywordPrint,
        KeywordReturn,
        KeywordThis,
        KeywordVar,
        KeywordWhile,
        KeywordSuper,
        KeywordBreak,
        KeywordContinue,
        KeywordGoto,
        Eof
    }
    
    public class SxToken
    {
        public SxTokenTypes Type { get; set; }
        public string Lexeme { get; set; }
        public object Literal { get; set; }
        public int Line { get; set; }

        public SxToken(SxTokenTypes type, string lexeme, object literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public string DebugPrint()
        {
            return $"{Type} - {Lexeme} - {Literal}";
        }
    }
}