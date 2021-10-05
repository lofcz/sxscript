namespace SxScript.SxStatements;

public class SxWhileStatement : SxStatement, SxStatement.ISxLoopingStatement
{
    public SxStatement Statement { get; set; }
    public bool Break { get; set; }
    public bool Continue { get; set; }

    public SxWhileStatement(SxExpression expr, SxStatement statement)
    {
        Expr = expr;
        Statement = statement;
        Break = false;
        Continue = false;
    }
    
    public override async Task<object?> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}