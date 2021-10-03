namespace SxScript.SxStatements;

public class SxGotoStatement : SxStatement
{
    public SxToken Identifier { get; set; }

    public SxGotoStatement(SxToken identifier)
    {
        Identifier = identifier;
    }
    
    public override T Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}