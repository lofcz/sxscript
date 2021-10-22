namespace SxScript.SxFFI;

public class SxClass : SxInstance, SxExpression.ISxCallable
{
    public string Name { get; set; }
    public Dictionary<string, SxFunction> Methods { get; set; }
    public List<SxResolvedCallArgument> Arguments { get; set; }
    public Dictionary<string, object> Fields { get; set; }
    
    public SxClass(SxClass metaclass, string name, Dictionary<string, SxFunction> methods, Dictionary<string, object> fields) : base(metaclass)
    {
        Name = name;
        Methods = methods;
        Fields = fields;
    }

    public override string ToString()
    {
        return Name;
    }

    public object? Call(SxInterpreter interpreter)
    {
        return null!;
    }

    public async Task<object?> PrepareAndCallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments)
    {
        SxInstance instance = new SxInstance(this);
        SxFunction? contructor = FindMethod(Name);

        if (contructor != null)
        {
            contructor = contructor.Bind(instance);
            await contructor.PrepareAndCallAsync(interpreter, arguments);
        }
        
        return instance;
    }

    public SxFunction? FindMethod(string name)
    {
        if (Methods.ContainsKey(name))
        {
            return Methods[name];
        }

        return null;
    }

    public async Task PrepareCallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments)
    {
        Arguments = arguments;
    }
}