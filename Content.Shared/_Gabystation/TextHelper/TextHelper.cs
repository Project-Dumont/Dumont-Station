using System.Text;
using System.Globalization;

namespace Content.Shared._Gabystation.TextHelper;

public static class TextHelper
{
    public static string RemoveAccents(string input)
    {
        /*
        string normalized = input.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
        */
        return input;
    }
}