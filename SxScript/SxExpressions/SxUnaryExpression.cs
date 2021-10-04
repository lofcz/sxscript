using System.Linq.Expressions;

namespace SxScript;

public class SxUnaryExpression : SxExpression
{
    public SxToken Operator { get; set; }
    public SxExpression Expr { get; set; }
    
    public SxUnaryExpression(SxToken op, SxExpression expr)
    {
        Operator = op;
        Expr = expr;
    }

    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}