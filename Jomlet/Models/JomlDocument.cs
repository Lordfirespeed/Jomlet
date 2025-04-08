using System.Text;

namespace Tomlet.Models;

public class JomlDocument : JomlTable
{
    // ReSharper disable once UnusedMember.Global
    public static JomlDocument CreateEmpty() => new();

    public string? TrailingComment { get; set; }

    internal JomlDocument()
    {
        //Non-public ctor.
    }

    internal JomlDocument(JomlTable from)
    {
        foreach (var key in from.Keys)
        {
            PutValue(key, from.GetValue(key));
        }
    }

    private string SerializeDocument()
    {
        var sb = new StringBuilder();
        sb.Append(SerializeNonInlineTable(null, false));

        if (TrailingComment != null)
        {
            var comment = new JomlCommentData {PrecedingComment = TrailingComment};
            sb.Append('\n');
            sb.Append(comment.FormatPrecedingComment());
        }

        return sb.ToString();
    }

    public override string SerializedValue => SerializeDocument();

    public override string StringValue => $"Toml root document ({Entries.Count} entries)";
}