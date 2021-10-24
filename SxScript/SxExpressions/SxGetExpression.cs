namespace SxScript;

public class SxGetExpression : SxExpression
{
    public SxToken Name { get; set; }
    public SxExpression Object { get; set; }
    public SxArrayExpression? ArrayExpr { get; set; }

    public SxGetExpression(SxToken name, SxExpression obj, SxArrayExpression? arrayExpression)
    {
        Name = name;
        Object = obj;
        ArrayExpr = arrayExpression;
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}