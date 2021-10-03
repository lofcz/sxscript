namespace SxScript.SxStatements;

public abstract class SxStatement
{
    public interface ISxStatementVisitor<T> 
    {
        T Visit(SxExpressionStatement expr);
        T Visit(SxPrintStatement expr);
        T Visit(SxVarStatement expr);
        T Visit(SxBlockStatement expr);
        T Visit(SxIfStatement expr);
        T Visit(SxWhileStatement expr);
        T Visit(SxBreakStatement expr);
        T Visit(SxLabelStatement expr);
        T Visit(SxGotoStatement expr);
    }
    
    public SxExpression Expr { get; set; }
    public abstract T Accept<T>(ISxStatementVisitor<T> visitor);
}