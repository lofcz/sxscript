namespace SxScript;

public interface IExpressionVisitor<T>
{
    T Visit(SxBinaryExpression<T> expr);
    T Visit(SxUnaryExpression<T> expr);
    T Visit(SxLiteralExpression<T> expr);
    T Visit(SxGroupingExpression<T> expr);
    T Visit(SxTernaryExpression<T> expr);
}