namespace SxScript.SxStatements;

public class SxIfStatement : SxStatement
{
    public SxExpression Condition { get; set; }
    public SxStatement? ThenBranch { get; set; }
    public SxStatement? ElseBranch { get; set; }
    
    public override T Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public SxIfStatement(SxExpression condition, SxStatement thenBranch, SxStatement elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }
}