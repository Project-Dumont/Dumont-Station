// SPDX-FileCopyrightText: 2025 cosmosgc <cosmoskitsune@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading.Tasks;

namespace Content.Server.Andromeda.TextToSpeech;
public interface ITTSManager
{
    Task<byte[]?> ConvertTextToSpeechAnnounce(int voice, string text);
    Task<byte[]?> ConvertTextToSpeechRadio(int voice, string text);
    Task<byte[]?> ConvertTextToSpeechStandard(int voice, string text);
    void Initialize();
}
