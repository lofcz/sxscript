namespace SxScript;

public class SxTernaryExpression<T> : SxExpression<T>
{
    public SxExpression<T> Expr { get; set; }
    public SxExpression<T> CaseTrue { get; set; }
    public SxExpression<T> CaseFalse { get; set; }
    
    public SxTernaryExpression(SxExpression<T> expr, SxExpression<T> caseTrue, SxExpression<T> caseFalse)
    {
        Expr = expr;
        CaseTrue = caseTrue;
        CaseFalse = caseFalse;
    }

    public override T Accept(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}