using System.Linq.Expressions;
using System.Text;

namespace SxScript;

public class SxAstPrinter : IExpressionVisitor<string>
{
    public string Visit(SxBinaryExpression<string> expr)
    {
        return Parenthesise(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string Visit(SxUnaryExpression<string> expr)
    {
        return Parenthesise(expr.Operator.Lexeme, expr.Expr);
    }

    public string Visit(SxLiteralExpression<string> expr)
    {
        return expr?.Value?.ToString() ?? "nill";
    }

    public string Visit(SxGroupingExpression<string> expr)
    {
        return Parenthesise("zavorky", expr.Expr);
    }
    
    public string Print(SxExpression<string> expression)
    {
        return expression.Accept(this);
    }

    string Parenthesise(string name, params SxExpression<string>[] expressions)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("(").Append(name);
        foreach (SxExpression<string> expression in expressions)
        {
            sb.Append(" ");
            sb.Append(expression.Accept(this));
        }

        sb.Append(")");
        return sb.ToString();
    }
}