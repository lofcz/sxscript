using System.Text;
namespace SxScriptVm;

public enum OpCodes : byte
{
    OP_RETURN = 0,
    OP_CONSTANT_8 = 1,
    OP_CONSTANT_16 = 2,
    OP_CONSTANT_32 = 3,
    OP_NEGATE = 4,
    OP_ADD = 5,
    OP_SUBSTRACT = 6,
    OP_MULTIPLY = 7,
    OP_DIVIDE = 8,
    OP_NULL = 9,
    OP_TRUE = 10,
    OP_FALSE = 11,
    OP_NOT = 12,
    OP_EQUAL = 13,
    OP_GREATER = 14,
    OP_LESS = 15,
    OP_NOT_EQUAL = 16,
    OP_EQUAL_GREATER = 17,
    OP_EQUAL_LESS = 18,
    OP_CONSTANT_INT_ZERO = 19,
    OP_CONSTANT_INT_ONE = 20,
    OP_CONSTANT_INT_TWO = 21,
    OP_CONSTANT_INT_MINUS_ONE = 22,
    OP_PRINT = 23,
    OP_POP = 24,
    OP_DEFINE_GLOBAL_8 = 25,
    OP_DEFINE_GLOBAL_16 = 26,
    OP_DEFINE_GLOBAL_32 = 27,
    OP_GET_GLOBAL_8 = 28,
    OP_GET_GLOBAL_16 = 29,
    OP_GET_GLOBAL_32 = 30,
    OP_SET_GLOBAL_8 = 31,
    OP_SET_GLOBAL_16 = 32,
    OP_SET_GLOBAL_32 = 33
}

public class SxChunk
{
    public List<byte> OpCodes { get; set; } = new List<byte>();
    public List<object> Constants { get; set; } = new List<object>();
    public List<SxLine> Lines { get; set; } = new List<SxLine>();
    private int CurrentLineCounter = 0;
    public List<SxError> CompileErrors { get; set; }
    public bool HasCompileErrors { get; set; }

    public SxChunk()
    {
        
    }

    public string Log()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < OpCodes.Count; i++)
        {
            sb.Append(i.ToString("0000"));
            sb.Append(" ");
            
            switch (OpCodes[i])
            {
                case (byte)SxScriptVm.OpCodes.OP_RETURN:
                {
                    sb.Append("OP_RETURN");
                    break;
                }
                case (byte) SxScriptVm.OpCodes.OP_CONSTANT_8:
                {
                    sb.Append($"OP_CONSTANT_8        | ");
                    object constant = Constants[OpCodes[i + 1]];
                    i++;
                    sb.Append($"{constant}");
                    break;
                }
                case (byte) SxScriptVm.OpCodes.OP_CONSTANT_16:
                {
                    sb.Append($"OP_CONSTANT_16\n");
                    byte b1 = OpCodes[i];
                    i++;
                    sb.Append($"{i.ToString("0000")} -");
                    byte b2 = OpCodes[i];
                    i++;
                    object constant = Constants[new[] {b1, b2}.ToShort()];
                    sb.Append($"{i.ToString("0000")} {constant}");
                    break;
                }
                case (byte) SxScriptVm.OpCodes.OP_CONSTANT_32:
                {
                    sb.Append($"OP_CONSTANT_32\n");
                    byte b1 = OpCodes[i];
                    i++;
                    sb.Append($"{i.ToString("0000")} -");
                    byte b2 = OpCodes[i];
                    i++;
                    sb.Append($"{i.ToString("0000")} -");
                    byte b3 = OpCodes[i];
                    i++;
                    sb.Append($"{i.ToString("0000")} -");
                    byte b4 = OpCodes[i];
                    i++;
                    object constant = Constants[new[] {b1, b2, b3, b4}.ToInt()];
                    sb.Append($"{i.ToString("0000")} {constant}");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_NEGATE:
                {
                    sb.Append("OP_NEGATE");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_ADD:
                {
                    sb.Append("OP_ADD");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_DIVIDE:
                {
                    sb.Append("OP_DIVIDE");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_SUBSTRACT:
                {
                    sb.Append("OP_SUBSTRACT");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_MULTIPLY:
                {
                    sb.Append("OP_MULTIPLY");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_TRUE:
                {
                    sb.Append("OP_TRUE");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_FALSE:
                {
                    sb.Append("OP_FALSE");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_NULL:
                {
                    sb.Append("OP_NULL");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_NOT:
                {
                    sb.Append("OP_NOT");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_EQUAL:
                {
                    sb.Append("OP_EQUAL");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_NOT_EQUAL:
                {
                    sb.Append("OP_NOT_EQUAL");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_GREATER:
                {
                    sb.Append("OP_GREATER");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_LESS:
                {
                    sb.Append("OP_LESS");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_EQUAL_GREATER:
                {
                    sb.Append("OP_EQUAL_GREATER");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_EQUAL_LESS:
                {
                    sb.Append("OP_EQUAL_LESS");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_CONSTANT_INT_ZERO:
                {
                    sb.Append("OP_CONSTANT_INT_ZERO");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_CONSTANT_INT_ONE:
                {
                    sb.Append("OP_CONSTANT_INT_ONE");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_CONSTANT_INT_TWO:
                {
                    sb.Append("OP_CONSTANT_INT_TWO");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_CONSTANT_INT_MINUS_ONE:
                {
                    sb.Append("OP_CONSTANT_INT_MINUS_ONE");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_PRINT:
                {
                    sb.Append("OP_PRINT");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_POP:
                {
                    sb.Append("OP_POP");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_DEFINE_GLOBAL_8:
                {
                    sb.Append("OP_DEFINE_GLOBAL_8   | ");
                    object constant = Constants[OpCodes[i + 1]];
                    i++;
                    sb.Append($"{constant}");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_GET_GLOBAL_8:
                {
                    sb.Append("OP_GET_GLOBAL_8      | ");
                    object constant = Constants[OpCodes[i + 1]];
                    i++;
                    sb.Append($"{constant}");
                    break;
                }
                default:
                {
                    sb.Append($"Neznámá instrukce - {OpCodes[i]}");
                    break;
                }
                case (byte)SxScriptVm.OpCodes.OP_SET_GLOBAL_8:
                {
                    sb.Append("OP_SET_GLOBAL_8      | ");
                    object constant = Constants[OpCodes[i + 1]];
                    i++;
                    sb.Append($"{constant}");
                    break;
                }
            }

            sb.Append("\n");
        }

        return sb.ToString();
    }

    public int PushOpCode(OpCodes code)
    {
        CurrentLineCounter++;
        OpCodes.Add((byte)code);
        return OpCodes.Count;
    }
    
    public int PushOpCode(byte code)
    {
        CurrentLineCounter++;
        OpCodes.Add(code);
        return OpCodes.Count;
    }
    
    public int PushOpCode(short codes)
    {
        if (codes <= byte.MaxValue)
        {
            return PushOpCode((byte) codes);
        }
        
        CurrentLineCounter += 2;
        OpCodes.Add(codes.GetByte(0));
        OpCodes.Add(codes.GetByte(1));
        return OpCodes.Count;
    }
    
    public int PushOpCode(int codes)
    {
        switch (codes)
        {
            case <= byte.MaxValue:
            {
                return PushOpCode((byte) codes);
            }
            case <= short.MaxValue:
            {
                return PushOpCode((short) codes);
            }
        }

        CurrentLineCounter += 4;
        OpCodes.Add(codes.GetByte(0));
        OpCodes.Add(codes.GetByte(1));
        OpCodes.Add(codes.GetByte(2));
        OpCodes.Add(codes.GetByte(3));
        return OpCodes.Count;
    }

    public int PushLine()
    {
        Lines.Add(new SxLine(Lines.Count, CurrentLineCounter));
        CurrentLineCounter = 0;
        return Lines.Count;
    }

    public int GetLine(int index)
    {
        int currentIndex = 0;
        int step = 0;
        
        while (true)
        {
            if (index <= currentIndex)
            {
                return step;
            }

            currentIndex += Lines[step].Offset;
            step++;
        }
    }

    public object GetConstant(int index)
    {
        return Constants[index];
    }

    public int PushConstant(object constant)
    {
        if (constant.IsNumber())
        {
            if ((int) constant == -1)
            {
                PushOpCode(SxScriptVm.OpCodes.OP_CONSTANT_INT_MINUS_ONE);
                return 0;
            }
            
            if ((int) constant == 0)
            {
                PushOpCode(SxScriptVm.OpCodes.OP_CONSTANT_INT_ZERO);
                return 0;
            }
            
            if ((int) constant == 1)
            {
                PushOpCode(SxScriptVm.OpCodes.OP_CONSTANT_INT_ONE);
                return 0;
            }
            
            if ((int) constant == 2)
            {
                PushOpCode(SxScriptVm.OpCodes.OP_CONSTANT_INT_TWO);
                return 0;
            }
        }
        
        Constants.Add(constant);
        int index = Constants.Count - 1;
        
        switch (Constants.Count)
        {
            case < byte.MaxValue:
            {
                PushOpCode(SxScriptVm.OpCodes.OP_CONSTANT_8);
                PushOpCode((byte)index);
                break;
            }
            case < short.MaxValue:
            {
                PushOpCode(SxScriptVm.OpCodes.OP_CONSTANT_16);
                PushOpCode((short)index);
                break;
            }
            default:
            {
                PushOpCode(SxScriptVm.OpCodes.OP_CONSTANT_16);
                PushOpCode(index);
                break;
            }
        }

        return index;
    }
}

public class SxLine
{
    public int Index { get; set; }
    public int Offset { get; set; }

    public SxLine(int index, int offset)
    {
        Index = index;
        Offset = offset;
    }
}