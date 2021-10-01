using System.Linq.Expressions;
namespace SxScript;

public class SxBinaryExpression<T> : SxExpression<T>
{
    public SxExpression<T> Left { get; set; }
    public SxToken Operator { get; set; }
    public SxExpression<T> Right { get; set; }
    
    public SxBinaryExpression(SxExpression<T> left, SxToken op, SxExpression<T> right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override T Accept(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}