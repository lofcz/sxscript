namespace SxScript;

public class SxVarExpression : SxExpression
{
    public SxToken Name { get; set; }

    public SxVarExpression(SxToken name)
    {
        Name = name;
    }
    
    public override T Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}