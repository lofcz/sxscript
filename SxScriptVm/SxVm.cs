using System.Text;

namespace SxScriptVm;

public enum SxInterpretResults
{
    Ok,
    CompileError,
    RuntimeError
}

public class SxVmInterpretResult 
{
    public SxInterpretResults Result { get; set; }
    public string StdOut { get; set; }
    public string StdErr { get; set; }
    public string Bytecode { get; set; }
}

public class SxVm
{
    public SxChunk Chunk { get; set; }
    public int Ip { get; set; }
    public StringBuilder StdOutput { get; set; }
    public Stack<object?> Stack { get; set; }
    public bool RuntimeOk { get; set; }
    public bool PrintLastExpr { get; set; }
    public Dictionary<string, object?> Globals { get; set; }

    public SxVmInterpretResult Interpret(string code)
    {
        SxCompiler compiler = new SxCompiler();
        return Interpret(compiler.Compile(code));
    }

    public SxVmInterpretResult Interpret(SxChunk chunk)
    {
        SxVmInterpretResult il = new SxVmInterpretResult();
        
        if (chunk.HasCompileErrors)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SxError err in chunk.CompileErrors)
            {
                sb.Append($"řádek {err.Line}: {err.Msg}\n");
            }

            il.Result = SxInterpretResults.CompileError;
            il.StdErr = sb.ToString();
            return il;
        }
        
        il = InterpretInternal(chunk);
        il.StdOut = StdOutput.ToString();
        il.Bytecode = Chunk.Log();
        return il;
    }
    
    private SxVmInterpretResult InterpretInternal(SxChunk chunk)
    {
        SxVmInterpretResult result = new SxVmInterpretResult();
        Chunk = chunk;
        Ip = 0;
        StdOutput = new StringBuilder();
        Stack = new Stack<object?>(256);
        RuntimeOk = true;
        PrintLastExpr = false;
        Globals = new Dictionary<string, object?>();

        while (RuntimeOk)
        {
            byte instruction = Chunk.OpCodes[Ip];

            switch (instruction)
            {
                case (byte) OpCodes.OP_RETURN:
                {
                    if (PrintLastExpr)
                    {
                        PrintLn(Pop());   
                    }
                    
                    result.Result = SxInterpretResults.Ok;
                    return result;
                }
                case (byte) OpCodes.OP_CONSTANT_8:
                {
                    byte b = ReadByte();
                    object val = Chunk.Constants[b];
                    Push(val);
                    break;
                }
                case (byte) OpCodes.OP_CONSTANT_16:
                {
                    break;
                }
                case (byte) OpCodes.OP_CONSTANT_32:
                {
                    break;
                }
                case (byte) OpCodes.OP_NEGATE:
                { 
                    object? obj = Pop();

                    if (obj == null)
                    {
                        RuntimeError($"Negaci není možné provést na null");
                    }
                    else if (obj.IsNumber())
                    {
                        Push(-(dynamic)obj);   
                    }
                    else if (obj is bool b)
                    {
                        Push(!b);
                    }
                    else
                    {
                        RuntimeError($"Negaci není možné provést na typu {obj.GetType()}");
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_ADD:
                {
                    object b = Pop() ?? 0;
                    object a = Pop() ?? 0;

                    if (a.IsNumber() && b.IsNumber())
                    {
                        Push((dynamic)a + (dynamic)b);
                    }
                    else if (a.IsString() && b.IsString())
                    {
                        Push((dynamic)a + (dynamic)b);
                    }
                    else if (a.ConvertibleToNumber() && b.ConvertibleToNumber())
                    {
                        a = a.ToNumber() ?? 0;
                        b = b.ToNumber() ?? 0;
                        
                        Push((dynamic)a + (dynamic)b);
                    }
                    else if (a.IsString() || b.IsString())
                    {
                        Push(a.ToString() + b);
                    }
                    else
                    {
                        if (!a.ConvertibleToNumber())
                        {
                            RuntimeError("Levá strana operátoru + není číslo a nelze na číslo převést");
                        }
                        else if (!b.ConvertibleToNumber())
                        {
                            RuntimeError("Pravá strana operátoru + není číslo a nelze na číslo převést");
                        }
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_SUBSTRACT:
                {
                    dynamic? b = Pop();
                    dynamic? a = Pop();
                    Push(a - b);
                    break;
                }
                case (byte) OpCodes.OP_DIVIDE:
                {
                    dynamic? b = Pop();
                    dynamic? a = Pop();
                    Push(a / b);
                    break;
                }
                case (byte) OpCodes.OP_MULTIPLY:
                {
                    Push((dynamic)Pop() * (dynamic)Pop());
                    break;
                }
                case (byte) OpCodes.OP_TRUE:
                {
                    Push(true);
                    break;
                }
                case (byte) OpCodes.OP_FALSE:
                {
                    Push(false);
                    break;
                }
                case (byte) OpCodes.OP_NULL:
                {
                    Push(null);
                    break;
                }
                case (byte) OpCodes.OP_NOT:
                {
                    object? obj = Pop();
                    if (obj == null)
                    {
                        Push(true);
                    }
                    else if (obj is bool b)
                    {
                        Push(!b);
                    }
                    else if (obj.IsNumber())
                    {
                        Push((dynamic) obj == 0);
                    }

                    break;
                }
                case (byte) OpCodes.OP_EQUAL:
                {
                    object? b = Pop();
                    object? a = Pop();

                    bool aNull = a == null;
                    bool bNull = b == null;

                    if (aNull && bNull)
                    {
                        Push(true);
                        break;
                    }

                    if (aNull || bNull)
                    {
                        Push(false);
                        break;
                    }

                    bool eq = a!.Equals(b);
                    Push(eq);
                    break;
                }
                case (byte) OpCodes.OP_NOT_EQUAL:
                {
                    object? b = Pop();
                    object? a = Pop();
                    Push(a != b);
                    break;
                }
                case (byte) OpCodes.OP_GREATER:
                {
                    object? b = Pop();
                    object? a = Pop();

                    if (a == null || b == null)
                    {
                        RuntimeError("Není možné použít >, protože jedna strana je null");
                    }
                    else if (a.IsNumber() && b.IsNumber())
                    {
                        Push((dynamic)a > (dynamic)b);   
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_LESS:
                {
                    object? b = Pop();
                    object? a = Pop();

                    if (a == null || b == null)
                    {
                        RuntimeError("Není možné použít <, protože jedna strana je null");
                    }
                    else if (a.IsNumber() && b.IsNumber())
                    {
                        Push((dynamic)a < (dynamic)b);   
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_EQUAL_GREATER:
                {
                    object? b = Pop();
                    object? a = Pop();

                    if (a == null || b == null)
                    {
                        RuntimeError("Není možné použít >=, protože jedna strana je null");
                    }
                    else if (a.IsNumber() && b.IsNumber())
                    {
                        Push((dynamic)a >= (dynamic)b);   
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_EQUAL_LESS:
                {
                    object? b = Pop();
                    object? a = Pop();

                    if (a == null || b == null)
                    {
                        RuntimeError("Není možné použít <=, protože jedna strana je null");
                    }
                    else if (a.IsNumber() && b.IsNumber())
                    {
                        Push((dynamic)a <= (dynamic)b);   
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_CONSTANT_INT_ZERO:
                {
                    Push(0);
                    break;
                }
                case (byte) OpCodes.OP_CONSTANT_INT_MINUS_ONE:
                {
                    Push(-1);
                    break;
                }
                case (byte) OpCodes.OP_CONSTANT_INT_ONE:
                {
                    Push(1);
                    break;
                }
                case (byte) OpCodes.OP_CONSTANT_INT_TWO:
                {
                    Push(2);
                    break;
                }
                case (byte) OpCodes.OP_PRINT:
                {
                    PrintLn(Pop());
                    break;
                }
                case (byte) OpCodes.OP_POP:
                {
                    Pop();
                    break;
                }
                case (byte) OpCodes.OP_DEFINE_GLOBAL_8:
                {
                    object? val = Pop();
                    object? keyIndexVal = Pop();
                    byte b = ReadByte();

                    if (keyIndexVal is string key)
                    {
                        if (Globals.ContainsKey(key))
                        {
                            Globals[key] = val;
                        }
                        else
                        {
                            Globals.Add(key, val);
                        }
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_GET_GLOBAL_8:
                {
                    object? keyIndexVal = Pop();
                    object? val = null;
                    byte b = ReadByte();
                    
                    if (keyIndexVal is string key)
                    {
                        if (Globals.ContainsKey(key))
                        {
                            val = Globals[key];
                        }
                        else
                        {
                            // nedefinovaná proměnná
                            Globals.Add(key, val);
                        }
                    }

                    Push(val);
                    break;
                }
                case (byte) OpCodes.OP_SET_GLOBAL_8:
                {
                    object? val = Pop();
                    object? keyIndexVal = Peek();
                    byte b = ReadByte();

                    if (keyIndexVal is string key)
                    {
                        if (Globals.ContainsKey(key))
                        {
                            Globals[key] = val;
                        }
                        else
                        {
                            Globals.Add(key, val);
                        }
                    }
                    
                    break;
                }
                default:
                {
                    break;
                }
            }

            Ip++;
        }

        result.Result = SxInterpretResults.Ok;
        return result;
    }

    void RuntimeError(string msg)
    {
        RuntimeOk = false;
        StdOutput.Append($"{msg}\n");
    }

    byte ReadByte()
    {
        Ip++;
        return Chunk.OpCodes[Ip];
    }

    void Print(object val)
    {
        StdOutput.Append(val);
    }

    void PrintLn(object? val)
    {
        StdOutput.Append($"{val}\n");
    }

    void PrintStackToStdOut()
    {
        StdOutput.Append(PrintStack());
    }

    string PrintStack()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        
        foreach (object item in Stack)
        {
            sb.Append(item);
        }

        sb.Append("]\n");

        return sb.ToString();
    }

    void Push(object? value)
    {
        Stack.Push(value);
    }

    object? Pop()
    {
        return Stack.Pop();
    }
    
    object? Peek(int distance = 0)
    {
        if (distance == 0)
        {
            return Stack.Peek();
        }
        
        return Stack.Skip(distance).FirstOrDefault();
    }
}