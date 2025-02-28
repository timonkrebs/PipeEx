#!/bin/bash
fileName="TupleDestructuring.g.cs"
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

    echo "Current item: $item, Types: $ty"

cat << EOF >> $fileName
    public static TResult I<$ty, TResult>(this ($ty) source, Func<$ty, TResult> func)
    {
        return func($tv);
    }
EOF

done
cat << EOF >> $fileName
}
EOF