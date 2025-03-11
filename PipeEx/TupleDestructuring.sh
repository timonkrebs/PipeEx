#!/bin/bash
fileName="PipeEx/TupleDestructuring.g.cs"
echo "Started generating $fileName"
cat << EOF > $fileName
namespace PipeEx;

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

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<$ty, TResult>(this ($ty) source, Func<$ty, TResult> transform)
    {
        return transform($tv);
    }
EOF

done
cat << EOF >> $fileName
}
EOF
echo "Successfully generated $fileName"