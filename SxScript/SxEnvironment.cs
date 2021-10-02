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

    public object? Get(string name)
    {
        if (Variables.TryGetValue(name, out object? val))
        {
            return val;
        }

        return Enclosing?.Get(name);
    }
}