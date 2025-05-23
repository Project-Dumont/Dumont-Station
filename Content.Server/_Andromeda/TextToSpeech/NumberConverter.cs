// SPDX-FileCopyrightText: 2025 Dreykor <160512778+Dreykor@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 cosmosgc <cosmoskitsune@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;

namespace Content.Server.Andromeda.TTS;

public static class NumberConverter
{
    private static readonly string[] Units =
    [
        "", "um", "dois", "três", "quatro", "cinco", "seis",
        "sete", "oito", "nove", "dez", "onze",
        "doze", "treze", "quatorze", "quinze",
        "dezesseis", "dezessete", "dezoito", "dezenove"
    ];

    private static readonly string[] Tens =
    [
        "", "dez", "vinte", "trinta", "quarenta", "cinquenta",
        "sessenta", "setenta", "oitenta", "noventa"
    ];

    private static readonly string[] Scales =
        ["", "mil", "milhões", "bilhões", "trilhões"];

    public static string NumberToText(long number)
    {
        if (number == 0)
            return "zero";

        if (number < 0)
            return "menos " + NumberToText(-number);

        var words = new StringBuilder();

        var unit = 0;

        while (number > 0)
        {
            if (number % 1000 != 0)
            {
                var chunk = new StringBuilder();

                var hundreds = (int)(number % 1000 / 100);
                var tensUnits = (int)(number % 100);

                if (hundreds != 0)
                    chunk.Append(hundreds == 1 ? "cem" : Units[hundreds] + " centos");

                if (tensUnits > 0)
                {
                    if (hundreds != 0)
                        chunk.Append(" e ");

                    if (tensUnits < 20)
                        chunk.Append(Units[tensUnits]);
                    else
                    {
                        var tens = tensUnits / 10;
                        var units = tensUnits % 10;

                        chunk.Append(Tens[tens]);

                        if (units != 0)
                            chunk.Append("-" + Units[units]);
                    }
                }

                // Ajustar para singular quando o multiplicador é 1
                if (unit > 0 && number % 1000 == 1)
                {
                    // Converter para forma singular (remover o "ões" final)
                    var scale = Scales[unit];
                    if (scale.EndsWith("ões"))
                        scale = scale.Substring(0, scale.Length - 3) + "ão";
                    chunk.Append(" " + scale);
                }
                else
                {
                    chunk.Append(" " + Scales[unit]);
                }

                if (words.Length > 0)
                    chunk.Append(" e ");

                words.Insert(0, chunk);
            }

            number /= 1000;
            unit++;
        }

        return words.ToString().Trim();
    }
}
