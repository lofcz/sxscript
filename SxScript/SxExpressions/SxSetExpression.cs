namespace SxScript;

public class SxSetExpression : SxExpression
{
    public SxExpression Object { get; set; }
    public SxToken Name { get; set; }
    public SxExpression Value { get; set; }

    public SxSetExpression(SxToken name, SxExpression obj, SxExpression value)
    {
        Object = obj;
        Name = name;
        Value = value;
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}