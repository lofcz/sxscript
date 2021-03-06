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
    public SxBlockStatement CurrentBlock { get; set; }

    public SxInterpreter()
    {
        Environment = Globals;
        Globals.SetIfDefined("clock", new SxNativeFunction<DateTime>(() => { return DateTime.Now; }));

        Globals.SetIfDefined("async_clock", new SxNativeAsyncFunction<DateTime>(async () => { return await Task.Run(() => DateTime.Now); }));
    }

    public async Task<object?> Visit(SxBinaryExpression expr)
    {
        object? right = await EvaluateAsync(expr.Right);
        object? left = await EvaluateAsync(expr.Left);

        if ((right is string rightStr || left is string leftStr) && expr.Operator.Type == SxTokenTypes.Plus)
        {
            return $"{left?.ToString() ?? ""}{right?.ToString() ?? ""}";
        }

        object? val = PerformArithmeticOperation(left, right, expr.Operator);
        return val;
    }

    public async Task<object?> Visit(SxUnaryExpression expr)
    {
        object? right = await EvaluateAsync(expr.Expr);
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
        AssertTypeOf(left, "Lev?? strana v??razu mus?? b??t int/double/bool", op, typeof(int), typeof(double), typeof(bool));
        AssertTypeOf(right, "Prav?? strana v??razu mus?? b??t int/double/bool", op, typeof(int), typeof(double), typeof(bool));

        return op.Type switch
        {
            SxTokenTypes.Plus => (object) ((dynamic) left + (dynamic) right),
            SxTokenTypes.Minus => (object) ((dynamic) left - (dynamic) right),
            SxTokenTypes.Star => (object) ((dynamic) left * (dynamic) right),
            SxTokenTypes.Slash => (object) ((dynamic) left / (dynamic) right),
            SxTokenTypes.Percent => (object) ((dynamic) left % (dynamic) right),
            SxTokenTypes.Caret => (object) Math.Pow((dynamic) left, (dynamic) right),
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

    public async Task<object?> Visit(SxGroupingExpression expr)
    {
        return await EvaluateAsync(expr.Expr);
    }

    public object? LookUpVariable(SxToken token, SxExpression expression)
    {
        object? val = null;

        if (Locals.TryGetValue(expression, out int distance))
        {
            val = Environment.GetAt(distance, token.Lexeme);
        }
        else
        {
            val = Globals.Get(token.Lexeme);
        }

        object? ReturnArrayValue(SxVarExpression? varExpression, SxArray array)
        {
            if (varExpression == null || varExpression.ArrayExpr == null)
            {
                return null;
            }

            Evaluate(varExpression.ArrayExpr);
            if (varExpression.ArrayExpr?.ArrayExprResolved != null)
            {
                return array.GetValueChained(varExpression.ArrayExpr.ArrayExprResolved);
            }

            return null;
        }

        if (expression is SxVarExpression varExpression)
        {
            if (varExpression.ArrayExpr != null)
            {
                if (val is SxArray array)
                {
                    return ReturnArrayValue(varExpression, array);
                }

                if (val is SxInstance inst)
                {
                    //return inst.Fields;

                    if (varExpression.ArrayName != null)
                    {
                        object? field = inst.Fields[varExpression.ArrayName.Lexeme];
                        if (field is SxArray fieldArray)
                        {
                            return ReturnArrayValue(varExpression, fieldArray);
                        }
                    }
                }
            }
        }

        return val;
    }

    // expr ? caseTrue : caseFalse
    public async Task<object?> Visit(SxTernaryExpression expr)
    {
        object? iff = await EvaluateAsync(expr.Expr);
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

    async Task<List<object?>?> ResolveArray(SxArrayExpression? arrayExpression)
    {
        if (arrayExpression == null)
        {
            return null;
        }

        List<object?> resolvedAccess = new List<object?>();

        if (arrayExpression.ArrayExpr != null)
        {
            for (int i = 0; i < arrayExpression.ArrayExpr.Count; i++)
            {
                if (arrayExpression.ArrayExpr[i] == null!)
                {
                    resolvedAccess.Add(null);
                }
                else
                {
                    object? obj = await EvaluateAsync(arrayExpression.ArrayExpr[i]);
                    resolvedAccess.Add(obj);
                }
            }
        }

        arrayExpression.ArrayExprResolved = resolvedAccess;
        return resolvedAccess;
    }

    public async Task<object> Visit(SxAssignExpression expr)
    {
        object? val = await EvaluateAsync(expr.Value);
        if (Locals.TryGetValue(expr, out int distance))
        {
            Environment.SetAtIfDefined(distance, expr.Name.Lexeme, val, expr.Operator, expr.ArrayExpr == null ? null : await ResolveArray(expr.ArrayExpr));
        }
        else
        {
            Globals.SetIfDefined(expr.Name.Lexeme, val, expr.Operator, expr.ArrayExpr == null ? null : await ResolveArray(expr.ArrayExpr));
        }

        return null!;
    }

    public async Task<object?> Visit(SxLogicalExpression expr)
    {
        object? left = await EvaluateAsync(expr.Left);

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

    public async Task<object?> Visit(SxPostfixExpression expr)
    {
        if (expr.Expr == null! && expr.Operator.Type == SxTokenTypes.LeftBracket)
        {
            SxArray? array = new SxArray(null);
            if (expr.Expr is SxVarExpression arrayDeclr)
            {
                Environment.SetIfDefined(arrayDeclr.Name.Lexeme, array);
            }

            return array;
        }

        if (expr.Expr != null && expr.Operator.Type == SxTokenTypes.LeftBracket)
        {
            if (expr.PostfixExpr != null)
            {
                object? arrayCandidate = await EvaluateAsync(expr.Expr);
                if (arrayCandidate is SxArray array)
                {
                    object? index = await EvaluateAsync(expr.PostfixExpr);
                    if (array.IndexedValues.ContainsKey(index))
                    {
                        return array.IndexedValues[index]!;
                    }

                    return null!;
                }
            }
        }

        object? right = await EvaluateAsync(expr.Expr!);
        object? val = right;

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
        object? callee = await EvaluateAsync(expr.Callee);

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
            toRet = await fn.PrepareAndCallAsync(this, arguments);
        }
        else if (callee is SxExpression.ISxAsyncCallable asyncFn)
        {
            toRet = expr.Await ? await asyncFn.CallAsync(this, arguments) : asyncFn.CallAsync(this, arguments);
        }

        CallDepth--;
        CallStack.Pop();

        return toRet;
    }

    public async Task<object> Visit(SxFunctionExpression expr)
    {
        return new SxFunction(new SxFunctionStatement(null, expr, false), expr.Body, Environment, false);
    }

    public async Task<object> Visit(SxGetExpression expr)
    {

        if (expr.Object is SxVarExpression varExpression)
        {
            varExpression.ArrayExpr = expr.ArrayExpr;
            varExpression.ArrayName = expr.Name;
        }

        object? obj = await EvaluateAsync(expr.Object);
        if (obj is SxInstance instance)
        {
            if (expr.ArrayExpr == null)
            {
                return instance.Get(expr.Name)!;   
            }

            if (expr.ArrayExpr.ArrayExpr.Count > 0 && instance.Fields.ContainsKey(expr.Name.Lexeme))
            {
                object? val = instance.Fields[expr.Name.Lexeme];
                List<object?> resolved = new List<object?>();

                for (int i = 0; i < expr.ArrayExpr.ArrayExpr.Count; i++)
                {
                    object? r = await EvaluateAsync(expr.ArrayExpr.ArrayExpr[i]);
                    if (r == null)
                    {
                        if (expr.ArrayExpr.ArrayExpr[i] is SxVarExpression varExpr)
                        {
                            if (Locals.TryGetValue(expr.ArrayExpr.ArrayExpr[i], out int distance))
                            {
                                r = Environment.GetAt(distance, varExpr.Name.Lexeme);
                            }   
                        }
                    }
                    resolved.Add(r);
                }
                
                if (val is SxArray arr)
                {
                    return arr.GetValueChained(resolved)!;
                }
            }
            
            return instance;
        }

        if (expr.ArrayExpr != null)
        {
            return obj!;
        }

        // [todo] pokus o p????stup k vlastnosti na n????em jin??m ne?? instanci

        return null!;
    }

    public async Task<object?> Visit(SxSetExpression expr)
    {
        object? obj = await EvaluateAsync(expr.Object);
        if (obj is SxInstance instance)
        {
            object? val = await EvaluateAsync(expr.Value);
            List<object?>? resolvedArray = null;

            if (expr.ArrayExpr != null)
            {
                await EvaluateAsync(expr.ArrayExpr);
                resolvedArray = expr.ArrayExpr.ArrayExprResolved;
            }

            instance.Set(expr.Name, val, expr.Operator, resolvedArray);
            return val;
        }

        // [todo] pokus o z??pis vlastnosti do n????eho jin??ho ne?? instance
        return null!;
    }

    public async Task<object> Visit(SxThisExpression expr)
    {
        if (expr.IsInvalid)
        {
            return null!;
        }

        return LookUpVariable(expr.Keyword, expr)!;
    }

    public async Task<object> Visit(SxSuperExpression expr)
    {
        SxFunction? method = null;

        if (Locals.TryGetValue(expr, out int distance))
        {
            object? supercls = Environment.GetAt(distance, "base");
            if (supercls is SxClass superclsCasted)
            {
                object? obj = Environment.GetAt(distance - 1, "this");
                if (obj is SxInstance instance)
                {
                    method = superclsCasted.FindMethod(expr.Method.Lexeme);
                    if (method != null)
                    {
                        //method.Closure = Environment;
                        return method.Bind(instance);
                    }
                }
            }
        }

        // [todo] pokus o zavol??n?? nedefinovan?? metody na p??edkovi

        return null!;
    }

    public async Task<object> Visit(SxArrayExpression expr)
    {
        SxArray array = new SxArray(null);
        List<object?> resolvedAccess = new List<object?>();
        object? lastObj = null;

        if (expr.ArrayExpr != null)
        {
            for (int i = 0; i < expr.ArrayExpr.Count; i++)
            {
                if (expr.ArrayExpr[i] == null!)
                {
                    array.IndexedValues[i] = null;
                    resolvedAccess.Add(null);
                }
                else
                {
                    object? obj = await EvaluateAsync(expr.ArrayExpr[i]);
                    lastObj = obj;
                    resolvedAccess.Add(obj);
                    array.IndexedValues[i] = obj;
                }
            }
        }

        expr.ArrayExprResolved = resolvedAccess;
        expr.Array = array;
        return array;
    }

    public async Task<object> Visit(SxArgumentDeclrExpression expr)
    {
        return null!;
    }

    public void WriteLine(object? str)
    {
        if (StdOut == null)
        {
            Console.WriteLine(str);
        }
        else
        {
            StdOut.Append($"{str ?? "null"}\n");
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

    public async Task<object?> EvaluateAsync(SxExpression expression)
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
        object? val = await EvaluateAsync(expr.Expr);
        WriteLine(val);
        return null!;
    }

    public async Task<object> Visit(SxVarStatement expr)
    {
        object? val = await EvaluateAsync(expr.Expr);
        Environment.Set(expr.Name.Lexeme, val);
        return null!;
    }

    public async Task<object> Visit(SxBlockStatement expr)
    {
        await ExecuteBlockAsync(expr, expr.Statements, expr.GeneratesScope ? new SxEnvironment(Environment) : Environment);
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
        SxFunction fn = new SxFunction(expr, expr.FunctionExpression.Body, Environment, false);
        Environment.SetIfDefined(expr.Name.Lexeme, fn);
        return null!;
    }

    public async Task<object?> Visit(SxReturnStatement expr)
    {
        object? val = null!;
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
                CurrentBlock.Return = true;
                CurrentBlock.ReturnValue = val;
            }
        }

        return val;
    }

    public async Task<object> Visit(SxClassStatement expr)
    {
        object? superclass = null;
        bool superclassOk = true;
        SxClass? castedSuperclass = null;

        if (expr.Superclass != null && expr.SuperclassIsValid)
        {
            superclass = await EvaluateAsync(expr.Superclass);

            if (superclass is not SxClass superCls)
            {
                // [todo] u??ivatel se pokusil zd??dit t????du od n????eho jin??ho ne?? t????dy
                superclassOk = false;
            }
            else
            {
                castedSuperclass = superCls;
            }
        }

        Environment.DefineOrRedefineEmpty(expr.Name.Lexeme);

        if (expr.Superclass != null && expr.SuperclassIsValid)
        {
            Environment = new SxEnvironment(Environment);
            Environment.DefineOrRedefineAndAssign("base", superclass);
        }

        Dictionary<string, SxFunction> classMethods = new Dictionary<string, SxFunction>();
        Dictionary<string, SxFunction> methods = new Dictionary<string, SxFunction>();
        Dictionary<string, object?> fields = new Dictionary<string, object?>();

        foreach (SxFunctionStatement classMethod in expr.ClassMethods)
        {
            SxFunction func = new SxFunction(classMethod, classMethod.FunctionExpression.Body, Environment, classMethod.Name.Lexeme == expr.Name.Lexeme);
            classMethods.Add(classMethod.Name.Lexeme, func);
        }

        foreach (SxFunctionStatement method in expr.Methods)
        {
            SxFunction func = new SxFunction(method, method.FunctionExpression.Body, Environment, method.Name.Lexeme == expr.Name.Lexeme);
            methods.Add(method.Name.Lexeme, func);
        }

        foreach (SxVarStatement field in expr.Fields)
        {
            fields.Add(field.Name.Lexeme, await EvaluateAsync(field.Expr));
        }

        SxClass metaclass = new SxClass(null, castedSuperclass, $"{expr.Name.Lexeme} metaclass", classMethods, fields);
        SxClass cls = new SxClass(metaclass, castedSuperclass, expr.Name.Lexeme, methods, fields);

        if (expr.Superclass != null && expr.SuperclassIsValid)
        {
            Environment = Environment.Enclosing!;
        }

        Environment.DefineOrRedefineAndAssign(expr.Name.Lexeme, cls);
        return null!;
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

        SxBlockStatement previousBlock = CurrentBlock;
        CurrentBlock = blockStatement;
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
        CurrentBlock = previousBlock;

        if (didReturn)
        {
            object? obj = blockStatement.ReturnValue;
            blockStatement.ReturnValue = null!;
            return obj;
        }

        return null;
    }
}