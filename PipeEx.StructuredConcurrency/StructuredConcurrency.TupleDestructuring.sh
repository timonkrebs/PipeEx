#!/bin/bash
fileName="PipeEx.StructuredConcurrency/StructuredConcurrency.TupleDestructuring.g.cs"
echo "Started generating $fileName"
cat << EOF > $fileName
namespace PipeEx.StructuredConcurrency;

public static class TupleDestructuring
{
EOF
for item in $(seq 1 12); do
    ty="TSource1"
    tv="source.Item1"
    targ="t.Item1"

    for i in $(seq 1 $item); do
        ty="${ty}, TSource$((i + 1))"
        tv="${tv}, source.Item$((i + 1))"
        targ="${targ}, t.Item$((i + 1))"
    done

cat << EOF >> $fileName

    public static async StructuredTask<TResult> I<$ty, TResult>(this ($ty) source, Func<$ty, Task<TResult>> func)
    {
        return await func($tv).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<$ty, TResult>(this ($ty) source, Func<$ty, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func($tv);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this Task<($ty)> s, Func<$ty, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func($tv);
    }

    public static StructuredTask<TResult> I<$ty, TResult>(this StructuredTask<($ty)> s, Func<$ty, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func($tv);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this Task<($ty)> s, Func<$ty, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func($tv).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<$ty, TResult>(this StructuredTask<($ty)> s, Func<$ty, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func($tv).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<$ty, TResult>(this Task<($ty)> s, Func<$ty, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func($targ), new CancellationTokenSource());

    public static StructuredTask<TResult> I<$ty, TResult>(this StructuredTask<($ty)> s, Func<$ty, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func($targ), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));
EOF

done
cat << EOF >> $fileName
}
EOF
echo "Successfully generated $fileName"