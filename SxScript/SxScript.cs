using System.Text;
using SxScript.SxSa;
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
        SxResolver resolver = new SxResolver(interpreter);
        await resolver.Resolve(exprStatements);
        object? obj = await interpreter.Evaluate(exprStatements, stdOut);

        return stdOut.ToString();
    }
}