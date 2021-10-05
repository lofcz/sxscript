namespace SxScript.SxStatements;

public class SxBlockStatement : SxStatement, SxStatement.ISxBreakableStatement, SxStatement.ISxReturnableStatement
{
    public List<SxStatement> Statements { get; set; }
    public bool Break { get; set; }
    public bool Continue { get; set; }
    public bool Return { get; set; }
    public object? ReturnValue { get; set; }
    
    public override async Task<object?> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }

    public SxBlockStatement(List<SxStatement> statements)
    {
        Statements = statements;
        Expr = null!;
        Break = false;
        Continue = false;
        Return = false;
        ReturnValue = null;
    }
}