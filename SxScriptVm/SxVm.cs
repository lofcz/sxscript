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
    public Stack<object> Stack { get; set; }

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

        while (true)
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
                    PushStack(-(dynamic) PopStack());
                    break;
                }
                case (byte) OpCodes.OP_ADD:
                {
                    PushStack((dynamic)PopStack() + (dynamic)PopStack());
                    break;
                }
                case (byte) OpCodes.OP_SUBSTRACT:
                {
                    dynamic b = PopStack();
                    dynamic a = PopStack();
                    PushStack(a - b);
                    break;
                }
                case (byte) OpCodes.OP_DIVIDE:
                {
                    dynamic b = PopStack();
                    dynamic a = PopStack();
                    PushStack(a / b);
                    break;
                }
                case (byte) OpCodes.OP_MULTIPLY:
                {
                    PushStack((dynamic)PopStack() * (dynamic)PopStack());
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

    void PushStack(object value)
    {
        Stack.Push(value);
    }

    object PopStack()
    {
        return Stack.Pop();
    }

    object PeekStack()
    {
        return Stack.Peek();
    }
}