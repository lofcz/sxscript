namespace SxScript;

public class SxVarExpression : SxExpression
{
    public SxToken Name { get; set; }
    public SxArrayExpression? ArrayExpr { get; set; }
    public SxToken? ArrayName { get; set; }

    public SxVarExpression(SxToken name, SxArrayExpression? arrayExpr, SxToken? arrayName)
    {
        Name = name;
        ArrayExpr = arrayExpr;
        ArrayName = arrayName;
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}