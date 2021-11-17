using System.Text;
using SxScriptVm;

namespace SxScriptVmRepl;

public class Program
{
    public static void Main()
    {
        while (true)
        {
            Repl();
        }
        
        SxChunk chunk = new SxChunk();
        chunk.PushConstant(1);
        chunk.PushOpCode(OpCodes.OP_NEGATE);
        chunk.PushConstant(5);
        chunk.PushOpCode(OpCodes.OP_ADD);
        chunk.PushOpCode(OpCodes.OP_RETURN);
        chunk.PushLine();

        SxVm vm = new SxVm();
        vm.Interpret(chunk);
        
        Console.WriteLine("Bytekód:");
        Console.WriteLine("----------------");
        Console.WriteLine(chunk.Log());
        
        Console.WriteLine("Výstup:");
        Console.WriteLine("----------------");
        Console.WriteLine(vm.StdOutput);
        
        Console.ReadKey();
    }

    public static void Repl()
    {
        Console.Clear();
        StringBuilder sb = new StringBuilder();
        Console.WriteLine("Napiš program, zadání ukončíš řádkou \"end\"");
        while (true)
        {
            Console.Write("> ");
            string str = Console.ReadLine();

            if (str == "end")
            {
                break;
            }
            
            sb.Append(str);
        }

        string input = sb.ToString();
        SxVm vm = new SxVm();
        SxVmInterpretResult result = vm.Interpret(input);
        
        Console.WriteLine("bytekód:");
        Console.WriteLine(result.Bytecode);
        Console.WriteLine("stdout:");
        Console.WriteLine(result.StdOut);
        Console.ReadKey();
    }
}