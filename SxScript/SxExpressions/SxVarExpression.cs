namespace SxScript;

public class SxVarExpression : SxExpression
{
    public SxToken Name { get; set; }

    public SxVarExpression(SxToken name)
    {
        Name = name;
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}