namespace SxScript;

public class SxArgumentDeclrExpression
{
    public SxExpression Modifier { get; set; }
    public SxToken Name { get; set; }
    public SxExpression DefaultValue { get; set; }
    public bool HasDefaultValue { get; set; }

    public SxArgumentDeclrExpression(SxToken name, SxExpression defaultValue, bool hasDefaultValue)
    {
        Name = name;
        DefaultValue = defaultValue;
        Modifier = null;
        HasDefaultValue = hasDefaultValue;
    }
}