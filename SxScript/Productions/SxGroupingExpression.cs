using System.Linq.Expressions;

namespace SxScript;

public class SxGroupingExpression : SxExpression
{
    public SxExpression Expr { get; set; }

    public SxGroupingExpression(SxExpression expr)
    {
        Expr = expr;
    }

    public override T Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}