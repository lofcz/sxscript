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
    
    public static bool IsNumber(this object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        
        switch (obj)
        {
            case sbyte _: return true;
            case byte _: return true;
            case short _: return true;
            case ushort _: return true;
            case int _: return true;
            case uint _: return true;
            case long _: return true;
            case ulong _: return true;
            case float _: return true;
            case double _: return true;
            case decimal _: return true;
            case char _: return true;
        }

        return false;
    }

    public static bool ConvertibleToNumber(this object? obj)
    {
        if (IsNumber(obj))
        {
            return true;
        }
        
        switch (obj)
        {
            case bool _: return true;
        }

        return false;
    }
    
    public static object? ToNumber(this object? obj)
    {
        if (obj == null)
        {
            return obj;
        }
        
        if (IsNumber(obj))
        {
            return obj;
        }

        if (obj is bool b)
        {
            return b ? 1 : 0;
        }

        if (obj is char c)
        {
            return (int) c;
        }

        return obj;
    }

    public static bool IsNumeric(this object? x) { return (x != null && IsNumeric(x.GetType())); }
    public static bool IsNumeric(Type type) { return IsNumeric(type, Type.GetTypeCode(type)); }
    public static bool IsNumeric(Type type, TypeCode typeCode) { return (typeCode == TypeCode.Decimal || (type.IsPrimitive && typeCode != TypeCode.Object && typeCode != TypeCode.Boolean && typeCode != TypeCode.Char)); }

}