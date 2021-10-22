namespace SxScript;

public abstract class SxExpression
{
   public interface ISxExpressionVisitor<T> 
   {
      Task<T> Visit(SxBinaryExpression expr);
      Task<T> Visit(SxUnaryExpression expr);
      Task<T> Visit(SxLiteralExpression expr);
      Task<T> Visit(SxGroupingExpression expr);
      Task<T> Visit(SxTernaryExpression expr);
      Task<T> Visit(SxVarExpression expr);
      Task<T> Visit(SxAssignExpression expr);
      Task<T> Visit(SxLogicalExpression expr);
      Task<T> Visit(SxPostfixExpression expr);
      Task<T> Visit(SxCallExpression expr);
      Task<T> Visit(SxFunctionExpression expr);
      Task<T> Visit(SxGetExpression expr);
      Task<T> Visit(SxSetExpression expr);
      Task<T> Visit(SxThisExpression expr);
   }
   
   public interface ISxCallable
   {
      object? Call(SxInterpreter interpreter);
      Task<object?> PrepareAndCallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments);
      Task PrepareCallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments);
   }
   
   public interface ISxAsyncCallable : ISxAwaitableExpression
   {
      Task<object?> CallAsync(SxInterpreter interpreter, List<SxResolvedCallArgument> arguments);
   }
   
   public interface ISxAwaitableExpression
   {
      public bool Await { get; set; }
   }

   public abstract Task<T> Accept<T>(ISxExpressionVisitor<T> visitor);
}