using System.Collections.Concurrent;

namespace SxScript;

public class SxEnvironment
{
    public Dictionary<string, object?> Variables = new Dictionary<string, object?>();

    public void Set(string name, object? value = null)
    {
        if (Variables.ContainsKey(name))
        {
            Variables[name] = value;   
        }
        else
        {
            Variables.Add(name, value);
        }
    }

    public object? Get(string name)
    {
        if (Variables.TryGetValue(name, out object? val))
        {
            return val;
        }

        return null;
    }
}