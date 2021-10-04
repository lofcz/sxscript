namespace SxScript.SxStatements;

public class SxPrintStatement : SxStatement
{
    public SxPrintStatement(SxExpression expression)
    {
        Expr = expression;
    }
    
    public override async Task<object> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}