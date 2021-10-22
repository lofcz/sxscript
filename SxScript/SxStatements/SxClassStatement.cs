using SxScript.SxFFI;

namespace SxScript.SxStatements;

public class SxClassStatement : SxStatement
{
    public SxToken Name { get; set; }
    public List<SxFunctionStatement> Methods { get; set; }

    public SxClassStatement(SxToken name, List<SxFunctionStatement> methods)
    {
        Name = name;
        Methods = methods;
    }
    
    public override async Task<object?> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}