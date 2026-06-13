using System.Text;

namespace PipeEx.SourceGenerators;

/// <summary>
/// Helpers that build the comma separated type-parameter and tuple-member lists for a
/// given tuple arity. Mirrors the <c>ty</c> / <c>tv</c> variables of the original
/// <c>TupleDestructuring.sh</c> scripts.
/// </summary>
internal static class TupleArity
{
    /// <summary>The smallest tuple arity that is generated (a two element tuple).</summary>
    public const int Min = 2;

    /// <summary>The largest tuple arity that is generated (a thirteen element tuple).</summary>
    public const int Max = 13;

    /// <summary>Builds <c>"TSource1, TSource2, ..., TSourceN"</c> for the given <paramref name="arity"/>.</summary>
    public static string TypeList(int arity) => Join(arity, "TSource");

    /// <summary>Builds <c>"source.Item1, source.Item2, ..., source.ItemN"</c> for the given <paramref name="arity"/>.</summary>
    public static string ValueList(int arity) => Join(arity, "source.Item");

    private static string Join(int arity, string prefix)
    {
        var sb = new StringBuilder();
        for (var i = 1; i <= arity; i++)
        {
            if (i > 1)
            {
                sb.Append(", ");
            }

            sb.Append(prefix).Append(i);
        }

        return sb.ToString();
    }
}
