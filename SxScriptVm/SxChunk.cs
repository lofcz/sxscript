using System.Text;
namespace SxScriptVm;

public enum OpCodes : byte
{
    OP_RETURN,
    OP_CONSTANT_8,
    OP_CONSTANT_16,
    OP_CONSTANT_32,
    OP_NEGATE,
    OP_ADD,
    OP_SUBSTRACT,
    OP_MULTIPLY,
    OP_DIVIDE
}

public class SxChunk
{
    public List<byte> OpCodes { get; set; } = new List<byte>();
    public List<object> Constants { get; set; } = new List<object>();
    public List<SxLine> Lines { get; set; } = new List<SxLine>();
    private int CurrentLineCounter = 0;

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
                    sb.Append($"OP_CONSTANT_8\n");
                    object constant = Constants[OpCodes[i + 1]];
                    i++;
                    sb.Append($"{i.ToString("0000")} {constant}");
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
                default:
                {
                    sb.Append($"Neznámá instrukce - {OpCodes[i]}");
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
        CurrentLineCounter += 2;
        OpCodes.Add(codes.GetByte(0));
        OpCodes.Add(codes.GetByte(1));
        return OpCodes.Count;
    }
    
    public int PushOpCode(int codes)
    {
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

    public int PushConstant(object constant)
    {
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