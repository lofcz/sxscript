using SxScript.SxFFI;
using SxScript.SxStatements;

namespace SxScript.SxSa;

public class SxResolver : SxExpression.ISxExpressionVisitor<object>, SxStatement.ISxStatementVisitor<object>
{
    public Stack<Dictionary<string, SxResolverVariable>> Scopes { get; set; }
    public SxInterpreter Interpreter { get; set; }
    public SxClassTypes CurrentClass = SxClassTypes.None;

    public SxResolver(SxInterpreter interpreter)
    {
        Interpreter = interpreter;
        Scopes = new Stack<Dictionary<string, SxResolverVariable>>();
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

        ResolveLocal(expr, expr.Name, true);
        return null!;
    }

    public async Task<object> Visit(SxAssignExpression expr)
    {
        await Resolve(expr.Value);
        ResolveLocal(expr, expr.Name, false);
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
        else
        {
            int cx = 1;
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
        await ResolveFunction(expr, SxFunctionTypes.Function);

        return null!;
    }

    public async Task<object> Visit(SxReturnStatement expr)
    {
        await Resolve(expr.Value);
        return null!;
    }

    public async Task<object> Visit(SxClassStatement expr)
    {
        SxClassTypes enclosingClass = CurrentClass;
        CurrentClass = SxClassTypes.Class;
        
        Declare(expr.Name);
        Define(expr.Name);

        bool canResolveSuperclass = true;
        
        if (expr.Superclass != null && expr.Name.Lexeme == expr.Superclass.Name.Lexeme)
        {
            // [todo] class Foo : Foo {}, uživatel se pokusil zdědit třídu samu od sebe
            expr.SuperclassIsValid = false;
            canResolveSuperclass = false;
        }
        
        if (expr.Superclass != null && canResolveSuperclass)
        {
            CurrentClass = SxClassTypes.Subclass;
            await Resolve(expr.Superclass);
        }
        
        
        BeginScope();
        Scopes.Peek()?.Add("this", new SxResolverVariable(expr.Name, SxResolverVariableStates.Used));

        if (expr.Superclass != null && expr.SuperclassIsValid)
        {
            BeginScope();
            Scopes.Peek().Add("base", new SxResolverVariable(new SxToken(SxTokenTypes.Identifier, "true", true, 0), SxResolverVariableStates.Used));
        }

        foreach (SxFunctionStatement method in expr.Methods)
        {
            SxFunctionTypes methodType = SxFunctionTypes.Method;
            if (method.Name.Lexeme == expr.Name.Lexeme)
            {
                methodType = SxFunctionTypes.Constructor;
            }

            await ResolveFunction(method, methodType);
        }
        
        foreach (SxFunctionStatement method in expr.ClassMethods)
        {
           // BeginScope();
           // Scopes.Peek().Add("this", new SxResolverVariable(new SxToken(SxTokenTypes.KeywordThis, "this", "this", 0), SxResolverVariableStates.Used));
            await ResolveFunction(method, SxFunctionTypes.Method);
           // EndScope();
        }

        foreach (SxVarStatement field in expr.Fields)
        {
            ResolveLocal(field.Expr, field.Name, true);
        }

        EndScope();

        if (expr.Superclass != null && expr.SuperclassIsValid)
        {
            EndScope();
        }
        
        CurrentClass = enclosingClass;
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
        Scopes.Push(new Dictionary<string, SxResolverVariable>());
    }

    void EndScope()
    {
        Dictionary<string, SxResolverVariable> scope = Scopes.Pop();

        foreach (KeyValuePair<string, SxResolverVariable> entry in scope)
        {
            if (entry.Value.State == SxResolverVariableStates.Defined)
            {
                // [todo] proměnná nebyla nikdy použita, warning
            }
        }
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

        Dictionary<string, SxResolverVariable> scope = Scopes.Peek();

        if (scope == null)
        {
            return;
        }
        
        if (!scope.ContainsKey(token.Lexeme))
        {
            scope.Add(token.Lexeme, new SxResolverVariable(token, SxResolverVariableStates.Declared));   
        }
        else
        {
            scope[token.Lexeme] = new SxResolverVariable(token, SxResolverVariableStates.Declared);
        }
    }

    void Define(SxToken token)
    {
        if (token == null!)
        {
            return;
        }
        
        if (Scopes.Count == 0)
        {
            return;
        }

        Dictionary<string, SxResolverVariable> scope = Scopes.Peek();
        if (!scope.ContainsKey(token.Lexeme))
        {
            scope.Add(token.Lexeme, new SxResolverVariable(token, SxResolverVariableStates.Defined));   
        }
        else
        {
            scope[token.Lexeme] = new SxResolverVariable(token, SxResolverVariableStates.Defined);
        }
    }

    async Task ResolveFunction(SxFunctionStatement func, SxFunctionTypes type)
    {
        if (type != SxFunctionTypes.Method)
        {
            BeginScope();   
        }
        
        foreach (SxToken param in func.FunctionExpression.Pars.Select(x => x.Name))
        {
            Declare(param);
            Define(param);
        }

        await Resolve(func.FunctionExpression.Body);
        
        if (type != SxFunctionTypes.Method)
        {
            EndScope();   
        }
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

    public async Task<object> Visit(SxGetExpression expr)
    {
        await Resolve(expr.Object);
        return null!;
    }

    public async Task<object> Visit(SxSetExpression expr)
    {
        await Resolve(expr.Value);
        await Resolve(expr.Object);
        return null!;
    }

    public async Task<object> Visit(SxThisExpression expr)
    {
        if (CurrentClass == SxClassTypes.None)
        {
            expr.IsInvalid = true;
            // [todo] pokus o použití this mimo kontext třídy
            return null!;
        }
        
        ResolveLocal(expr, expr.Keyword, true);
        return null!;
    }

    public async Task<object> Visit(SxSuperExpression expr)
    {
        if (CurrentClass == SxClassTypes.None)
        {
            // [todo] base není možné použít mimo třídu
        }

        if (CurrentClass == SxClassTypes.Class)
        {
            // [todo] base není možné použít ve třídě, která nedědí z žádné třídy
        }
        
        ResolveLocal(expr, expr.Keyword, true);
        return null!;
    }

    public async Task<object> Visit(SxArrayExpression expr)
    {
        if (expr.ArrayExpr != null)
        {
            foreach (SxExpression? ex in expr.ArrayExpr)
            {
                if (ex != null!)
                {
                    await Resolve(ex);         
                }
            }
        }

        return null!;
    }

    void ResolveLocal(SxExpression expression, SxToken name, bool isUsed)
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
        {
            Dictionary<string, SxResolverVariable> castedScope = Scopes.ToArray()[i];
            
            if (castedScope.ContainsKey(name.Lexeme))
            {
                Interpreter.Resolve(expression, Scopes.Count - 1 - i);

                if (isUsed)
                {
                    castedScope[name.Lexeme].State = SxResolverVariableStates.Used;
                }
                
                return;
            }
        }
    }
}