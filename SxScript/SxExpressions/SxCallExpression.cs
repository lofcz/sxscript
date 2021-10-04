namespace SxScript;

public class SxCallExpression : SxExpression, SxExpression.ISxAwaitableExpression
{
    public SxExpression Callee { get; set; }
    public SxToken Paren { get; set; }
    public List<SxExpression> Arguments { get; set; }

    public SxCallExpression(SxExpression callee, SxToken paren, List<SxExpression> arguments)
    {
        Callee = callee;
        Paren = paren;
        Arguments = arguments;
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }

    public bool Await { get; set; }
}