namespace SxScriptVm;

public class SxError
{
    public string Msg { get; set; }
    public int Line { get; set; }

    public SxError(string msg, int line)
    {
        Msg = msg;
        Line = line;
    }
}