using SxScript.SxStatements;

namespace SxScript.SxFFI;

public class SxFunction : SxExpression.ISxCallable, SxStatement.ISxCallStatement
{
    public SxFunctionStatement Declaration { get; set; }
    public SxBlockStatement Block { get; set; }
    public bool Return { get; set; }
    public SxStatement Statement { get; set; }
    public bool Await { get; set; }
    public SxEnvironment LocalEnvironment { get; set; }
    public SxEnvironment Closure { get; set; }

    public SxFunction(SxFunctionStatement declaration, SxBlockStatement block, SxEnvironment closure)
    {
        Declaration = declaration;
        Block = block;
        Return = false;
        Statement = block;
        Await = true;
        Closure = closure;
    }
    
    public object? Call(SxInterpreter interpreter)
    {
        object? val = interpreter.ExecuteBlock(Block, Declaration.Body.Statements, LocalEnvironment);
        return val;
    }

    public async Task PrepareCallAsync(SxInterpreter interpreter, List<object> arguments)
    {
        LocalEnvironment = new SxEnvironment(Closure);
        for (int i = 0; i < Declaration.Pars.Count; i++)
        {
            object? resolvedValue = arguments?.Count > i ? arguments[i] : await interpreter.EvaluateAsync(Declaration.Pars[i].DefaultValue);
            LocalEnvironment.Set(Declaration.Pars[i].Name.Lexeme, resolvedValue);
        }
    }
}