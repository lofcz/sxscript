namespace SxScript.SxStatements;

public class SxGotoStatement : SxStatement
{
    public SxToken Identifier { get; set; }

    public SxGotoStatement(SxToken identifier)
    {
        Identifier = identifier;
    }
    
    public override async Task<object> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}