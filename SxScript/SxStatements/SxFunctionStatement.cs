namespace SxScript.SxStatements;

public class SxFunctionStatement : SxStatement, SxStatement.ISxCallStatement
{
    public SxToken Name { get; set; }
    public List<SxToken> Pars { get; set; }
    public SxBlockStatement Body { get; set; }
    public bool Return { get; set; }
    public SxStatement Statement { get; set; } 
    public SxFunctionStatement(SxToken name, List<SxToken> pars, List<SxStatement> body)
    {
        Name = name;
        Pars = pars;
        Body = new SxBlockStatement(body);
        Return = false;
        Statement = this;
    }
    
    public override async Task<object?> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}