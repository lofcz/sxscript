using System.Linq.Expressions;

namespace SxScript;

public class SxPostfixExpression : SxExpression
{
    public SxToken Operator { get; set; }
    public SxExpression Expr { get; set; }
    
    public SxPostfixExpression(SxToken op, SxExpression expr)
    {
        Operator = op;
        Expr = expr;
    }

    public override T Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}