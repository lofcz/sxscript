namespace SxScript.SxFFI;

public class SxClass : SxExpression.ISxCallable
{
    public string Name { get; set; }
    public Dictionary<string, SxFunction> Methods { get; set; }

    public SxClass(string name, Dictionary<string, SxFunction> methods)
    {
        Name = name;
        Methods = methods;
    }

    public override string ToString()
    {
        return Name;
    }

    public object? Call(SxInterpreter interpreter)
    {
        SxInstance instance = new SxInstance(this);
        return instance;
    }

    public async Task PrepareCallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments)
    {
        return;
    }
}