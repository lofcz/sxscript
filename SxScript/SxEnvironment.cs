using System.Collections.Concurrent;

namespace SxScript;

public class SxEnvironment
{
    public SxEnvironment? Enclosing { get; set; }
    public Dictionary<string, object?> Variables = new Dictionary<string, object?>();

    public SxEnvironment(SxEnvironment? enclosing)
    {
        Enclosing = enclosing;
    }
    
    public void Set(string name, object? value = null)
    {
        if (Variables.ContainsKey(name))
        {
            Variables[name] = value;
            return;
        }
        
        Variables.Add(name, value);
    }

    public void DefineOrRedefineEmpty(string name)
    {
        Set(name, null);
    }

    public void DefineOrRedefineAndAssign(string name, object? value = null)
    {
        Set(name, value);
    }
    
    // [todo] vyřešit co tady
    public void SetIfDefined(string name, object? value = null)
    {
        if (Variables.ContainsKey(name))
        {
            Variables[name] = value;
            return;
        }

        if (Enclosing == null)
        {
            // [todo] pokus o nastavení nedefinované proměnné
            Set(name, value);
        }

        Enclosing?.SetIfDefined(name, value);
    }

    public void SetAtIfDefined(int distance, string name, object? value = null)
    {
        Ancestor(distance).SetIfDefined(name, value);
    }
    
    public void SetIfDefinedToSelf(string name, object? value = null)
    {
        if (Variables.ContainsKey(name))
        {
            Variables[name] = value;
            return;
        }

        if (Enclosing == null)
        {
            // [todo] pokus o nastavení nedefinované proměnné
            Set(name, value);
        }

        Enclosing?.SetIfDefined(name, value);
    }

    public object? Get(string name)
    {
        if (Variables.TryGetValue(name, out object? val))
        {
            return val;
        }

        return Enclosing?.Get(name);
    }

    public object? GetAt(int distance, string name)
    {
        return Ancestor(distance).Get(name);
    }

    SxEnvironment Ancestor(int distance)
    {
        SxEnvironment env = this;
        for (int i = 0; i < distance; i++)
        {
            if (env.Enclosing != null)
            {
                env = env.Enclosing;   
            }
            else
            {
                return env;
            }
        }

        return env;
    }
}