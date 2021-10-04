using System.Linq.Expressions;
namespace SxScript;

public class SxBinaryExpression : SxExpression
{
    public SxExpression Left { get; set; }
    public SxToken Operator { get; set; }
    public SxExpression Right { get; set; }
    
    public SxBinaryExpression(SxExpression left, SxToken op, SxExpression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}