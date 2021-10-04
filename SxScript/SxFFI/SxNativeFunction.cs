using System.Reflection;

namespace SxScript.SxFFI;

public abstract class SxNativeAsyncBase : SxExpression.ISxAsyncCallable
{
    public abstract Task<object?> CallAsync(SxInterpreter interpreter, List<object?> arguments);
    public bool Await { get; set; }
}

public abstract class SxNativeBase : SxExpression.ISxCallable
{
    public abstract object? Call(SxInterpreter interpreter, List<object?> arguments);
}

public class SxNativeFunction<T> : SxNativeBase
{
    public Func<T> Method { get; set; }
    public SxNativeFunction(Func<T> method) { Method = method; }

    public override object Call(SxInterpreter interpreter, List<object?> arguments)
    {
        object obj = Method.Invoke()!;
        return obj;
    }
}

public class SxNativeAsyncFunction<T> : SxNativeAsyncBase
{
    public Func<Task<T>> Method { get; set; }
    public SxNativeAsyncFunction(Func<Task<T>> method) { Method = method; }

    public override async Task<object?> CallAsync(SxInterpreter interpreter, List<object?> arguments)
    {
        object? obj = await Method.Invoke();
        return obj!;
    }
}