using System.Collections.Concurrent;
using SxScript.SxFFI;

namespace SxScript;

public class SxEnvironment
{
    public SxEnvironment? Enclosing { get; set; }
    public Dictionary<string, object?> Variables = new Dictionary<string, object?>();

    public SxEnvironment(SxEnvironment? enclosing)
    {
        Enclosing = enclosing;
    }

    void PerformSet(string name, object? value = null, SxToken? op = null, List<object?>? resolvedArrayExpression = null)
    {
        if (resolvedArrayExpression != null)
        {
            SxArrayHelper.PerformSetArray(Variables[name], name, value, op, resolvedArrayExpression);
            return;
        }
        
        Variables[name] = SxArithmetic.ResolveSetValue(Variables[name]!, value, op);
    }

    public void Set(string name, object? value = null, SxToken? op = null, List<object?>? resolvedArrayExpression = null)
    {
        if (Variables.ContainsKey(name))
        {
            PerformSet(name, value, op, resolvedArrayExpression);
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
    public void SetIfDefined(string name, object? value = null, SxToken? op = null, List<object?>? resolvedArrayExpression = null)
    {
        if (Variables.ContainsKey(name))
        {
            PerformSet(name, value, op, resolvedArrayExpression);
            return;
        }

        if (Enclosing == null)
        {
            // [todo] pokus o nastavení nedefinované proměnné
            Set(name, value, op, resolvedArrayExpression);
        }

        Enclosing?.SetIfDefined(name, value, op, resolvedArrayExpression);
    }

    public void SetAtIfDefined(int distance, string name, object? value = null, SxToken? op = null, List<object?>? resolvedArrayExpression = null)
    {
        Ancestor(distance).SetIfDefined(name, value, op, resolvedArrayExpression);
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