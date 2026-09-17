using System.Text;

namespace Content.Client._Starfall.UserInterface.Controls;

/// <summary>
/// Wraps a string of text into multiple lines, preserving markup tags and escape sequences, and ensuring that no line exceeds a given character limit.
/// Used for _UM's runechat.
/// </summary>
/// <remarks>
/// The original implementation of word wrapping in _UM counted invisible tag syntax towards the character limit
/// and can end up being split right in the middle of a tag (turning "[/color]" into "[/color-")
/// producing corrupt markup that crashes the parser. It also had a bug where it would break a line
/// in the middle of a word if the word was followed by punctuation with no space, such as highlight matching a word
/// directly followed by punctuation with no space ("Captain's").
/// </remarks>
public sealed class WordWrapHelper
{
    /// <summary>
    /// A single indivisible unit of the input text: either one visible character (which may be a
    /// 2-character escape sequence like "\[") or one whole markup tag ("[color=red]" or "[/color]").
    /// </summary>
    private readonly struct Atom
    {
        public readonly string Raw;
        public readonly int VisibleLength;
        public readonly TagKind Kind;

        public Atom(string raw, int visibleLength, TagKind kind)
        {
            Raw = raw;
            VisibleLength = visibleLength;
            Kind = kind;
        }

        public bool IsSpace => Kind == TagKind.None && Raw == " ";
    }

    private enum TagKind
    {
        None,
        Open,
        Close,
        SelfClosing,
    }

    public static IEnumerable<string> WordWrap(string text, int charLimit)
    {
        var atoms = Tokenize(text);
        var lines = new List<string>();

        var currentLine = new List<Atom>();
        var openTags = new List<string>();
        var currentVisible = 0;

        var i = 0;
        while (i < atoms.Count)
        {
            var tokenEnd = FindTokenEnd(atoms, i, out var wordEnd);
            var tokenVisible = SumVisible(atoms, i, tokenEnd);

            // Does the whole word + any trailing spaces fit on the current line?
            if (currentVisible + tokenVisible <= charLimit)
            {
                for (var j = i; j < tokenEnd; j++)
                    Append(currentLine, openTags, ref currentVisible, atoms[j]);
                i = tokenEnd;
                continue;
            }

            if (currentLine.Count > 0)
            {
                FlushLine(lines, currentLine, openTags);
                currentVisible = 0;
                Reopen(currentLine, openTags);
            }

            var wordVisible = SumVisible(atoms, i, wordEnd);

            if (wordVisible <= charLimit)
            {
                for (var j = i; j < tokenEnd; j++)
                    Append(currentLine, openTags, ref currentVisible, atoms[j]);
                i = tokenEnd;
                continue;
            }

            // The word itself (ignoring markup) is too long to fit on an empty line, split it,
            // don't break inside a tag or escape sequence.
            var available = Math.Max(charLimit - 1, 1);
            var j2 = i;
            while (j2 < wordEnd)
            {
                var atom = atoms[j2];

                if (currentVisible > 0 && currentVisible + atom.VisibleLength > available)
                {
                    FlushLine(lines, currentLine, openTags, hyphen: true);
                    currentVisible = 0;
                    Reopen(currentLine, openTags);
                }

                Append(currentLine, openTags, ref currentVisible, atom);
                j2++;
            }

            // Trailing spaces that belong to this
            for (; j2 < tokenEnd; j2++)
                Append(currentLine, openTags, ref currentVisible, atoms[j2]);

            i = tokenEnd;
        }

        if (currentLine.Count > 0)
            FlushLine(lines, currentLine, openTags);

        return lines;
    }

    private static void Append(List<Atom> line, List<string> openTags, ref int visible, Atom atom)
    {
        line.Add(atom);
        visible += atom.VisibleLength;

        if (atom.Kind == TagKind.Open)
            openTags.Add(atom.Raw);
        else if (atom.Kind == TagKind.Close && openTags.Count > 0)
            openTags.RemoveAt(openTags.Count - 1);
    }

    /// <summary>
    /// Reopens any tags that were still open at the end of the previous line, so that the next line is valid markup on its own.
    /// </summary>
    private static void Reopen(List<Atom> line, List<string> openTags)
    {
        foreach (var tag in openTags)
            line.Add(new Atom(tag, 0, TagKind.Open));
    }

    private static void FlushLine(List<string> lines, List<Atom> line, List<string> openTags, bool hyphen = false)
    {
        // Trim trailing spaces from the end of the line, but not from the end of the word that was split across lines.
        while (line.Count > 0 && line[^1].IsSpace)
            line.RemoveAt(line.Count - 1);

        var sb = new StringBuilder();
        foreach (var atom in line)
            sb.Append(atom.Raw);

        if (hyphen)
            sb.Append('-');

        // Close any tags that were still open at the end of the line, so that the next line is valid markup on its own.
        for (var k = openTags.Count - 1; k >= 0; k--)
            sb.Append('[').Append('/').Append(GetTagName(openTags[k])).Append(']');

        lines.Add(sb.ToString());
        line.Clear();
    }

    private static string GetTagName(string rawOpenTag)
    {
        // rawOpenTag looks like "[color=#FFFFFF]", "[font=Foo size=12]" or "[bold]".
        var inner = rawOpenTag.Substring(1, rawOpenTag.Length - 2);
        var nameEnd = 0;
        while (nameEnd < inner.Length && char.IsLetterOrDigit(inner[nameEnd]))
            nameEnd++;

        return inner.Substring(0, nameEnd);
    }

    private static int SumVisible(List<Atom> atoms, int start, int end)
    {
        var sum = 0;
        for (var i = start; i < end; i++)
            sum += atoms[i].VisibleLength;

        return sum;
    }

    /// <summary>
    /// Finds the end of the next token (a word plus any trailing spaces) starting from the given index.
    /// </summary>
    private static int FindTokenEnd(List<Atom> atoms, int start, out int wordEnd)
    {
        var i = start;
        while (i < atoms.Count && !atoms[i].IsSpace)
            i++;

        wordEnd = i;

        while (i < atoms.Count && atoms[i].IsSpace)
            i++;

        return i;
    }

    private static List<Atom> Tokenize(string text)
    {
        var atoms = new List<Atom>(text.Length);
        var i = 0;
        while (i < text.Length)
        {
            var c = text[i];

            // Escape sequences are treated as a single visible character, so that they don't get split across lines.
            if (c == '\\' && i + 1 < text.Length && IsEscapable(text[i + 1]))
            {
                atoms.Add(new Atom(text.Substring(i, 2), 1, TagKind.None));
                i += 2;
                continue;
            }

            if (c == '[')
            {
                var end = text.IndexOf(']', i + 1);
                if (end == -1)
                {
                    // No closing bracket, treat the rest of the string as normal text!
                    for (var k = i; k < text.Length; k++)
                        atoms.Add(new Atom(text[k].ToString(), 1, TagKind.None));

                    break;
                }

                var raw = text.Substring(i, end - i + 1);
                var inner = raw.Substring(1, raw.Length - 2).TrimEnd();

                var kind = raw.Length >= 2 && raw[1] == '/'
                    ? TagKind.Close
                    : inner.EndsWith("/")
                        ? TagKind.SelfClosing
                        : TagKind.Open;

                atoms.Add(new Atom(raw, 0, kind));
                i = end + 1;
                continue;
            }

            atoms.Add(new Atom(c.ToString(), 1, TagKind.None));
            i++;
        }

        return atoms;
    }

    private static bool IsEscapable(char c) => c is '\\' or '[' or ']' or '/';
}
