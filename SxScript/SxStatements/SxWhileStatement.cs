namespace SxScript.SxStatements;

public class SxWhileStatement : SxStatement
{
    public SxStatement Statement { get; set; }

    public SxWhileStatement(SxExpression expr, SxStatement statement)
    {
        Expr = expr;
        Statement = statement;
    }
    
    public override T Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}