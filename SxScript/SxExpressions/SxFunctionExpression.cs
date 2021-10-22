using SxScript.SxStatements;

namespace SxScript;

public class SxFunctionExpression : SxExpression, SxExpression.ISxAwaitableExpression, SxStatement.ISxReturnableStatement
{
    public List<SxArgumentDeclrExpression> Pars { get; set; }
    public SxBlockStatement Body { get; set; }

    public SxFunctionExpression(List<SxArgumentDeclrExpression> pars, List<SxStatement> body)
    {
        Pars = pars;
        Body = new SxBlockStatement(body, false);
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }

    public bool Await { get; set; }
    public bool Return { get; set; }
}