using System.Linq.Expressions;

namespace SxScript;

public class SxGroupingExpression<T> : SxExpression<T>
{
    public SxExpression<T> Expr { get; set; }

    public SxGroupingExpression(SxExpression<T> expr)
    {
        Expr = expr;
    }

    public override T Accept(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}