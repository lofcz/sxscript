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
    public string Bytecode { get; set; }
}

public class SxVm
{
    public SxChunk Chunk { get; set; }
    public int Ip { get; set; }
    public StringBuilder StdOutput { get; set; }
    public Stack<object?> Stack { get; set; }
    public bool RuntimeOk { get; set; }

    public SxVmInterpretResult Interpret(string code)
    {
        SxCompiler compiler = new SxCompiler();
        return Interpret(compiler.Compile(code));
    }

    public SxVmInterpretResult Interpret(SxChunk chunk)
    {
        SxVmInterpretResult il = InterpretInternal(chunk);
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
        Stack = new Stack<object>(256);
        RuntimeOk = true;

        while (RuntimeOk)
        {
            byte instruction = Chunk.OpCodes[Ip];

            switch (instruction)
            {
                case (byte) OpCodes.OP_RETURN:
                {
                    PrintLn(PopStack());
                    result.Result = SxInterpretResults.Ok;
                    return result;
                }
                case (byte) OpCodes.OP_CONSTANT_8:
                {
                    byte b = ReadByte();
                    object val = Chunk.Constants[b];
                    PushStack(val);
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
                    object? obj = PopStack();

                    if (obj == null)
                    {
                        RuntimeError($"Negaci není možné provést na null");
                    }
                    else if (obj.IsNumber())
                    {
                        PushStack(-(dynamic)obj);   
                    }
                    else if (obj is bool b)
                    {
                        PushStack(!b);
                    }
                    else
                    {
                        RuntimeError($"Negaci není možné provést na typu {obj.GetType()}");
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_ADD:
                {
                    object b = PopStack() ?? 0;
                    object a = PopStack() ?? 0;

                    if (a.IsNumber() && b.IsNumber())
                    {
                        PushStack((dynamic)a + (dynamic)b);
                    }
                    else if (a.ConvertibleToNumber() && b.ConvertibleToNumber())
                    {
                        a = a.ToNumber() ?? 0;
                        b = b.ToNumber() ?? 0;
                        
                        PushStack((dynamic)a + (dynamic)b);
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
                    dynamic? b = PopStack();
                    dynamic? a = PopStack();
                    PushStack(a - b);
                    break;
                }
                case (byte) OpCodes.OP_DIVIDE:
                {
                    dynamic? b = PopStack();
                    dynamic? a = PopStack();
                    PushStack(a / b);
                    break;
                }
                case (byte) OpCodes.OP_MULTIPLY:
                {
                    PushStack((dynamic)PopStack() * (dynamic)PopStack());
                    break;
                }
                case (byte) OpCodes.OP_TRUE:
                {
                    PushStack(true);
                    break;
                }
                case (byte) OpCodes.OP_FALSE:
                {
                    PushStack(false);
                    break;
                }
                case (byte) OpCodes.OP_NULL:
                {
                    PushStack(null);
                    break;
                }
                case (byte) OpCodes.OP_NOT:
                {
                    object? obj = PopStack();
                    if (obj == null)
                    {
                        PushStack(true);
                    }
                    else if (obj is bool b)
                    {
                        PushStack(!b);
                    }
                    else if (obj.IsNumber())
                    {
                        PushStack((dynamic) obj == 0);
                    }

                    break;
                }
                case (byte) OpCodes.OP_EQUAL:
                {
                    object? b = PopStack();
                    object? a = PopStack();
                    PushStack(a == b);
                    break;
                }
                case (byte) OpCodes.OP_NOT_EQUAL:
                {
                    object? b = PopStack();
                    object? a = PopStack();
                    PushStack(a != b);
                    break;
                }
                case (byte) OpCodes.OP_GREATER:
                {
                    object? b = PopStack();
                    object? a = PopStack();

                    if (a == null || b == null)
                    {
                        RuntimeError("Není možné použít >, protože jedna strana je null");
                    }
                    else if (a.IsNumber() && b.IsNumber())
                    {
                        PushStack((dynamic)a > (dynamic)b);   
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_LESS:
                {
                    object? b = PopStack();
                    object? a = PopStack();

                    if (a == null || b == null)
                    {
                        RuntimeError("Není možné použít <, protože jedna strana je null");
                    }
                    else if (a.IsNumber() && b.IsNumber())
                    {
                        PushStack((dynamic)a < (dynamic)b);   
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_EQUAL_GREATER:
                {
                    object? b = PopStack();
                    object? a = PopStack();

                    if (a == null || b == null)
                    {
                        RuntimeError("Není možné použít >=, protože jedna strana je null");
                    }
                    else if (a.IsNumber() && b.IsNumber())
                    {
                        PushStack((dynamic)a >= (dynamic)b);   
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_EQUAL_LESS:
                {
                    object? b = PopStack();
                    object? a = PopStack();

                    if (a == null || b == null)
                    {
                        RuntimeError("Není možné použít <=, protože jedna strana je null");
                    }
                    else if (a.IsNumber() && b.IsNumber())
                    {
                        PushStack((dynamic)a <= (dynamic)b);   
                    }
                    
                    break;
                }
                case (byte) OpCodes.OP_CONSTANT_INT_ZERO:
                {
                    PushStack(0);
                    break;
                }
                case (byte) OpCodes.OP_CONSTANT_INT_MINUS_ONE:
                {
                    PushStack(-1);
                    break;
                }
                case (byte) OpCodes.OP_CONSTANT_INT_ONE:
                {
                    PushStack(1);
                    break;
                }
                case (byte) OpCodes.OP_CONSTANT_INT_TWO:
                {
                    PushStack(2);
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

    void PrintLn(object val)
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

    void PushStack(object? value)
    {
        Stack.Push(value);
    }

    object? PopStack()
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