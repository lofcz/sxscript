using SxScript.SxStatements;

namespace SxScript;

public class SxCallExpression : SxExpression, SxExpression.ISxAwaitableExpression, SxStatement.ISxReturnableStatement
{
    public SxExpression Callee { get; set; }
    public SxToken Paren { get; set; }
    public List<SxCallArgument> Arguments { get; set; }
    public bool Await { get; set; }
    public bool Return { get; set; }

    public SxCallExpression(SxExpression callee, SxToken paren, List<SxCallArgument> arguments)
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

public class SxCallArgument
{
    public SxExpression Value { get; set; }
    public SxExpression? Name { get; set; }

    public SxCallArgument(SxExpression value, SxExpression name)
    {
        Value = value;
        Name = name;
    }
}

public class SxResolvedCallArgument
{
    public string? Name { get; set; }
    public object? Value { get; set; }

    public SxResolvedCallArgument(object? value, string? name)
    {
        Value = value;
        Name = name;
    }
}

