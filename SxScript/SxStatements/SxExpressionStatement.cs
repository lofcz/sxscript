namespace SxScript.SxStatements;

public class SxExpressionStatement : SxStatement
{
    public SxExpressionStatement(SxExpression expression)
    {
        Expr = expression;
    }
    
    public override async Task<object> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}