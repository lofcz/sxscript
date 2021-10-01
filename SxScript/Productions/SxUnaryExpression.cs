using System.Linq.Expressions;

namespace SxScript;

public class SxUnaryExpression<T> : SxExpression<T>
{
    public SxToken Operator { get; set; }
    public SxExpression<T> Expr { get; set; }
    
    public SxUnaryExpression(SxToken op, SxExpression<T> expr)
    {
        Operator = op;
        Expr = expr;
    }

    public override T Accept(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}