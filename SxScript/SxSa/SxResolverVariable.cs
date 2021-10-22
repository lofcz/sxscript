namespace SxScript.SxSa;

public enum SxResolverVariableStates
{
    Declared,
    Defined,
    Used
}

public class SxResolverVariable
{
    public SxToken Name { get; set; }
    public SxResolverVariableStates State { get; set; }

    public SxResolverVariable(SxToken name, SxResolverVariableStates state)
    {
        Name = name;
        State = state;
    }
}