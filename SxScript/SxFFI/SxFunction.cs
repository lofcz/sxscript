using SxScript.SxStatements;

namespace SxScript.SxFFI;

public class SxFunction : SxExpression.ISxCallable, SxStatement.ISxCallStatement
{
    public SxFunctionStatement Declaration { get; set; }
    public SxBlockStatement Block { get; set; }
    public bool Return { get; set; }
    public SxStatement Statement { get; set; }

    public SxFunction(SxFunctionStatement declaration, SxBlockStatement block)
    {
        Declaration = declaration;
        Block = block;
        Return = false;
        Statement = block;
    }
    
    public object? Call(SxInterpreter interpreter, List<object> arguments)
    {
        SxEnvironment environment = new SxEnvironment(interpreter.Globals);
        for (int i = 0; i < arguments.Count; i++)
        {
            environment.Set(Declaration.Pars[i].Lexeme, arguments[i]);
        }

        object? val = interpreter.ExecuteBlock(Block, Declaration.Body.Statements, environment);
        return val;
    }
}