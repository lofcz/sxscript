namespace SxScript.SxStatements;

public class SxLabelStatement : SxStatement
{
    public SxToken Identifier { get; set; }
    public SxStatement Statement { get; set; }
    
    public override T Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public SxLabelStatement(SxToken identifier, SxStatement statement)
    {
        Identifier = identifier;
        Statement = statement;
    }
}