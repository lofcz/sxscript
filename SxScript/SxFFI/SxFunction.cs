using SxScript.SxStatements;

namespace SxScript.SxFFI;

public enum SxFunctionTypes
{
    None,
    Function,
    Method
}

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

    public SxFunction Bind(SxInstance instance)
    {
        SxEnvironment environment = new SxEnvironment(Closure);
        environment.DefineOrRedefineAndAssign("this", instance);
        return new SxFunction(Declaration, Block, environment);
    }
    
    public object? Call(SxInterpreter interpreter)
    {
        object? val = interpreter.ExecuteBlock(Block, Declaration.FunctionExpression.Body.Statements, LocalEnvironment);
        return val;
    }

    public async Task PrepareCallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments)
    {
        LocalEnvironment = new SxEnvironment(Closure);
        int firstNamed = 0;
        
        for (int i = 0; i < Declaration.FunctionExpression.Pars.Count; i++)
        {
            if (arguments?.Count > i && arguments?[i].Name != null)
            {
                firstNamed = i;
                break;
            }
            
            object? resolvedValue = arguments?.Count > i ? arguments[i].Value : await interpreter.EvaluateAsync(Declaration.FunctionExpression.Pars[i].DefaultValue);
            LocalEnvironment.Set(Declaration.FunctionExpression.Pars[i].Name.Lexeme, resolvedValue);
        }

        for (int i = firstNamed; i < arguments?.Count; i++)
        {
            if (arguments?[i].Name == null)
            {
                continue;
            }

            object? resolvedValue = arguments[i].Value;
            LocalEnvironment.Set(arguments[i]?.Name ?? "", resolvedValue);
        }
    }
}