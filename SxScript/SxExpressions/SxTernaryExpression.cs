namespace SxScript;

public class SxTernaryExpression : SxExpression
{
    public SxExpression Expr { get; set; }
    public SxExpression CaseTrue { get; set; }
    public SxExpression CaseFalse { get; set; }
    
    public SxTernaryExpression(SxExpression expr, SxExpression caseTrue, SxExpression caseFalse)
    {
        Expr = expr;
        CaseTrue = caseTrue;
        CaseFalse = caseFalse;
    }

    public override T Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}