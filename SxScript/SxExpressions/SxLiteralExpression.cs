using System.Linq.Expressions;
namespace SxScript;

public class SxLiteralExpression : SxExpression
{
    public object Value { get; set; }

    public SxLiteralExpression(object value)
    {
        Value = value;
    }

    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}