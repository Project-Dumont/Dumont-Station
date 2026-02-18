using System.Linq;
using System.Text.RegularExpressions;
using Content.Shared.Chat;
using Robust.Shared.Utility;

namespace Content.Client._UM.UserInterface.Controls;

public sealed class WordWrapHelper
{
    public static IEnumerable<string> WordWrap(string text, int charLimit)
    {
        var lines = new List<string>();
        var currentLine = "";

        int i = 0;
        while (i < text.Length)
        {
            string token = ExtractToken(text, i, out int nextIndex);
            string word = token.TrimEnd(' ');
            string spaces = token.Substring(word.Length);

            if ((currentLine + token).Length <= charLimit)
            {
                currentLine += token;
                i = nextIndex;
                continue;
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.TrimEnd(' '));
                currentLine = "";
            }

            if (word.Length <= charLimit)
            {
                currentLine = word + spaces;
                i = nextIndex;
            }
            else
            {
                // Word is too long - must split it with hyphen
                int available = charLimit - 1;
                currentLine = word.Substring(0, available) + "-";
                lines.Add(currentLine);
                currentLine = "";
                i += available;
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine.TrimEnd(' '));
        }

        foreach (var line in lines)
        {
            yield return line;
        }
    }

    /// <summary>
    /// Extracts the next token (word + trailing spaces) from text starting at position i
    /// </summary>
    private static string ExtractToken(string text, int startIndex, out int nextIndex)
    {
        int i = startIndex;

        // Read the word (non-space characters)
        while (i < text.Length && text[i] != ' ')
        {
            i++;
        }

        // Read all trailing spaces
        while (i < text.Length && text[i] == ' ')
        {
            i++;
        }

        nextIndex = i;
        return text.Substring(startIndex, i - startIndex);
    }
}
