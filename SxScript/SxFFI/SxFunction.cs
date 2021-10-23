using SxScript.SxStatements;

namespace SxScript.SxFFI;

public enum SxFunctionTypes
{
    None,
    Function,
    Method,
    Constructor
}

public enum SxClassTypes
{
    None,
    Class,
    Subclass
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
    public bool IsContructor { get; set; }

    public SxFunction(SxFunctionStatement declaration, SxBlockStatement block, SxEnvironment closure, bool isContructor)
    {
        Declaration = declaration;
        Block = block;
        Return = false;
        Statement = block;
        Await = true;
        Closure = closure;
        IsContructor = isContructor;
    }

    public SxFunction Bind(SxInstance instance)
    {
        SxEnvironment environment = new SxEnvironment(Closure);
        environment.DefineOrRedefineAndAssign("this", instance);
        return new SxFunction(Declaration, Block, environment, IsContructor);
    }
    
    public async Task<object?> Call(SxInterpreter interpreter)
    {
        object? val = await interpreter.ExecuteBlockAsync(Block, Declaration.FunctionExpression.Body.Statements, LocalEnvironment);

        if (IsContructor)
        {
            return Closure.GetAt(0, "this");
        }
        
        return val;
    }

    public async Task<object?> PrepareAndCallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments)
    {
        await PrepareCallAsync(interpreter, arguments);
        return await Call(interpreter);
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

        object? rfThis = Closure.GetAt(0, "this");
        if (rfThis != null)
        {
            LocalEnvironment.Set("this", rfThis);
        }
    }
}