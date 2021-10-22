namespace SxScript;

public class SxThisExpression : SxExpression
{
    public SxToken Keyword { get; set; }

    public SxThisExpression(SxToken keyword)
    {
        Keyword = keyword;
    }
    
    public override async Task<T> Accept<T>(ISxExpressionVisitor<T> visitor)
    {
        return await visitor.Visit(this);
    }
}