// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 João Fernandez <joaorbfernandez@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Gabystation.Text
{
    /// <summary>
    /// Utility methods for text manipulation and processing
    /// </summary>
    public static class TextHelpers
    {
        /// <summary>
        /// Removes common accents from text for better search matching.
        /// </summary>
        /// <param name="text">The text to remove accents from</param>
        /// <returns>Text with accents removed</returns>
        public static string RemoveAccents(string text)
        {
            return text
                .Replace("á", "a").Replace("à", "a").Replace("ã", "a").Replace("â", "a").Replace("ä", "a")
                .Replace("é", "e").Replace("ê", "e").Replace("è", "e").Replace("ë", "e")
                .Replace("í", "i").Replace("î", "i").Replace("ì", "i").Replace("ï", "i")
                .Replace("ó", "o").Replace("ô", "o").Replace("õ", "o").Replace("ò", "o").Replace("ö", "o")
                .Replace("ú", "u").Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
                .Replace("ç", "c")
                .Replace("Á", "A").Replace("À", "A").Replace("Ã", "A").Replace("Â", "A").Replace("Ä", "A")
                .Replace("É", "E").Replace("Ê", "E").Replace("È", "E").Replace("Ë", "E")
                .Replace("Í", "I").Replace("Î", "I").Replace("Ì", "I").Replace("Ï", "I")
                .Replace("Ó", "O").Replace("Ô", "O").Replace("Õ", "O").Replace("Ò", "O").Replace("Ö", "O")
                .Replace("Ú", "U").Replace("Ù", "U").Replace("Û", "U").Replace("Ü", "U")
                .Replace("Ç", "C");
        }
    }
}
