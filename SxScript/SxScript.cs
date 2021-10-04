using System.Text;
using SxScript.SxStatements;

namespace SxScript;

public class SxScript
{
    public async Task<string> Interpret(string code)
    {
        StringBuilder stdOut = new StringBuilder();
        
        SxLexer lexer = new SxLexer(code.ToString());
        List<SxToken> tokens = lexer.Tokenize();
        
        SxParser<string> parser = new SxParser<string>(tokens);
        List<SxStatement> exprStatements = parser.Parse();
        
        SxInterpreter interpreter = new SxInterpreter();
        object? obj = interpreter.Evaluate(exprStatements, stdOut);

        return stdOut.ToString();
    }
}