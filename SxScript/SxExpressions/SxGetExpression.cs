namespace SxScript;

public class SxGetExpression : SxExpression
{
    public SxToken Name { get; set; }
    public SxExpression Object { get; set; }

    public SxGetExpression(SxToken name, SxExpression obj)
    {
        Name = name;
        Object = obj;
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}