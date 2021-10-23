namespace SxScript.SxFFI;

public class SxInstance
{
    public SxClass Class { get; set; }
    public Dictionary<string, object> Fields { get; set; }

    public SxInstance(SxClass? cls)
    {
        if (cls != null)
        {
            Class = cls;
            Fields = new Dictionary<string, object>(cls.Fields);   
        }
    }

    public void Set(SxToken name, object value)
    {
        if (Fields.ContainsKey(name.Lexeme))
        {
            Fields[name.Lexeme] = value;
        }
        else
        {
            Fields.Add(name.Lexeme, value);
        }
    }

    public object? Get(SxToken name)
    {
        if (Fields.TryGetValue(name.Lexeme, out object? val))
        {
            return val;
        }

        SxFunction? fn = FindMethod(name.Lexeme);;
        if (fn != null)
        {
            return fn.Bind(this);
        }

        if (Class.Superclass != null)
        {
            return Class.Superclass.Get(name);
        }

        // [todo] nedefinovaná vlastnost
        return null;
    }

    SxFunction? FindMethod(string name)
    {
        if (Class.Methods.ContainsKey(name))
        {
            return Class.Methods[name];
        }

        if (Class.Superclass != null)
        {
            if (Class.Superclass.Methods.ContainsKey(name))
            {
                return Class.Superclass.Methods[name];
            }
        }

        return null;
    }

    public override string ToString()
    {
        return $"instance of {Class}";
    }
}