using SxScript.SxFFI;

namespace SxScript.SxStatements;

public class SxClassStatement : SxStatement
{
    public SxToken Name { get; set; }
    public List<SxFunctionStatement> Methods { get; set; }
    public List<SxVarStatement> Fields { get; set; }
    public List<SxFunctionStatement> ClassMethods { get; set; }
    public SxVarExpression? Superclass { get; set; }
    public bool SuperclassIsValid { get; set; }

    public SxClassStatement(SxToken name, List<SxFunctionStatement> methods, List<SxVarStatement> fields, List<SxFunctionStatement> classMethods, SxVarExpression superclass)
    {
        Name = name;
        Methods = methods;
        Fields = fields;
        ClassMethods = classMethods;
        Superclass = superclass;
        SuperclassIsValid = true;
    }
    
    public override async Task<object?> Accept<T>(ISxStatementVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}