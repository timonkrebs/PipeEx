namespace PipeEx;

public static class Core
{
    public static TResult I<TSource, TResult>(this TSource source, Func<TSource, TResult> func)
    {
        return func(source);
    }
}
