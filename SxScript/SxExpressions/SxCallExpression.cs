using SxScript.SxStatements;

namespace SxScript;

public class SxCallExpression : SxExpression, SxExpression.ISxAwaitableExpression, SxStatement.ISxReturnableStatement
{
    public SxExpression Callee { get; set; }
    public SxToken Paren { get; set; }
    public List<SxExpression> Arguments { get; set; }
    public bool Await { get; set; }
    public bool Return { get; set; }

    public SxCallExpression(SxExpression callee, SxToken paren, List<SxExpression> arguments)
    {
        Callee = callee;
        Paren = paren;
        Arguments = arguments;
        Await = false;
        Return = false;
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}