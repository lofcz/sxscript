namespace SxScript;

public class SxAssignExpression : SxExpression
{
    public SxToken Name { get; set; }
    public SxExpression Value { get; set; }
    
    public override T Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public SxAssignExpression(SxToken name, SxExpression value)
    {
        Name = name;
        Value = value;
    }
}