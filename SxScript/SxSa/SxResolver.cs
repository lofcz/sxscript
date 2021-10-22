using SxScript.SxStatements;

namespace SxScript.SxSa;

public class SxResolver : SxExpression.ISxExpressionVisitor<object>, SxStatement.ISxStatementVisitor<object>
{
    public Stack<Dictionary<string, bool>> Scopes { get; set; }
    public SxInterpreter Interpreter { get; set; }

    public SxResolver(SxInterpreter interpreter)
    {
        Interpreter = interpreter;
        Scopes = new Stack<Dictionary<string, bool>>();
    }

    public async Task<object> Visit(SxBinaryExpression expr)
    {
        await Resolve(expr.Left);
        await Resolve(expr.Right);
        return null!;
    }

    public async Task<object> Visit(SxUnaryExpression expr)
    {
        await Resolve(expr.Expr);
        return null!;
    }

    public async Task<object> Visit(SxLiteralExpression expr)
    {
        return null!;
    }

    public async Task<object> Visit(SxGroupingExpression expr)
    {
        await Resolve(expr.Expr);
        return null!;
    }

    public async Task<object> Visit(SxTernaryExpression expr)
    {
        await Resolve(expr.Expr);
        await Resolve(expr.CaseTrue);
        await Resolve(expr.CaseFalse);

        return null!;
    }

    public async Task<object> Visit(SxVarExpression expr)
    {
        if (Scopes.Count != 0 && !Scopes.Peek().ContainsKey(expr.Name.Lexeme))
        {
            // [todo] chyba, lokální proměnná v její definici
            int x = 0;
        }

        ResolveLocal(expr, expr.Name);
        return null!;
    }

    public async Task<object> Visit(SxAssignExpression expr)
    {
        await Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);

        return null!;
    }

    public async Task<object> Visit(SxLogicalExpression expr)
    {
        await Resolve(expr.Left);
        await Resolve(expr.Right);
        return null!;
    }

    public async Task<object> Visit(SxPostfixExpression expr)
    {
        await Resolve(expr.Expr);
        return null!;
    }

    public async Task<object> Visit(SxCallExpression expr)
    {
        await Resolve(expr.Callee);

        foreach (SxCallArgument argument in expr.Arguments)
        {
            if (argument.Name != null)
            {
                //await Resolve(argument.Name);
            }
            
            await Resolve(argument.Value);
        }

        return null!;
    }

    public async Task<object> Visit(SxExpressionStatement expr)
    {
        await Resolve(expr.Expr);
        return null!;
    }

    public async Task<object?> Visit(SxPrintStatement expr)
    {
        await Resolve(expr.Expr);
        return null!;
    }

    public async Task<object> Visit(SxVarStatement expr)
    {
        Declare(expr.Name);
        await Resolve(expr.Expr);
        Define(expr.Name);

        return null!;
    }

    public async Task<object> Visit(SxBlockStatement expr)
    {
        if (!expr.GeneratesScope)
        {
            await Resolve(expr.Statements);
            return null!;
        }
        
        BeginScope();
        await Resolve(expr.Statements);
        EndScope();

        return null!;
    }

    public async Task<object> Visit(SxIfStatement expr)
    {
        await Resolve(expr.Condition);

        if (expr.ThenBranch != null)
        {
            await Resolve(expr.ThenBranch);
        }

        if (expr.ElseBranch != null)
        {
            await Resolve(expr.ElseBranch);
        }

        return null!;
    }

    public async Task<object> Visit(SxWhileStatement expr)
    {
        await Resolve(expr.Expr);
        await Resolve(expr.Body);

        return null!;
    }

    public async Task<object> Visit(SxBreakStatement expr)
    {
        return null!;
    }

    public async Task<object> Visit(SxLabelStatement expr)
    {
        Declare(expr.Identifier);
        Define(expr.Identifier);
        await Resolve(expr.Statement);

        return null!;
    }

    public async Task<object> Visit(SxGotoStatement expr)
    {
        Declare(expr.Identifier);
        Define(expr.Identifier);

        return null!;
    }

    public async Task<object> Visit(SxForStatement expr)
    {
        await Resolve(expr.Initializer);
        await Resolve(expr.Condition);
        await Resolve(expr.Body);
        await Resolve(expr.Increment);
        
        return null!;
    }

    public async Task<object> Visit(SxContinueStatement expr)
    {
        return null!;
    }

    public async Task<object> Visit(SxFunctionStatement expr)
    {
        Declare(expr.Name);
        Define(expr.Name);
        await ResolveFunction(expr);

        return null!;
    }

    public async Task<object> Visit(SxReturnStatement expr)
    {
        await Resolve(expr.Value);
        return null!;
    }

    public async Task Resolve(List<SxStatement> statements)
    {
        foreach (SxStatement statement in statements)
        {
            await Resolve(statement);
        }
    }

    public async Task Resolve(SxStatement statement)
    {
        await statement.Accept(this);
    }
    
    public async Task Resolve(SxExpression expr)
    {
        if (expr == null)
        {
            return;
        }
        
        await expr.Accept(this);
    }

    void BeginScope()
    {
        Scopes.Push(new Dictionary<string, bool>());
    }

    void EndScope()
    {
        Scopes.Pop();
    }

    void Declare(SxToken token)
    {
        if (token == null)
        {
            return;
        }
        
        if (Scopes.Count == 0)
        {
            return;
        }

        Dictionary<string, bool> scope = Scopes.Peek();

        if (scope == null)
        {
            return;
        }
        
        if (!scope.ContainsKey(token.Lexeme))
        {
            scope.Add(token.Lexeme, false);   
        }
    }

    void Define(SxToken token)
    {
        if (token == null)
        {
            return;
        }
        
        if (Scopes.Count == 0)
        {
            return;
        }

        Dictionary<string, bool> scope = Scopes.Peek();
        if (!scope.ContainsKey(token.Lexeme))
        {
            scope.Add(token.Lexeme, true);   
        }
    }

    async Task ResolveFunction(SxFunctionStatement func)
    {
        BeginScope();
        foreach (SxToken param in func.FunctionExpression.Pars.Select(x => x.Name))
        {
            Declare(param);
            Define(param);
        }

        await Resolve(func.FunctionExpression.Body);
        EndScope();
    }
    
    public async Task<object> Visit(SxFunctionExpression expr)
    {
        BeginScope();
        foreach (SxToken param in expr.Pars.Select(x => x.Name))
        {
            Declare(param);
            Define(param);
        }

        await Resolve(expr.Body);
        EndScope();

        return null!;
    }

    void ResolveLocal(SxExpression expression, SxToken name)
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
        {
            if (Scopes.ToArray()[i].ContainsKey(name.Lexeme))
            {
                Interpreter.Resolve(expression, Scopes.Count - 1 - i);
                return;
            }
        }
    }
}