using System.Text;
using SxScript.Exceptions;
using SxScript.SxFFI;
using SxScript.SxStatements;

namespace SxScript;

public class SxInterpreter : SxExpression.ISxExpressionVisitor<object>, SxStatement.ISxStatementVisitor<object>
{
    public SxEnvironment Globals = new SxEnvironment(null);
    public SxEnvironment Environment;
    public SortedList<int, SxStatement> AllStatements = new SortedList<int, SxStatement>();
    public Dictionary<string, SxLabelStatement> Labels = new Dictionary<string, SxLabelStatement>();
    public SxStatement CurrentStatement = null;
    public bool Jump = false;
    public SxToken JumpDestination = null;
    public int CurrentStatementIndex = 0;
    public StringBuilder StdIn { get; set; } = null;
    public StringBuilder StdOut { get; set; } = null;
    public int LoopDepth = 0;
    public int CallDepth = 0;
    public Stack<SxStatement> LoopStack = new Stack<SxStatement>();
    public Stack<SxExpression> CallStack = new Stack<SxExpression>();
    public SxStatement.ISxLoopingStatement CurrentLoopStatement = null;
    public SxStatement.ISxCallStatement CurrentCallStatement = null;
    public Dictionary<SxExpression, int> Locals = new Dictionary<SxExpression, int>();

    public SxInterpreter()
    {
        Environment = Globals;
        Globals.SetIfDefined("clock", new SxNativeFunction<DateTime>(() =>
        {
            return DateTime.Now;
        }));
        
        Globals.SetIfDefined("async_clock", new SxNativeAsyncFunction<DateTime>(async () =>
        {
            return await Task.Run(() => DateTime.Now);
        }));
    }
    
    public async Task<object?> Visit(SxBinaryExpression expr)
    {
        object right = await EvaluateAsync(expr.Right);
        object left = await EvaluateAsync(expr.Left);

        if ((right is string rightStr || left is string leftStr) && expr.Operator.Type == SxTokenTypes.Plus)
        {
            return $"{left?.ToString() ?? ""}{right?.ToString() ?? ""}";
        }

        object? val = PerformArithmeticOperation(left, right, expr.Operator);
        return val;
    }

    public async Task<object> Visit(SxUnaryExpression expr)
    {
        object right = await EvaluateAsync(expr.Expr);
        switch (expr.Operator.Type)
        {
            case SxTokenTypes.Minus:
            {
                if (right is double dbl)
                {
                    return -dbl;   
                }

                if (right is int it)
                {
                    return -it;
                }
                
                break;
            }
            case SxTokenTypes.Plus:
            {
                return right;
            }
            case SxTokenTypes.Exclamation:
            {
                return !ObjectIsTruthy(right);
            }
        }

        return null;
    }

    public object? PerformArithmeticOperation(object left, object right, SxToken op)
    {
        AssertTypeOf(left, "Levá strana výrazu musí být int/double/bool", op, typeof(int), typeof(double), typeof(bool));
        AssertTypeOf(right, "Pravá strana výrazu musí být int/double/bool", op,typeof(int), typeof(double), typeof(bool));
        
        return op.Type switch
        {
            SxTokenTypes.Plus => (object) ((dynamic) left + (dynamic) right),
            SxTokenTypes.Minus => (object) ((dynamic) left - (dynamic) right),
            SxTokenTypes.Star => (object) ((dynamic) left * (dynamic) right),
            SxTokenTypes.Slash => (object) ((dynamic) left / (dynamic) right),
            SxTokenTypes.Percent => (object) ((dynamic) left % (dynamic) right),
            SxTokenTypes.Greater => (object) ((dynamic) left > (dynamic) right),
            SxTokenTypes.GreaterEqual => (object) ((dynamic) left >= (dynamic) right),
            SxTokenTypes.Less => (object) ((dynamic) left < (dynamic) right),
            SxTokenTypes.LessEqual => (object) ((dynamic) left <= (dynamic) right),
            SxTokenTypes.EqualEqual => ObjectIsEqual(left, right),
            SxTokenTypes.ExclamationEqual => !ObjectIsEqual(left, right),
            _ => null
        };
    }

    public bool AssertTypeOf(object obj, string errMsg, SxToken closestToken, params Type[] types)
    {
        if (obj == null)
        {
            return false;
        }
        
        Type objT = obj.GetType();
        
        for (int i = 0; i < types.Length; i++)
        {
            if (objT == types[i])
            {
                return true;
            }
        }

        return false;
    } 

    public bool ObjectIsEqual(object? left, object? right)
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null)
        {
            return false;
        }

        return left.Equals(right);
    }
    
    public bool ObjectIsTruthy(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is bool bl)
        {
            return bl;
        }

        if (obj is int i)
        {
            return i > 0;
        }

        if (obj is double d)
        {
            return d > 0;
        }

        return true;
    }

    public void Resolve(SxExpression expression, int depth)
    {
        Locals.Add(expression, depth);
    }

    public async Task<object> Visit(SxLiteralExpression expr)
    {
        return expr.Value;
    }

    public async Task<object> Visit(SxGroupingExpression expr)
    {
        return await EvaluateAsync(expr.Expr);
    }

    public object? LookUpVariable(SxToken token, SxExpression expression)
    {
        if (Locals.TryGetValue(expression, out int distance))
        {
            return Environment.GetAt(distance, token.Lexeme);
        }

        return Globals.Get(token.Lexeme);
    }

    // expr ? caseTrue : caseFalse
    public async Task<object> Visit(SxTernaryExpression expr)
    {
        object iff = await EvaluateAsync(expr.Expr);
        if (iff is bool bl)
        {
            return await EvaluateAsync(bl ? expr.CaseTrue : expr.CaseFalse);
        }

        return await EvaluateAsync(expr.CaseTrue);
    }
    
    public async Task<object> Visit(SxVarExpression expr)
    {
        return LookUpVariable(expr.Name, expr)!;
    }

    public async Task<object> Visit(SxAssignExpression expr)
    {
        object? val = await EvaluateAsync(expr.Value);
        if (Locals.TryGetValue(expr, out int distance))
        {
            Environment.SetAtIfDefined(distance, expr.Name.Lexeme, val);
        }
        else
        {
            Globals.SetIfDefined(expr.Name.Lexeme, val);
        }
        
        return null!;
    }

    public async Task<object> Visit(SxLogicalExpression expr)
    {
        object left = await EvaluateAsync(expr.Left);

        if (expr.Operator.Type == SxTokenTypes.KeywordOr)
        {
            if (ObjectIsTruthy(left))
            {
                return left;
            }
        }
        else
        {
            if (!ObjectIsTruthy(left))
            {
                return left;
            }
        }

        return await EvaluateAsync(expr.Right);
    }

    public async Task<object> Visit(SxPostfixExpression expr)
    {
        object right = await EvaluateAsync(expr.Expr);
        object val = right;
        
        switch (expr.Operator.Type)
        {
            case SxTokenTypes.MinusMinus:
            {
                if (right is double dbl)
                {
                    dbl--;
                    val = dbl;   
                }

                if (right is int it)
                {
                    it--;
                    val = it;
                }
                
                break;
            }
            case SxTokenTypes.PlusPlus:
            {
                if (right is double dbl)
                {
                    dbl++;
                    val = dbl;   
                }

                if (right is int it)
                {
                    it++;
                    val = it;
                }
                
                break;
            }
        }

        if (expr.Expr is SxVarExpression varExpr)
        {
            Environment.SetIfDefined(varExpr.Name.Lexeme, val);   
        }
        
        return val;
    }

    public async Task<object?> Visit(SxCallExpression expr)
    {
        object callee = await EvaluateAsync(expr.Callee);
        
        CallDepth++;
        CallStack.Push(expr.Callee);

        if (callee is SxStatement.ISxCallStatement callable)
        {
            CurrentCallStatement = callable;
        }
        
        object? toRet = null!;
        List<SxResolvedCallArgument> arguments = new List<SxResolvedCallArgument>();

        foreach (SxCallArgument argument in expr.Arguments)
        {
            object? argVal = await EvaluateAsync(argument.Value);
            string? argName = null;

            if (argument.Name != null)
            {
                if (argument.Name is SxVarExpression sxVar)
                {
                    argName = sxVar.Name.Lexeme;
                }
                else
                {
                    argName = (await EvaluateAsync(argument.Name))?.ToString() ?? null;   
                }
            }
            
            arguments.Add(new SxResolvedCallArgument(argVal, argName));
        }
        
        if (callee is SxExpression.ISxCallable fn)
        {
            // [todo] remove me
            await fn.PrepareCallAsync(this, arguments);
            toRet = fn.Call(this);
        }
        else if (callee is SxExpression.ISxAsyncCallable asyncFn)
        {
            toRet = expr.Await ? await asyncFn.CallAsync(this, arguments) : asyncFn.CallAsync(this, arguments);
        }

        CallDepth--;
        CallStack.Pop();

        return toRet;
    }

    public async Task<object> Visit(SxArgumentDeclrExpression expr)
    {
        return null!;
    }

    public void WriteLine(object str)
    {
        if (StdOut == null)
        {
            Console.WriteLine(str);
        }
        else
        {
            StdOut.Append($"{str}\n");
        }
    }

    public async Task<object?> Evaluate(List<SxStatement> statements, StringBuilder stdout = null, StringBuilder stdin = null)
    {
        AllStatements = new SortedList<int, SxStatement>();
        for (int i = 0; i < statements.Count; i++)
        {
            AllStatements.Add(i, statements[i]);
        }

        StdIn = stdin;
        StdOut = stdout;

        for (CurrentStatementIndex = 0; CurrentStatementIndex < statements.Count; CurrentStatementIndex++)
        {
            SxStatement statement = statements[CurrentStatementIndex];
            object? statementResult = await ExecuteAsync(statement);

            if (Jump)
            {
                Jump = false;
                if (JumpDestination != null)
                {
                    if (Labels.TryGetValue(JumpDestination.Lexeme, out SxLabelStatement? lblStmt))
                    {
                        int index = AllStatements.IndexOfValue(lblStmt);
                        if (index >= 0)
                        {
                            CurrentStatementIndex = index - 1;
                            JumpDestination = null;
                        }
                    }
                }
            }
        }
        
        return null;
    }

    public object Evaluate(SxExpression expression)
    {
        return expression?.Accept(this) ?? null!;
    }
    
    public async Task<object> EvaluateAsync(SxExpression expression)
    {
        return await expression?.Accept(this)! ?? null!;
    }
    
    public object Execute(SxStatement statement)
    {
        return statement.Accept(this);
    }
    
    public async Task<object?> ExecuteAsync(SxStatement statement)
    {
        return await statement.Accept(this);
    }

    public async Task<object> Visit(SxExpressionStatement expr)
    {
        await EvaluateAsync(expr.Expr);
        return null!;
    }

    public async Task<object?> Visit(SxPrintStatement expr)
    {
        object val = await EvaluateAsync(expr.Expr);
        WriteLine(val);
        return null!;
    }

    public async Task<object> Visit(SxVarStatement expr)
    {
        object val = await EvaluateAsync(expr.Expr);
        Environment.Set(expr.Name.Lexeme, val);
        return null!;
    }

    public async Task<object> Visit(SxBlockStatement expr)
    {
        await ExecuteBlockAsync(expr, expr.Statements, new SxEnvironment(Environment));
        return null!;
    }

    public async Task<object> Visit(SxIfStatement expr)
    {
        if (ObjectIsTruthy(await EvaluateAsync(expr.Condition)))
        {
            if (expr.ThenBranch != null)
            {
                await ExecuteAsync(expr.ThenBranch);   
            }
        }
        else if (expr.ElseBranch != null)
        {
            await ExecuteAsync(expr.ElseBranch);
        }

        return null!;
    }

    public async Task<object> Visit(SxWhileStatement expr)
    {
        LoopDepth++;
        LoopStack.Push(expr.Body);
        CurrentLoopStatement = expr;
        
        while (ObjectIsTruthy(await EvaluateAsync(expr.Expr)))
        {
            if (expr.Continue)
            {
                expr.Continue = false;
                continue;
            }

            if (expr.Break)
            {
                expr.Break = false;
                break;
            }
            
            await ExecuteAsync(expr.Body);
        }

        LoopStack.Pop();
        LoopDepth--;
        return null!;
    }

    public async Task<object> Visit(SxBreakStatement expr)
    {
        CurrentLoopStatement.Break = true;
        if (CurrentLoopStatement.Body is SxStatement.ISxBreakableStatement breakableStatement)
        {
            breakableStatement.Break = true;
        }
        
        return null!;
    }
    
    public async Task<object> Visit(SxContinueStatement expr)
    {
        CurrentLoopStatement.Continue = true;
        if (CurrentLoopStatement.Body is SxStatement.ISxBreakableStatement breakableStatement)
        {
            breakableStatement.Continue = true;
        }
        
        return null!;
    }

    public async Task<object> Visit(SxFunctionStatement expr)
    {
        SxFunction fn = new SxFunction(expr, expr.Body, Environment);
        Environment.SetIfDefined(expr.Name.Lexeme, fn);
        return null!;
    }

    public async Task<object> Visit(SxReturnStatement expr)
    {
        object val = null!;
        if (expr.Value != null!)
        {
            val = await EvaluateAsync(expr.Value);
        }

        if (CurrentCallStatement != null)
        {
            CurrentCallStatement.Return = true;

            if (CurrentCallStatement is SxFunction sxFn)
            {
                sxFn.Block.Return = true;
                sxFn.Block.ReturnValue = val;
            }
        }

        return val;
    }

    public async Task<object> Visit(SxLabelStatement expr)
    {
        if (!Labels.TryGetValue(expr.Identifier.Lexeme, out _))
        {
            Labels.Add(expr.Identifier.Lexeme, expr);   
        }
        
        await ExecuteAsync(expr.Statement);
        return null!;
    }

    public async Task<object> Visit(SxGotoStatement expr)
    {
        Jump = true;
        JumpDestination = expr.Identifier;
        return null!;
    }

    public async Task<object> Visit(SxForStatement expr)
    {
        LoopDepth++;
        LoopStack.Push(expr.Body);
        CurrentLoopStatement = expr;
        
        await ExecuteAsync(expr.Initializer);

        while (ObjectIsTruthy(await EvaluateAsync(expr.Condition)))
        {
            if (expr.Continue)
            {
                expr.Continue = false;
                continue;
            }

            if (expr.Break)
            {
                expr.Break = false;
                break;
            }
            
            await ExecuteAsync(expr.Body);
            await EvaluateAsync(expr.Increment);
        }

        LoopStack.Pop();
        LoopDepth--;
        return null!;
    }

    public object? ExecuteBlock(SxBlockStatement blockStatement, List<SxStatement> statements, SxEnvironment environment)
    {
        bool didReturn = false;
        
        SxEnvironment previous = Environment;
        Environment = environment;
        foreach (SxStatement statement in statements)
        {
            Execute(statement);
            
            if (blockStatement.Break || blockStatement.Continue || blockStatement.Return)
            {
                blockStatement.Break = false;
                blockStatement.Continue = false;
                blockStatement.Return = false;
                didReturn = true;
                break;
            }
        }

        Environment = previous;

        if (didReturn)
        {
            object? obj = blockStatement.ReturnValue;
            blockStatement.ReturnValue = null!;
            return obj;
        }

        return null;
    }
    
    public async Task<object?> ExecuteBlockAsync(SxBlockStatement blockStatement, List<SxStatement> statements, SxEnvironment environment)
    {
        bool didReturn = false;
        
        SxEnvironment previous = Environment;
        Environment = environment;
        foreach (SxStatement statement in statements)
        {
            await ExecuteAsync(statement);
            
            if (blockStatement.Break || blockStatement.Continue || blockStatement.Return)
            {
                blockStatement.Break = false;
                blockStatement.Continue = false;
                blockStatement.Return = false;
                didReturn = true;
                break;
            }
        }

        Environment = previous;
        
        if (didReturn)
        {
            object? obj = blockStatement.ReturnValue;
            blockStatement.ReturnValue = null!;
            return obj;
        }

        return null;
    }
}