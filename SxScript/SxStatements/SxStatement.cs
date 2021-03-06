namespace SxScript.SxStatements;

public abstract class SxStatement
{
    public interface ISxStatementVisitor<T> 
    {
        Task<T> Visit(SxExpressionStatement expr);
        Task<object?> Visit(SxPrintStatement expr);
        Task<T> Visit(SxVarStatement expr);
        Task<T> Visit(SxBlockStatement expr);
        Task<T> Visit(SxIfStatement expr);
        Task<T> Visit(SxWhileStatement expr);
        Task<T> Visit(SxBreakStatement expr);
        Task<T> Visit(SxLabelStatement expr);
        Task<T> Visit(SxGotoStatement expr);
        Task<T> Visit(SxForStatement expr);
        Task<T> Visit(SxContinueStatement expr);
        Task<T> Visit(SxFunctionStatement expr);
        Task<T> Visit(SxReturnStatement expr);
        Task<T> Visit(SxClassStatement expr);
    }
    
    public interface ISxBreakableStatement
    {
        public bool Break { get; set; }
        public bool Continue { get; set; }
    }
    
    public interface ISxReturnableStatement
    {
        public bool Return { get; set; }
    }
    
    public interface ISxLoopingStatement : ISxBreakableStatement
    {
        public SxStatement Body { get; set; }
    }
    
    public interface ISxCallStatement : ISxReturnableStatement
    {
        public SxStatement Statement { get; set; }
    }
    
    public SxExpression Expr { get; set; }
    public abstract Task<object?> Accept<T>(ISxStatementVisitor<T> visitor);
}