using System.Linq.Expressions;
namespace SxScript;

public class SxLiteralExpression<T> : SxExpression<T>
{
    public object Value { get; set; }

    public SxLiteralExpression(object value)
    {
        Value = value;
    }

    public override T Accept(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}