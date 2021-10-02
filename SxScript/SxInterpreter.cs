namespace SxScript;

public class SxInterpreter : SxExpression.ISxExpressionVisitor<object>
{
    public object? Visit(SxBinaryExpression expr)
    {
        object right = Evaluate(expr.Right);
        object left = Evaluate(expr.Left);

        if ((right is string rightStr || left is string leftStr) && expr.Operator.Type == SxTokenTypes.Plus)
        {
            return left.ToString() + right.ToString();
        }

        object? val = PerformArithmeticOperation(left, right, expr.Operator.Type);
        return val;
    }

    public object Visit(SxUnaryExpression expr)
    {
        object right = Evaluate(expr.Expr);
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

    public object? PerformArithmeticOperation(object left, object right, SxTokenTypes op)
    {
        return op switch
        {
            SxTokenTypes.Plus => (object) ((dynamic) left + (dynamic) right),
            SxTokenTypes.Minus => (object) ((dynamic) left - (dynamic) right),
            SxTokenTypes.Star => (object) ((dynamic) left * (dynamic) right),
            SxTokenTypes.Slash => (object) ((dynamic) left / (dynamic) right),
            SxTokenTypes.Greater => (object) ((dynamic) left > (dynamic) right),
            SxTokenTypes.GreaterEqual => (object) ((dynamic) left >= (dynamic) right),
            SxTokenTypes.Less => (object) ((dynamic) left < (dynamic) right),
            SxTokenTypes.LessEqual => (object) ((dynamic) left <= (dynamic) right),
            SxTokenTypes.EqualEqual => ObjectIsEqual(left, right),
            SxTokenTypes.ExclamationEqual => !ObjectIsEqual(left, right),
            _ => null
        };
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

        return true;
    }

    public object Visit(SxLiteralExpression expr)
    {
        return expr.Value;
    }

    public object Visit(SxGroupingExpression expr)
    {
        return Evaluate(expr.Expr);
    }

    // expr ? caseTrue : caseFalse
    public object Visit(SxTernaryExpression expr)
    {
        object iff = Evaluate(expr.Expr);
        if (iff is bool bl)
        {
            return Evaluate(bl ? expr.CaseTrue : expr.CaseFalse);
        }

        return Evaluate(expr.CaseTrue);
    }

    public object Evaluate(SxExpression expression)
    {
        return expression.Accept(this);
    }
}