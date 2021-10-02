namespace SxScript.SxStatements;

public class SxExpressionStatement : SxStatement
{
    public SxExpressionStatement(SxExpression expression)
    {
        Expr = expression;
    }
    
    public override T Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}