using SxScript.Exceptions;
using SxScript.SxFFI;
using SxScript.SxStatements;

namespace SxScript;

public class SxParser<T>
{
    private List<SxToken> Tokens { get; set; }
    private int Current { get; set; }
    private int LoopDepth { get; set; }
    public List<SxStatement> Statements { get; set; }
    public List<SxTokenTypes> AccessModifiersAndIdentifier = new List<SxTokenTypes>() {SxTokenTypes.KeywordStatic, SxTokenTypes.Identifier};
    
    public SxParser(List<SxToken> tokens)
    {
        Tokens = tokens;
    }

    public List<SxStatement> Parse()
    {
        Statements = new List<SxStatement>();
        while (!IsAtEnd())
        {
            Statements.Add(Declaration());
        }

        return Statements;
    }
    
    /*
        parse          → declaration * EOF ;
        declaration    → varDeclr
                         | funDeclr
                         | classDeclr
                         | statement ;
        funDeclr       → (modifier)?* ("fn" | "func" | "function") function ;
        classDeclr     → "class" IDENTIFIER (":" IDENTIFIER)? "{"? (memberFunction | memberDeclr)* "}" ;
        modifier       → "async" ;
        memberFunction → "static"? function ;
        function       → IDENTIFIER "(" parameters? ")" block ;
        memberDeclr    → IDENTIFIER ("=" expression)? ";"? ;   
        parameters     → IDENTIFIER ( "," IDENTIFIER )* ;                 
        varDeclr       → "var"? declarator ("=" expression)? ";"? ;                   
        statement      → exprStmt
                         | ifStmt
                         | whileStmt
                         | forStmt
                         | gotoStmt
                         | labeledStmt
                         | printStmt
                         | breakStmt
                         | continueStmt 
                         | returnStmt
                         | block ;
        returnStmt     → ("return" | "ret") expression? ";"? ;                
        breakStmt      → "break" ";"? ;
        continueStmt   → "continue" ";"? ;
        gotoStmt       → "goto" IDENTIFIER ";"? ;   
        labeledStmt    → IDENTIFIER ":" statement                   
        continueStmt   → "continue" ";"? ;            
        forStmt        → "for" "("? (varDeclr | exprStmt | ";"?) expression? ";"? expression? ";"? ")"? statement ;          
        whileStmt      → "while" "("? expression ")"? statement ;            
        ifStmt         → "if" "("? expression ")"? statement ( "else" statement )? ;                 
        block          → "{" declaration * "}" ;        
        printStmt      → "print" expression ";"? ;
        exprStmt       → expression ";"? ;     
        expression     → assignment ";" ;
        assignment     → ( call "." )? (postfix)? IDENTIFIER ("=" | "+=" | "-=" | "*=" | "/=" | "%=" | "^=") assignment
                         | logicOr 
                         | logicOr "?" ternary ;                 
        logicOr        → logicAnd ( "or" logicAnd )* ;
        logicAnd       → equality ( "and" equality )* ;                    
        ternary        → expression ":" expression ;
        equality       → comparison ( ( "!=" | "==" ) comparison )* ;
        comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
        term           → factor ( ( "-" | "+" ) factor )* ;
        factor         → unary ( ( "%" | "/" | "*" | "^" ) unary )* ;
        unary          → ( "!" | "-" | "+" ) unary
                        | postfix ;
        postfix        → call
                        | primary ("++" | "--")
                        | "[" expression? "]" postfix
                        | primary ;
        call           → "await"? primary ( "(" arguments? ")" | "." declarator )* ;                
        arguments      → (expression ":")? expression ("," (expression ":")? expression ("=" expression)? )* ;
        declarator     → IDENTIFIER ("[" expression? "]")?*               
        primary        → NUMBER 
                        | STRING | IDENTIFIER | "true" | "false" | "nil" | "this"
                        | "(" expression ")" | ( (modifier)?* ("fn" | "func" | "function")  "(" parameters? ")" block )
                        | "[" expression? ("," expression)?* "]"
                        | "base" "." IDENTIFIER ;
     */

    SxStatement Declaration()
    {
        if (Match(SxTokenTypes.KeywordVar))
        {
            return VarDeclr();
        }

        if (Match(SxTokenTypes.KeywordClass))
        {
            return ClassDeclr();
        }

        if (Check(SxTokenTypes.KeywordFunction) && CheckNth(1, SxTokenTypes.Identifier))
        {
            return FunDeclr("function");
        }

        return Statement();
    }

    SxArrayExpression ArrayDeclarator(SxArrayExpression? parent = null)
    {
        SxArrayExpression arrayAccess = parent ?? new SxArrayExpression(new List<SxExpression>(), null, null);
        if (Match(SxTokenTypes.LeftBracket))
        {
            if (Match(SxTokenTypes.RightBracket))
            {
                
            }
            else
            {
                SxExpression expr = Expression();
                arrayAccess.ArrayExpr?.Add(expr);
                Consume(SxTokenTypes.RightBracket, "Očekávána ] na konci přístupu k poli");
            }

            if (Check(SxTokenTypes.LeftBracket))
            {
                return ArrayDeclarator(arrayAccess);
            }
        }

        return arrayAccess;
    }

    SxArrayExpression DeclaratorArrayPart()
    {
        SxArrayExpression? arrayExpression = null;
        if (Check(SxTokenTypes.LeftBracket))
        {
            arrayExpression = ArrayDeclarator();
        }

        return arrayExpression;
    }
    
    SxExpression Declarator(SxToken? name)
    {
        if (name == null)
        {
            name = Consume(SxTokenTypes.Identifier, "Očekáván název proměnné");
        }
        
        SxArrayExpression? arrayExpression = DeclaratorArrayPart();
        return new SxVarExpression(name, arrayExpression, null);
    }

    SxClassStatement ClassDeclr()
    {
        SxToken name = Consume(SxTokenTypes.Identifier, "Očekáván název třídy");

        SxVarExpression superclass = null;
        if (Match(SxTokenTypes.Colon))
        {
            Consume(SxTokenTypes.Colon, "Očekávána : pro zdědění třídy");
            superclass = new SxVarExpression(Consume(SxTokenTypes.Identifier, "Očekáván název třídy při dědění za :"), null, null);
        }
        
        if (Match(SxTokenTypes.LeftBrace))
        {
            Consume(SxTokenTypes.LeftBrace, "Očekávána { za názvem v deklaraci třídy");
        }

        List<SxFunctionStatement> methods = new List<SxFunctionStatement>();
        List<SxFunctionStatement> classMethods = new List<SxFunctionStatement>();
        List<SxVarStatement> fields = new List<SxVarStatement>();
        
        while (!Check(SxTokenTypes.RightBrace) && !IsAtEnd())
        {
            SxTokenTypes? next = NextTokenNotAccessModifierOrIdentifier(1);
            if (next == SxTokenTypes.LeftParen)
            {
                SxFunctionStatement fn = MemberFunction("metoda");
                if (fn.IsStatic)
                {
                    classMethods.Add(fn);
                }
                else
                {
                    methods.Add(fn);   
                }
            }
            else if (next == SxTokenTypes.Equal)
            {
                fields.Add(MemberDeclr());
            }
        }

        Consume(SxTokenTypes.RightBrace, "Očekávána } na konci deklarace třídy");
        return new SxClassStatement(name, methods, fields, classMethods, superclass);
    }

    SxTokenTypes? NextTokenNotAccessModifierOrIdentifier(int i = 1)
    {
        while (!IsAtEnd())
        {
            if (!CheckNth(i, AccessModifiersAndIdentifier))
            {
                break;
            }

            i++;
        }

        return PeekNth(i)?.Type ?? SxTokenTypes.Eof;
    }

    SxFunctionStatement MemberFunction(string kind)
    {
        bool isStatic = Match(SxTokenTypes.KeywordStatic);
        return FunDeclr(kind, false, false, isStatic);
    }
    
    SxFunctionStatement FunDeclr(string kind, bool includeFnKeyword = true, bool includeRightBrace = false, bool isStatic = false)
    {
        if (includeFnKeyword)
        {
            Match(SxTokenTypes.KeywordFunction);   
        }

        if (Match(SxTokenTypes.KeywordAsync))
        {
            // async
        }

        SxToken name = Consume(SxTokenTypes.Identifier, $"Očekáván název {kind}");
        SxFunctionExpression expr = FunctionBody("function", includeRightBrace);

        return new SxFunctionStatement(name, expr, isStatic);
    }

    SxFunctionExpression FunctionBody(string kind, bool includeRightBrace = false)
    {
        Consume(SxTokenTypes.LeftParen, "Očekávána ( za deklarací signatury funkce");
        List<SxArgumentDeclrExpression> pars = new List<SxArgumentDeclrExpression>();

        if (!Check(SxTokenTypes.RightParen))
        {
            do
            {
                SxExpression argumentDefaultValue = null!;
                SxToken par = Consume(SxTokenTypes.Identifier, "Očekáván název parametru");
                bool hasDefaultValue = false;
                
                if (Match(SxTokenTypes.Equal))
                {
                    argumentDefaultValue = Expression();
                    hasDefaultValue = true;
                }
                
                pars.Add(new SxArgumentDeclrExpression(par, argumentDefaultValue, hasDefaultValue));
            } while (Match(SxTokenTypes.Comma));
        }

        Consume(SxTokenTypes.RightParen, "Očekávána ) na konci deklarace signatury funkce");
        Consume(SxTokenTypes.LeftBrace, "Očekávána { na začátku deklarace obsahu funkce");
        List<SxStatement> body = Block();
        return new SxFunctionExpression(pars, body);
    }

    SxVarStatement MemberDeclr()
    {
        SxToken identifier = Consume(SxTokenTypes.Identifier, "Očekáván název proměnné");
        SxExpression initialVal = null;
        
        if (Match(SxTokenTypes.Equal))
        {
            initialVal = Expression();
        }

        if (Check(SxTokenTypes.Semicolon))
        {
            Consume(SxTokenTypes.Semicolon, "Očekáván ;");
        }

        return new SxVarStatement(initialVal, identifier);
    }
    
    SxVarStatement VarDeclr()
    {
        SxToken identifier = Consume(SxTokenTypes.Identifier, "Očekáván název proměnné");
        SxExpression initialVal = null;
        
        if (Match(SxTokenTypes.Equal))
        {
            initialVal = Expression();
        }

        if (Check(SxTokenTypes.Semicolon))
        {
            Consume(SxTokenTypes.Semicolon, "Očekáván ;");
        }

        return new SxVarStatement(initialVal, identifier);
    }

    SxStatement Statement(bool generateScope = true)
    {
        if (Match(SxTokenTypes.KeywordPrint))
        {
            return PrintStmt();
        }

        if (Match(SxTokenTypes.LeftBrace))
        {
            return new SxBlockStatement(Block(), generateScope);
        }

        if (Match(SxTokenTypes.KeywordIf))
        {
            return IfStmt();
        }

        if (Match(SxTokenTypes.KeywordWhile))
        {
            return WhileStmt();
        }

        if (Match(SxTokenTypes.KeywordFor))
        {
            return ForStmt();
        }

        if (Match(SxTokenTypes.KeywordBreak))
        {
            return BreakStmt();
        }
        
        if (Match(SxTokenTypes.KeywordContinue))
        {
            return ContinueStmt();
        }

        if (Check(SxTokenTypes.Identifier) && CheckNth(1, SxTokenTypes.Colon))
        {
            return LabeledStmt();
        }
        
        if (Match(SxTokenTypes.KeywordGoto))
        {
            return GotoStmt();
        }

        if (Match(SxTokenTypes.KeywordReturn))
        {
            return ReturnStmt();
        }

        return ExprStmt();
    }

    SxStatement ReturnStmt()
    {
        SxToken previous = Previous();
        SxExpression val = null!;

        if (!Check(SxTokenTypes.Semicolon) && !Check(SxTokenTypes.RightBrace))
        {
            val = Expression();
        }

        if (Match(SxTokenTypes.Semicolon))
        {
            // ;
        }

        return new SxReturnStatement(previous, val);
    }

    SxStatement GotoStmt()
    {
        SxToken identifier = Consume(SxTokenTypes.Identifier, "Očekáván název návěští");
        if (Check(SxTokenTypes.Semicolon))
        {
            Consume(SxTokenTypes.Semicolon, "Očekáván ;");
        }

        return new SxGotoStatement(identifier);
    }

    SxStatement LabeledStmt()
    {
        SxToken identifier = null;
        
        if (Match(SxTokenTypes.Identifier))
        {
            identifier = Previous();
        }

        Consume(SxTokenTypes.Colon, "Očekávána :");

        SxStatement statement = Statement();
        return new SxLabelStatement(identifier, statement);
    }

    SxStatement BreakStmt()
    {
        if (Match(SxTokenTypes.Semicolon))
        {
            // ;
        }

        if (LoopDepth == 0)
        {
            // [todo] chyba, musí být uvnitř cyklu pro použití break
        }

        return new SxBreakStatement();
    }
    
    SxStatement ContinueStmt()
    {
        if (Match(SxTokenTypes.Semicolon))
        {
            // ;
        }

        if (LoopDepth == 0)
        {
            // [todo] chyba, musí být uvnitř cyklu pro použití break
        }

        return new SxContinueStatement();
    }

    // forStmt → "for" "("? (varDeclr | exprStmt | ";"?) expression? ";"? expression? ")"? statement ;     
    // dekonstrukce na:
    // {
    //  var i = initializer
    //  while (i operator condition | true) 
    //  {
    //   statement
    //   increment
    //  }
    // }
    SxStatement ForStmt()
    {
        if (Match(SxTokenTypes.LeftParen))
        {
            // (    
        }

        SxStatement initializer;
        if (Match(SxTokenTypes.Semicolon))
        {
            initializer = null;
        }
        else if (Match(SxTokenTypes.KeywordVar))
        {
            initializer = VarDeclr();
        }
        else
        {
            initializer = ExprStmt();
        }

        SxExpression condition;
        if (Check(SxTokenTypes.Semicolon))
        {
            condition = null;
        }
        else
        {
            condition = Expression();
        }

        if (Match(SxTokenTypes.Semicolon))
        {
            // ;
        }

        SxExpression increment;
        if (Check(SxTokenTypes.RightParen))
        {
            increment = null;
        }
        else
        {
            increment = Expression();
        }

        if (Match(SxTokenTypes.RightParen))
        {
            // )    
        }

        // [todo] loop depth inc
        SxStatement statement = Statement(false);
        return new SxForStatement(initializer, condition, increment, statement);
    }

    SxStatement WhileStmt()
    {
        try
        {
            if (Match(SxTokenTypes.LeftParen))
            {
                // (    
            }

            SxExpression expr = Expression();

            if (Match(SxTokenTypes.RightParen))
            {
                // )
            }

            LoopDepth++;
            SxStatement statement = Statement();
            return new SxWhileStatement(expr, statement);
        }
        catch (SxBreakException e)
        {
            return null!;
        }
        finally
        {
            LoopDepth--;
        }
    }
    
    SxStatement IfStmt()
    {
        if (Match(SxTokenTypes.LeftParen))
        {
            // (    
        }

        SxExpression expr = Expression();

        if (Match(SxTokenTypes.RightParen))
        {
            // )
        }

        SxStatement thenBranch = Statement();
        SxStatement elseBranch = null!;

        if (Match(SxTokenTypes.KeywordElse))
        {
            elseBranch = Statement();
        }

        return new SxIfStatement(expr, thenBranch, elseBranch);
    }

    List<SxStatement> Block()
    {
        List<SxStatement> statements = new List<SxStatement>();

        while (!Check(SxTokenTypes.RightBrace) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(SxTokenTypes.RightBrace, "Očekávána }");
        return statements;
    }

    SxStatement PrintStmt()
    {
        SxExpression expr = Expression();

        if (Check(SxTokenTypes.Semicolon))
        {
            Consume(SxTokenTypes.Semicolon, "Očekáván ;");   
        }

        return new SxPrintStatement(expr);
    }

    SxStatement ExprStmt()
    {
        SxExpression expr = Expression();
        if (Check(SxTokenTypes.Semicolon))
        {
            Consume(SxTokenTypes.Semicolon, "Očekáván ;");   
        }

        return new SxExpressionStatement(expr);
    }

    // expression → ternary ;
    SxExpression Expression()
    {
        return Assignment();
    }
    
    SxExpression Assignment()
    {
        SxExpression expr = LogicalOr();
        
        if (Match(SxTokenTypes.Equal) || Match(SxTokenTypes.PlusEqual) || Match(SxTokenTypes.MinusEqual) || Match(SxTokenTypes.StarEqual) || Match(SxTokenTypes.SlashEqual) || Match(SxTokenTypes.PercentEqual) || Match(SxTokenTypes.CaretEqual))
        {
            SxToken op = Previous();
            SxExpression value = Assignment();

            if (expr is SxVarExpression varExpr)
            {
                SxToken name = varExpr.Name;
                return new SxAssignExpression(name, value, op, null, varExpr.ArrayExpr);
            }
            
            if (expr is SxGetExpression getExpr)
            {
                return new SxSetExpression(getExpr.Name, getExpr.Object, value, op, null, getExpr.ArrayExpr);
            }
            
            // [todo] error, neplatný cíl pro přiřazení
        }

        if (Match(SxTokenTypes.Question))
        {
            return Ternary(expr);
        }

        return expr;
    }

    SxExpression LogicalOr()
    {
        SxExpression expr = LogicalAnd();
        
        while (Match(SxTokenTypes.KeywordOr))
        {
            SxToken op = Previous();
            SxExpression right = LogicalAnd();
            expr = new SxLogicalExpression(expr, op, right);
        }

        return expr;
    }

    SxExpression LogicalAnd()
    {
        SxExpression expr = Equality();
        while (Match(SxTokenTypes.KeywordAnd))
        {
            SxToken op = Previous();
            SxExpression right = Equality();
            return new SxLogicalExpression(expr, op, right);
        }

        return expr;
    }

    // ternary → equality ? expression : expression
    // | equality
    SxExpression Ternary(SxExpression expr)
    {
        SxExpression caseTrue = Expression();
        Consume(SxTokenTypes.Colon, "V ternárním operátoru chybí :");
        SxExpression caseFalse = Expression();
        return new SxTernaryExpression(expr, caseTrue, caseFalse);
    }

    // equality → comparison ( ( "!=" | "==" ) comparison )* ;
    SxExpression Equality()
    {
        SxExpression expr = Comparison();
        while (Match(SxTokenTypes.ExclamationEqual, SxTokenTypes.EqualEqual))
        {
            SxToken op = Previous();
            SxExpression right = Comparison();
            expr = new SxBinaryExpression(expr, op, right);
        }

        return expr;
    }

    // comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
    SxExpression Comparison()
    {
        SxExpression expr = Term();
        while (Match(SxTokenTypes.Greater, SxTokenTypes.GreaterEqual, SxTokenTypes.Less, SxTokenTypes.LessEqual))
        {
            SxToken op = Previous();
            SxExpression right = Term();
            expr = new SxBinaryExpression(expr, op, right);
        }

        return expr;
    }
    
    // term → factor ( ( "-" | "+" ) factor )* ;
    SxExpression Term()
    {
        SxExpression expr = Factor();
        while (Match(SxTokenTypes.Minus, SxTokenTypes.Plus))
        {
            SxToken op = Previous();
            SxExpression right = Factor();
            expr = new SxBinaryExpression(expr, op, right);
        }

        return expr;
    }

    // factor → unary ( ( "%" | "/" | "*" ) unary )* ;
    SxExpression Factor()
    {
        SxExpression expr = Unary();
        while (Match(SxTokenTypes.Slash, SxTokenTypes.Star, SxTokenTypes.Percent, SxTokenTypes.Caret))
        {
            SxToken op = Previous();
            SxExpression right = Unary();
            expr = new SxBinaryExpression(expr, op, right);
        }

        return expr;
    }
    
    // unary → ( "!" | "-" | "+" ) unary
    // | primary ;
    SxExpression Unary()
    {
        if (Match(SxTokenTypes.Exclamation, SxTokenTypes.Minus, SxTokenTypes.Plus))
        {
            SxToken op = Previous();
            SxExpression right = Unary();
            return new SxUnaryExpression(op, right);
        }
        
        return Postfix();
    }

    SxExpression Postfix(SxExpression? parent = null)
    {
        SxExpression expr = parent ?? Call();
        
        if (Match(SxTokenTypes.LeftBracket))
        {
            SxToken op = Previous();
            SxPostfixExpression finalExpr;

            if (Match(SxTokenTypes.RightBracket))
            {
                finalExpr = new SxPostfixExpression(op, expr, null);
            }
            else
            {
                SxExpression arrayExpr = Expression();
                Consume(SxTokenTypes.RightBracket, "Očekáván ] na konci přístupu k poli");
                finalExpr = new SxPostfixExpression(op, expr, arrayExpr);
            }

            if (Check(SxTokenTypes.LeftBracket))
            {
                return Postfix(finalExpr);
            }

            return finalExpr;
        }

        if (Match(SxTokenTypes.PlusPlus, SxTokenTypes.MinusMinus))
        {
            SxToken op = Previous();
            return new SxPostfixExpression(op, expr, null);
        }

        return expr;
    }

    SxExpression Call()
    {
        bool awaitCall = Match(SxTokenTypes.KeywordAwait);
        SxExpression expr = Primary();

        while (true)
        {
            if (Match(SxTokenTypes.LeftParen))
            {
                expr = Arguments(expr);
            }
            else if (Match(SxTokenTypes.Dot))
            {
                SxToken name = Consume(SxTokenTypes.Identifier, "Očekáván název člena za .");
                SxArrayExpression arrayExpression = null;
                
                if (Check(SxTokenTypes.LeftBracket))
                {
                    arrayExpression = DeclaratorArrayPart();
                }
                
                expr = new SxGetExpression(name, expr, arrayExpression);
            }
            else
            {
                break;
            }
        }

        if (expr is SxExpression.ISxAwaitableExpression awaitable)
        {
            awaitable.Await = awaitCall;
        }
        
        return expr;
    }

    SxExpression Arguments(SxExpression callee)
    {
        List<SxCallArgument> arguments = new List<SxCallArgument>();
        if (!Check(SxTokenTypes.RightParen))
        {
            do
            {
                SxExpression expression = Expression();

                if (Match(SxTokenTypes.Colon))
                {
                    SxExpression val = Expression();
                    arguments.Add(new SxCallArgument(val, expression));
                }
                else
                {
                    arguments.Add(new SxCallArgument(expression, null!));
                }
                
            } while (Match(SxTokenTypes.Comma));
        }

        SxToken paren = Consume(SxTokenTypes.RightParen, "Očekávána )");
        return new SxCallExpression(callee, paren, arguments);
    }
    
    // primary → NUMBER | STRING | IDENTIFIER | "true" | "false" | "nil" 
    // | "(" expression ")" ;
    SxExpression Primary()
    {
        if (Match(SxTokenTypes.Number, SxTokenTypes.String))
        {
            object val = Previous().Literal;
            return new SxLiteralExpression(val);
        }

        if (Match(SxTokenTypes.Identifier))
        {
            return Declarator(Previous()); // new SxVarExpression(Previous());
        }

        if (Match(SxTokenTypes.True))
        {
            return new SxLiteralExpression(true);
        }

        if (Match(SxTokenTypes.False))
        {
            return new SxLiteralExpression(false);
        }

        if (Match(SxTokenTypes.Nill))
        {
            return new SxLiteralExpression(null);
        }

        if (Match(SxTokenTypes.KeywordThis))
        {
            return new SxThisExpression(Previous());
        }

        if (Match(SxTokenTypes.LeftParen))
        {
            SxExpression expr = Expression();
            Consume(SxTokenTypes.RightParen, "Očekávána ) k ukončení páru kulatých závorek");
            return new SxGroupingExpression(expr);
        }

        if (Match(SxTokenTypes.KeywordFunction))
        {
            return FunctionBody("function");
        }

        if (Match(SxTokenTypes.KeywordBase))
        {
            SxToken keyword = Previous();
            Consume(SxTokenTypes.Dot, "Očekávána . za base");
            SxToken method = Consume(SxTokenTypes.Identifier, "Očekáván název metody nadřazené třídy");

            return new SxSuperExpression(keyword, method);
        }

        if (Match(SxTokenTypes.LeftBracket))
        {
            if (Match(SxTokenTypes.RightBracket))
            {
                return new SxArrayExpression(null, null, null);
            }

            List<SxExpression> values = new List<SxExpression>();
            SxExpression expr = Expression();
            values.Add(expr);

            while (Match(SxTokenTypes.Comma))
            {
                values.Add(Expression());
            }
            
            Consume(SxTokenTypes.RightBracket, "Očekávána ] na konci pole");
            return new SxArrayExpression(values, null, null);
        }

        return null;
    }

    SxToken Consume(SxTokenTypes type, string errMsg)
    {
        if (Check(type))
        {
            return Step();
        }

        return null;
    }

    bool Match(params SxTokenTypes[] types)
    {
        foreach (SxTokenTypes type in types)
        {
            if (Check(type))
            {
                Step();
                return true;
            }
        }

        return false;
    }

    bool Check(SxTokenTypes type)
    {
        if (IsAtEnd())
        {
            return false;
        }

        return Peek().Type == type;
    }
    
    bool CheckNth(int n, SxTokenTypes type)
    {
        if (IsAtEnd())
        {
            return false;
        }
        
        return PeekNth(n)?.Type == type;
    }
    
    bool CheckNth(int n, List<SxTokenTypes> types)
    {
        if (IsAtEnd())
        {
            return false;
        }

        return types.Contains(PeekNth(n)?.Type ?? SxTokenTypes.Eof);
    }

    SxToken Step()
    {
        if (!IsAtEnd())
        {
            Current++;
        }

        return Previous();
    }

    bool IsAtEnd()
    {
        return Peek().Type == SxTokenTypes.Eof;
    }

    SxToken Peek()
    {
        return Tokens[Current];
    }
    
    SxToken? PeekNth(int n = 1)
    {
        if (Tokens.Count >= Current + n)
        {
            return Tokens[Current + n];
        }

        return null;
    }

    SxToken Previous()
    {
        return Tokens[Current - 1];
    }
}