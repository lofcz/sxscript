using System.Collections.Generic;

namespace SxScript
{
    public class SxLexer
    {
        public string Source { get; set; }
        public List<SxToken> Tokens { get; set; } = new List<SxToken>();
        public List<string> Messages { get; set; } = new List<string>();

        public Dictionary<string, SxTokenTypes> Keywords = new Dictionary<string, SxTokenTypes>()
        {
            {"and", SxTokenTypes.KeywordAnd}, {"class", SxTokenTypes.KeywordClass}, {"else", SxTokenTypes.KeywordElse},
            {"false", SxTokenTypes.False}, {"for", SxTokenTypes.KeywordFor}, {"fn", SxTokenTypes.KeywordFunction}, 
            {"func", SxTokenTypes.KeywordFunction}, {"function", SxTokenTypes.KeywordFunction}, {"if", SxTokenTypes.KeywordIf},
            {"null", SxTokenTypes.Nill}, {"nill", SxTokenTypes.Nill}, {"or", SxTokenTypes.KeywordOr}, {"print", SxTokenTypes.KeywordPrint},
            {"return", SxTokenTypes.KeywordReturn}, {"super", SxTokenTypes.KeywordSuper}, {"this", SxTokenTypes.KeywordThis}, {"true", SxTokenTypes.True},
            {"var", SxTokenTypes.KeywordVar}, {"while", SxTokenTypes.KeywordWhile}
        };

        public SxLexer(string source)
        {
            Source = source;
        }

        public List<SxToken> Tokenize()
        {
            int line = 1;
            int pos = 0;
            int start = 0;
            bool tokenize = true;
            char c = ' ';
            string currentLexeme = "";
            bool anyErrors = false;

            while (tokenize)
            {
                start = pos;
                ScanToken();

                if (IsAtEnd())
                {
                    tokenize = false;
                    c = ' ';
                    AddToken(SxTokenTypes.Eof);
                }

                if (anyErrors)
                {
                    tokenize = false;
                }
            }

            bool IsAtEnd()
            {
                return pos >= Source.Length;
            }

            void ScanToken()
            {
                c = Step();
                switch (c)
                {
                     case '(':
                         AddToken(SxTokenTypes.LeftParen); break;
                     case ')':
                         AddToken(SxTokenTypes.RightParen); break;
                     case '{':
                         AddToken(SxTokenTypes.LeftBrace); break;
                     case '}':
                         AddToken(SxTokenTypes.RightBrace); break;
                     case '[':
                         AddToken(SxTokenTypes.LeftBracket); break;
                     case ']':
                         AddToken(SxTokenTypes.RightBracket); break;
                     case ',':
                         AddToken(SxTokenTypes.Comma); break;
                     case '.':
                         AddToken(SxTokenTypes.Dot); break;
                     case ';':
                         AddToken(SxTokenTypes.Semicolon); break;
                     case '+':
                         AddToken(SxTokenTypes.Plus); break;
                     case '-':
                         AddToken(SxTokenTypes.Minus); break;
                     case '*':
                         AddToken(SxTokenTypes.Star); break;
                     case '=':
                     {
                         if (Match('='))
                         {
                             AddToken(SxTokenTypes.EqualEqual);    
                         }
                         else
                         {
                             AddToken(SxTokenTypes.Equal);    
                         }
                         
                         break;
                     }
                     case '!':
                     {
                         if (Match('='))
                         {
                             AddToken(SxTokenTypes.ExclamationEqual);
                         }
                         else
                         {
                             AddToken(SxTokenTypes.Exclamation);
                         }
                         
                         break;
                     }
                     case '>':
                     {
                         if (Match('='))
                         {
                             AddToken(SxTokenTypes.GreaterEqual);
                         }
                         else
                         {
                             AddToken(SxTokenTypes.Greater);
                         }

                         break;
                     }
                     case '<':
                     {
                         if (Match('='))
                         {
                             AddToken(SxTokenTypes.LessEqual);
                         }
                         else
                         {
                             AddToken(SxTokenTypes.Less);
                         }

                         break;
                     }
                     case '/':
                     {
                         if (Match('/'))
                         {
                             while (Peek() != '\n' && !IsAtEnd())
                             {
                                 Step();
                             }
                             
                             DiscardCurrentLexeme();
                         }
                         else
                         {
                             AddToken(SxTokenTypes.Slash);
                         }
                         
                         break;
                     }
                     case ' ':
                     case '\r':
                     case '\t':
                         DiscardCurrentLexeme();
                         break;
                     case '\n':
                         line++;
                         DiscardCurrentLexeme();
                         break;
                     case '\"':
                     {
                         while (!(Peek() == '\"' && Peek(0) != '\\') && !IsAtEnd())
                         {
                             if (Peek() == '\n')
                             {
                                 line++;
                             }

                             Step();
                         }

                         if (IsAtEnd())
                         {
                             Error(line, "Neuzavřený textový řetěz, chybí \"");
                             return;
                         }

                         Step();
                         currentLexeme = currentLexeme.Substring(1, currentLexeme.Length - 2);
                         AddToken(SxTokenTypes.String);
                         
                         break;
                     }
                     default:
                     {
                         if (IsDigit(c))
                         {
                             while (IsDigit(Peek()))
                             {
                                 Step();
                             }

                             if (Peek() == '.' && IsDigit(Peek(2)))
                             {
                                 Step();

                                 while (IsDigit(Peek()))
                                 {
                                     Step();
                                 }
                             }
                             
                             AddToken(SxTokenTypes.Number);
                             break;
                         }
                         else if (IsAlphaNumeric(c))
                         {
                             while (IsAlphaNumeric(Peek()))
                             {
                                 Step();
                             }

                             if (Keywords.TryGetValue(currentLexeme, out SxTokenTypes keywordType))
                             {
                                 AddToken(keywordType);   
                             }
                             else
                             {
                                 AddToken(SxTokenTypes.Identifier);
                             }
                             
                             break;
                         }
                         
                         Error(line, "Neznámý znak");
                         break;   
                     }
                }
            }

            bool IsAlphaNumeric(char ch)
            {
                return IsDigit(ch) || IsAlpha(ch);
            }
            
            bool IsAlpha(char ch)
            {
                return (ch is >= 'a' and <= 'z') || (ch is >= 'A' and <= 'Z') || ch is '_'; 
            }

            bool IsDigit(char ch)
            {
                return ch is >= '0' and <= '9';
            }

            char Step(int i = 1)
            {
                char cc = Source[pos];
                currentLexeme += cc;
                pos += i;
                return cc;
            }

            char Peek(int i = 1)
            {
                if (IsAtEnd())
                {
                    return '\n';
                }

                int peekedPos = pos + i - 1;

                if (peekedPos < 0)
                {
                    pos = 0;
                }

                if (Source.Length <= peekedPos)
                {
                    return Source[^1];
                }

                return Source[peekedPos];
            }

            bool Match(char expected)
            {
                if (IsAtEnd())
                {
                    return false;
                }

                if (Source[pos] != expected)
                {
                    return false;
                }

                Step();
                return true;
            }

            void AddToken(SxTokenTypes type)
            {
                SxToken token = new SxToken(type, currentLexeme, null, line);
                Tokens.Add(token);
                DiscardCurrentLexeme();
            }

            void DiscardCurrentLexeme()
            {
                currentLexeme = "";
            }

            void Error(int line, string message)
            {
                anyErrors = true;
                Messages.Add($"Chyba na řádku {line} - {message}");
            }

            return Tokens;
        }
    }
}