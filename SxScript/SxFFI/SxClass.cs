namespace SxScript.SxFFI;

public class SxClass : SxInstance, SxExpression.ISxCallable
{
    public string Name { get; set; }
    public Dictionary<string, SxFunction> Methods { get; set; }
    public List<SxResolvedCallArgument> Arguments { get; set; }
    public Dictionary<string, object> Fields { get; set; }
    public SxClass? Superclass { get; set; }
    
    public SxClass(SxClass metaclass, SxClass? superclass, string name, Dictionary<string, SxFunction> methods, Dictionary<string, object> fields) : base(metaclass)
    {
        Name = name;
        Methods = methods;
        Fields = fields;
        Superclass = superclass;
    }

    public override string ToString()
    {
        return Name;
    }

    public async Task<object?> Call(SxInterpreter interpreter)
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

        if (Superclass != null)
        {
            return Superclass.FindMethod(name);
        }

        return null;
    }

    public async Task PrepareCallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments)
    {
        Arguments = arguments;
    }
}