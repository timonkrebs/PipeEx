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

    for i in $(seq 1 $item); do
        ty="${ty}, TSource$((i + 1))"
        tv="${tv}, source.Item$((i + 1))"
    done

cat << EOF >> $fileName

    public static async StructuredTask<TResult> I<$ty, TResult>(this ($ty) source, Func<$ty, Task<TResult>> func)
    {
        return await func($tv);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this ($ty) source, Func<$ty, StructuredTask<TResult>> func)
    {
        return await func($tv);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this Task<($ty)> s, Func<$ty, TResult> func)
    {
        var source = await s;
        return func($tv);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this StructuredTask<($ty)> s, Func<$ty, TResult> func)
    {
        var source = await s;
        return func($tv);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this Task<($ty)> s, Func<$ty, Task<TResult>> func)
    {
        var source = await s;
        return await func($tv);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this StructuredTask<($ty)> s, Func<$ty, Task<TResult>> func)
    {
        var source = await s;
        return await func($tv);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this Task<($ty)> s, Func<$ty, StructuredTask<TResult>> func)
    {
        var source = await s;
        return await func($tv);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this StructuredTask<($ty)> s, Func<$ty, StructuredTask<TResult>> func)
    {
        var source = await s;
        return await func($tv);
    }
EOF

done
cat << EOF >> $fileName
}
EOF
echo "Successfully generated $fileName"