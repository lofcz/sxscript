namespace SxScript.SxStatements;

public class SxBlockStatement : SxStatement
{
    public List<SxStatement> Statements { get; set; }
    
    public override T Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public SxBlockStatement(List<SxStatement> statements)
    {
        Statements = statements;
        Expr = null!;
    }
}