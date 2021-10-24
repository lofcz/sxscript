using SxScript.SxFFI;

namespace SxScript;

public class SxArrayExpression : SxExpression
{
    public List<SxExpression>? ArrayExpr { get; set; }
    public List<object?>? ArrayExprResolved { get; set; }
    public SxArray? Array { get; set; }

    public SxArrayExpression(List<SxExpression>? arrayExpr, List<object?>? arrayExprResolved, SxArray? array)
    {
        ArrayExpr = arrayExpr;
        ArrayExprResolved = arrayExprResolved;
        Array = array;
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}