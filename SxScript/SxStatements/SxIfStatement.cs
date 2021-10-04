namespace SxScript.SxStatements;

public class SxIfStatement : SxStatement
{
    public SxExpression Condition { get; set; }
    public SxStatement? ThenBranch { get; set; }
    public SxStatement? ElseBranch { get; set; }
    
    public override async Task<object> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }

    public SxIfStatement(SxExpression condition, SxStatement thenBranch, SxStatement elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }
}