namespace SxScript.SxStatements;

public class SxContinueStatement : SxStatement
{
    public override async Task<object?> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}