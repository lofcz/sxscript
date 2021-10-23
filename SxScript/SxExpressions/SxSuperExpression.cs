namespace SxScript;

public class SxSuperExpression : SxExpression
{
    public SxToken Keyword { get; set; }
    public SxToken Method { get; set; }

    public SxSuperExpression(SxToken keyword, SxToken method)
    {
        Keyword = keyword;
        Method = method;
    }

    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}