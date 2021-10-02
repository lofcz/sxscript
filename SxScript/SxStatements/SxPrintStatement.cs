namespace SxScript.SxStatements;

public class SxPrintStatement : SxStatement
{
    public SxPrintStatement(SxExpression expression)
    {
        Expr = expression;
    }
    
    public override T Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}