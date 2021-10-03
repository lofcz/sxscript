using System.Linq.Expressions;
using System.Text;

namespace SxScript;

public class SxAstPrinter : SxExpression.ISxExpressionVisitor<string>
{
    public string Visit(SxBinaryExpression expr)
    {
        return Parenthesise(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string Visit(SxUnaryExpression expr)
    {
        return Parenthesise(expr.Operator.Lexeme, expr.Expr);
    }

    public string Visit(SxLiteralExpression expr)
    {
        return expr?.Value?.ToString() ?? "nill";
    }

    public string Visit(SxGroupingExpression expr)
    {
        return Parenthesise("zavorky", expr.Expr);
    }

    public string Visit(SxTernaryExpression expr)
    {
        return Parenthesise("ternarni", expr.Expr, expr.CaseTrue, expr.CaseFalse);
    }

    public string Visit(SxVarExpression expr)
    {
        return Parenthesise($"definice promenne - {expr.Name.Lexeme}");
    }

    public string Visit(SxAssignExpression expr)
    {
        return Parenthesise($"nastaveni hodnoty promenne - {expr.Name.Lexeme}", expr.Value);
    }

    public string Visit(SxLogicalExpression expr)
    {
        return Parenthesise($"logický operátor - {expr.Operator.Lexeme}", expr.Left, expr.Right);
    }

    public string Print(SxExpression expression)
    {
        return expression.Accept(this);
    }

    string Parenthesise(string name, params SxExpression[] expressions)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("(").Append(name);
        foreach (SxExpression expression in expressions)
        {
            sb.Append(" ");
            sb.Append(expression.Accept(this));
        }

        sb.Append(")");
        return sb.ToString();
    }
}