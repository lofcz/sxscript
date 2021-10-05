namespace SxScript.SxStatements;

public class SxLabelStatement : SxStatement
{
    public SxToken Identifier { get; set; }
    public SxStatement Statement { get; set; }
    
    public override async Task<object?> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }

    public SxLabelStatement(SxToken identifier, SxStatement statement)
    {
        Identifier = identifier;
        Statement = statement;
    }
}