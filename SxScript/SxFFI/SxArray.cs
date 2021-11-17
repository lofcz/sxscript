namespace SxScript.SxFFI;

public class SxArray : SxInstance
{
    public Dictionary<object?, object?> IndexedValues { get; set; }

    public SxArray(SxClass? cls) : base(cls)
    {
        IndexedValues = new Dictionary<object?, object?>();
    }

    public object? GetValueChained(List<object?> keysChain)
    {
        SxArray lastDict = this;
        for (int i = 0; i < keysChain.Count; i++)
        {
            object? val = lastDict.GetValue(keysChain[i]);
            if (val == null)
            {
                return null;
            }

            if (val is SxArray dict)
            {
                lastDict = dict;
            }

            if (i == keysChain.Count - 1)
            {
                return val;
            }
        }

        return null;
    }

    public void SetChained(List<object?> keysChain, object? value)
    {
        SxArray lastDict = this;
        for (int i = 0; i < keysChain.Count; i++)
        {
            object? val = lastDict.GetValue(keysChain[i]);
            if (val == null)
            {
                lastDict.IndexedValues[keysChain[i]] = new Dictionary<object, object?> {{0, value}};
                val = lastDict.GetValue(keysChain[i]);
            }

            if (val is SxArray dict)
            {
                lastDict = dict;
            }

            if (i == keysChain.Count - 1)
            {
                if (lastDict.IndexedValues.ContainsKey(keysChain[i]))
                {
                    lastDict.IndexedValues[keysChain[i]] = value;
                    return;
                }
        
                lastDict.IndexedValues.Add(keysChain[i], value);
            }
        }
    }

    public void Set(object? key, object? value)
    {
        if (IndexedValues.ContainsKey(key))
        {
            IndexedValues[key] = value;
            return;
        }
        
        IndexedValues.Add(key, value);
    }

    public object? GetValue(object? key)
    {
        if (IndexedValues.ContainsKey(key))
        {
            return IndexedValues[key];
        }

        return null;
    }

    public override string ToString()
    {
        return string.Join(", ", IndexedValues);
    }

    public Task<object?> Call(SxInterpreter interpreter)
    {
        return null!;
    }

    public Task<object?> PrepareAndCallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments)
    {
        return null!;
    }

    public Task PrepareCallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments)
    {
        return null!;
    }
    
    public override object? Get(SxToken name)
    {
        if (name.Lexeme == "length")
        {
            return IndexedValues.Count;
        }
        
        return null;
    }
}