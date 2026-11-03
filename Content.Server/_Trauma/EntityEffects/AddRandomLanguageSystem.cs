// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._EinsteinEngines.Language;
using Content.Shared._EinsteinEngines.Language.Components;
using Content.Shared.EntityEffects;
using Content.Trauma.Shared.EntityEffects;
using Robust.Shared.Random;

namespace Content.Trauma.Server.EntityEffects;

public sealed partial class AddRandomLanguageSystem : EntitySystem
{
    [Dependency] private LanguageSystem _language = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<AddRandomLanguage>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<AddRandomLanguage> args)
    {
        var uid = args.Args.TargetEntity;
        if (!TryComp<LanguageSpeakerComponent>(uid, out var speaker))
            return;

        var lang = _random.Pick(args.Effect.Languages);
        _language.AddLanguage(uid, lang);
        _language.SetLanguage((uid, speaker), lang);
    }
}
