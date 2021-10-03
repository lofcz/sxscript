namespace SxScript.SxStatements;

public class SxBreakStatement : SxStatement
{
    public override T Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}