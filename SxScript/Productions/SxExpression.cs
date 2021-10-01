namespace SxScript;

public abstract class SxExpression<T>
{
   public abstract T Accept(IExpressionVisitor<T> visitor);
}