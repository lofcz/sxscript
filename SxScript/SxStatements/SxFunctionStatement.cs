namespace SxScript.SxStatements;

public class SxFunctionStatement : SxStatement, SxStatement.ISxCallStatement
{
    public SxToken Name { get; set; }
    public bool Return { get; set; }
    public SxStatement Statement { get; set; } 
    public SxFunctionExpression FunctionExpression { get; set; }

    public SxFunctionStatement(SxToken name, SxFunctionExpression functionExpression)
    {
        Name = name;
        FunctionExpression = functionExpression;
        Return = false;
        Statement = this;
    }
    
    public override async Task<object?> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}