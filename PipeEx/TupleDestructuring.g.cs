namespace PipeEx;

public static class TupleDestructuring
{
    public static TResult I<TSource1, TSource2, TResult>(this (TSource1, TSource2) source, Func<TSource1, TSource2, TResult> func)
    {
        return func(source.Item1, source.Item2);
    }
    public static TResult I<TSource1, TSource2, TSource3, TResult>(this (TSource1, TSource2, TSource3) source, Func<TSource1, TSource2, TSource3, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TResult>(this (TSource1, TSource2, TSource3, TSource4) source, Func<TSource1, TSource2, TSource3, TSource4, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
    }
}
