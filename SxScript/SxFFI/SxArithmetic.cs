using System.Globalization;

namespace SxScript.SxFFI;

public static class SxArithmetic
{
    public static object? ResolveSetValue(object? source, object? value = null, SxToken? op = null)
    {
        if (op == null || op.Type == SxTokenTypes.Equal)
        {
            source = value;
            return source;
        }

        if (op.Type is SxTokenTypes.PlusEqual or SxTokenTypes.MinusEqual or SxTokenTypes.StarEqual or SxTokenTypes.SlashEqual or SxTokenTypes.PercentEqual or SxTokenTypes.CaretEqual)
        {
            if (source is int leftInt)
            {
                if (value is int rightInt)
                {
                    source = op.Type switch
                    {
                        SxTokenTypes.PlusEqual => leftInt + rightInt,
                        SxTokenTypes.MinusEqual => leftInt - rightInt,
                        SxTokenTypes.StarEqual => leftInt * rightInt,
                        SxTokenTypes.SlashEqual => leftInt / rightInt,
                        SxTokenTypes.PercentEqual => leftInt % rightInt,
                        SxTokenTypes.CaretEqual => Math.Pow(leftInt, rightInt),
                        _ => source
                    };
                    return source;
                }

                if (value is double rightDouble)
                {
                    source = op.Type switch
                    {
                        SxTokenTypes.PlusEqual => leftInt + rightDouble,
                        SxTokenTypes.MinusEqual => leftInt - rightDouble,
                        SxTokenTypes.StarEqual => leftInt * rightDouble,
                        SxTokenTypes.SlashEqual => leftInt / rightDouble,
                        SxTokenTypes.PercentEqual => leftInt % rightDouble,
                        SxTokenTypes.CaretEqual => Math.Pow(leftInt, rightDouble),
                        _ => source
                    };
                    return source;
                }
                
                if (value is string rightString)
                {
                    source = op.Type switch
                    {
                        SxTokenTypes.PlusEqual => leftInt + rightString,
                        SxTokenTypes.MinusEqual => leftInt.ToString(CultureInfo.InvariantCulture).Replace(rightString, ""),
                        _ => source
                    };
                    
                    return source;
                }

                if (value is bool rightBool)
                {
                    if (rightBool)
                    {
                        source = op.Type switch
                        {
                            SxTokenTypes.PlusEqual => leftInt + 1,
                            SxTokenTypes.MinusEqual => leftInt.ToString(CultureInfo.InvariantCulture).Replace("true", ""),
                            _ => source
                        };
                        return source;
                    }
                }
                
                return source;
            }

            if (source is double leftDouble)
            {
                if (value is double rightDouble)
                {
                    source = op.Type switch
                    {
                        SxTokenTypes.PlusEqual => leftDouble + rightDouble,
                        SxTokenTypes.MinusEqual => leftDouble - rightDouble,
                        SxTokenTypes.StarEqual => leftDouble * rightDouble,
                        SxTokenTypes.SlashEqual => leftDouble / rightDouble,
                        SxTokenTypes.PercentEqual => leftDouble % rightDouble,
                        SxTokenTypes.CaretEqual => Math.Pow(leftDouble, rightDouble),
                        _ => source
                    };
                    return source;
                }

                if (value is int rightInt)
                {
                    source = op.Type switch
                    {
                        SxTokenTypes.PlusEqual => leftDouble + rightInt,
                        SxTokenTypes.MinusEqual => leftDouble - rightInt,
                        SxTokenTypes.StarEqual => leftDouble * rightInt,
                        SxTokenTypes.SlashEqual => leftDouble / rightInt,
                        SxTokenTypes.PercentEqual => leftDouble % rightInt,
                        SxTokenTypes.CaretEqual => Math.Pow(leftDouble, rightInt),
                        _ => source
                    };
                    return source;
                }
                
                if (value is string rightString)
                {
                    source = op.Type switch
                    {
                        SxTokenTypes.PlusEqual => leftDouble + rightString,
                        SxTokenTypes.MinusEqual => leftDouble.ToString(CultureInfo.InvariantCulture).Replace(rightString, ""),
                        _ => source
                    };
                    return source;
                }
                
                if (value is bool rightBool)
                {
                    if (rightBool)
                    {
                        source = op.Type switch
                        {
                            SxTokenTypes.PlusEqual => leftDouble + 1,
                            SxTokenTypes.MinusEqual => leftDouble.ToString(CultureInfo.InvariantCulture).Replace("true", ""),
                            _ => source
                        };
                        return source;
                    }
                }
                
                return source;
            }

            if (source is string leftString)
            {
                if (value is bool rightBool)
                {
                    source = leftString + (rightBool ? "true" : "false");
                    return source;
                }

                source = leftString + (value ?? "");
                return source;
            }
        }

        return source;
    }
}