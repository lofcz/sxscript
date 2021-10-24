namespace SxScript;

public class SxSetExpression : SxExpression
{
    public SxExpression Object { get; set; }
    public SxToken Name { get; set; }
    public SxExpression Value { get; set; }
    public SxToken Operator { get; set; }
    public SxExpression? Index { get; set; }
    public SxArrayExpression? ArrayExpr { get; set; }

    public SxSetExpression(SxToken name, SxExpression obj, SxExpression value, SxToken op, SxExpression? index, SxArrayExpression? arrayExpr)
    {
        Object = obj;
        Name = name;
        Value = value;
        Operator = op;
        Index = index;
        ArrayExpr = arrayExpr;
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}