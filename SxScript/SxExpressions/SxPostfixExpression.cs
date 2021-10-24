using System.Linq.Expressions;

namespace SxScript;

public class SxPostfixExpression : SxExpression
{
    public SxToken Operator { get; set; }
    public SxExpression Expr { get; set; }
    public SxExpression? PostfixExpr { get; set; }
    
    public SxPostfixExpression(SxToken op, SxExpression expr, SxExpression? postfixExpr)
    {
        Operator = op;
        Expr = expr;
        PostfixExpr = postfixExpr;
    }

    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}