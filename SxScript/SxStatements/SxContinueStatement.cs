namespace SxScript.SxStatements;

public class SxContinueStatement : SxStatement
{
    public override T Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}