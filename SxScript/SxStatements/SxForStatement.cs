namespace SxScript.SxStatements;

public class SxForStatement : SxStatement, SxStatement.ISxLoopingStatement
{
    public SxStatement Initializer { get; set; }
    public SxExpression Condition { get; set; }
    public SxExpression Increment { get; set; }
    public SxStatement Body { get; set; }
    public bool Break { get; set; }
    public bool Continue { get; set; }

    public SxForStatement(SxStatement initializer, SxExpression condition, SxExpression increment, SxStatement statement)
    {
        Initializer = initializer;
        Condition = condition;
        Increment = increment;
        Body = statement;
        Break = false;
        Continue = false;
    }
    
    public override async Task<object?> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}