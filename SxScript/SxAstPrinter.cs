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

    public async Task<string> Visit(SxFunctionExpression expr)
    {
        return Parenthesise("anonymní funkce", expr.Body.Statements.Select(x => x.Expr).ToList());
    }

    public async Task<string> Visit(SxGetExpression expr)
    {
        return Parenthesise("čtení vlastnosti", expr.Object);
    }

    public async Task<string> Visit(SxSetExpression expr)
    {
        return Parenthesise("zápis vlastnosti", expr.Object);
    }

    public async Task<string> Visit(SxThisExpression expr)
    {
        return Parenthesise("lokální kontext", new SxLiteralExpression(expr.Keyword));
    }

    public async Task<string> Visit(SxSuperExpression expr)
    {
        return Parenthesise("volání předka", new SxLiteralExpression(expr.Keyword));
    }

    public async Task<string> Visit(SxArrayExpression expr)
    {
        return Parenthesise("pole", expr.ArrayExpr);
    }

    public async Task<string> Visit(SxArgumentDeclrExpression expr)
    {
        return Parenthesise("argument fce", expr.Modifier);
    }

    public async Task<string> Print(SxExpression expression)
    {
        return await expression.Accept(this);
    }

    string Parenthesise(string name, List<SxExpression> expressions)
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

    string Parenthesise(string name, params SxExpression[] expressions)
    {
        return Parenthesise(name, expressions.ToList());
    }
}