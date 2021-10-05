namespace SxScript.SxStatements;

public class SxReturnStatement : SxStatement
{
    public SxToken Keyword { get; set; }
    public SxExpression Value { get; set; }

    public SxReturnStatement(SxToken keyword, SxExpression value)
    {
        Keyword = keyword;
        Value = value;
    }
    
    public override async Task<object?> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}