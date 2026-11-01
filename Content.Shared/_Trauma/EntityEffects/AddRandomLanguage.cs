// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Language;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects;

/// <summary>
/// Changes the target's active language to one randomly picked from a list.
/// </summary>
public sealed partial class AddRandomLanguage : EventEntityEffect<AddRandomLanguage>
{
    [DataField]
    public List<ProtoId<LanguagePrototype>> Languages = new()
    {
        "Monkey",
        "RootSpeak",
        "Moffic",
        "Draconic",
        "Calcic",
        "Bubblish",
        "NewKinPidgin",
        "Xeeplian",
        "Elyran",
        "Freespeak",
        "NovuNederic",
        "Tradeband",
        "SpaceItalian",
        "Carptongue"
    };

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
