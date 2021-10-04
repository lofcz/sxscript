using System.Linq.Expressions;
using System.Text;

namespace SxScript;

public class SxAstPrinter : SxExpression.ISxExpressionVisitor<string>
{
    public async Task<string> Visit(SxBinaryExpression expr)
    {
        return Parenthesise(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public async Task<string> Visit(SxUnaryExpression expr)
    {
        return Parenthesise(expr.Operator.Lexeme, expr.Expr);
    }

    public async Task<string> Visit(SxLiteralExpression expr)
    {
        return expr?.Value?.ToString() ?? "nill";
    }

    public async Task<string> Visit(SxGroupingExpression expr)
    {
        return Parenthesise("zavorky", expr.Expr);
    }

    public async Task<string> Visit(SxTernaryExpression expr)
    {
        return Parenthesise("ternarni", expr.Expr, expr.CaseTrue, expr.CaseFalse);
    }

    public async Task<string> Visit(SxVarExpression expr)
    {
        return Parenthesise($"definice promenne - {expr.Name.Lexeme}");
    }

    public async Task<string> Visit(SxAssignExpression expr)
    {
        return Parenthesise($"nastaveni hodnoty promenne - {expr.Name.Lexeme}", expr.Value);
    }

    public async Task<string> Visit(SxLogicalExpression expr)
    {
        return Parenthesise($"logický operátor - {expr.Operator.Lexeme}", expr.Left, expr.Right);
    }

    public async Task<string> Visit(SxPostfixExpression expr)
    {
        return Parenthesise(expr.Operator.Lexeme, expr.Expr);
    }

    public async Task<string> Visit(SxCallExpression expr)
    {
        return Parenthesise("funkce", expr.Callee);
    }

    public async Task<string> Print(SxExpression expression)
    {
        return await expression.Accept(this);
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