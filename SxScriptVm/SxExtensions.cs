namespace SxScriptVm;

public static class SxExtensions
{
    public static byte GetByte(this short c, int n)
    {
        return (byte)(c >> (8 * n));
    } 
    
    public static byte GetByte(this int c, int n)
    {
        return (byte)(c >> (8 * n));
    }

    public static byte[] GetBytes(this short n)
    {
        return BitConverter.GetBytes(n);
    }

    public static byte[] GetBytes(this int n)
    {
        return BitConverter.GetBytes(n);
    }

    public static short ToShort(this byte[] bytes)
    {
        return BitConverter.ToInt16(bytes);
    }

    public static int ToInt(this byte[] bytes)
    {
        return BitConverter.ToInt32(bytes);
    }

    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }
}