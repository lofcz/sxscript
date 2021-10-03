namespace SxScript;

public abstract class SxExpression
{
   public interface ISxExpressionVisitor<T> 
   {
      T Visit(SxBinaryExpression expr);
      T Visit(SxUnaryExpression expr);
      T Visit(SxLiteralExpression expr);
      T Visit(SxGroupingExpression expr);
      T Visit(SxTernaryExpression expr);
      T Visit(SxVarExpression expr);
      T Visit(SxAssignExpression expr);
      T Visit(SxLogicalExpression expr);
   }

   public abstract T Accept<T>(ISxExpressionVisitor<T> visitor);
}