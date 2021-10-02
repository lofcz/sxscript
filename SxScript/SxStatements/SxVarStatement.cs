namespace SxScript.SxStatements;

public class SxVarStatement : SxStatement
{
    public SxToken Name { get; set; }
    
    public SxVarStatement(SxExpression expression, SxToken name)
    {
        Expr = expression;
        Name = name;
    }
    
    public override T Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}