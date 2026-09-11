// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Trauma.Shared.BloodSplatter;

using BodySystem = Content.Shared.Body.Systems.SharedBodySystem;
using BodyPartSystem = Content.Shared.Body.Systems.SharedBodySystem;

using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Body.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

using System.Linq;
// Dumont end

namespace Content.Trauma.Shared.Heretic.EntityEffects;

public sealed partial class LockBladeEffect : EntityEffectBase<LockBladeEffect>
{
    [DataField]
    public SoundSpecifier WoundSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/blood3.ogg");

    [DataField]
    public SoundSpecifier OpeningSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/goresplat.ogg");

    [DataField]
    public EntProtoId Wound = "WeepingAvulsion";

    [DataField]
    public EntProtoId BloodChunk = "BloodChunkEffect";

    [DataField]
    public ProtoId<DamageGroupPrototype> DamageGroup = "Brute";
}

public sealed partial class LockBladeEffectSystem : EntityEffectSystem<BodyComponent, LockBladeEffect>
{
    [Dependency] private WoundSystem _wound = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private ThrowingSystem _throw = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private BodyPartSystem _part = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    protected override void Effect(Entity<BodyComponent> target, ref EntityEffectEvent<LockBladeEffect> args)
    {
        // TODO: predict ts
        if (_net.IsClient)
            return;

        var targeting = CompOrNull<TargetingComponent>(args.User);

        var (type, symmetry) = _body.ConvertTargetBodyPart(targeting?.Target ?? TargetBodyPart.Chest);
        if (_part.GetBodyChildrenOfType(target, type, symmetry: symmetry).Select(x => x.Id).FirstOrNull() is not { } targetPart)
            return;

        if (!_wound.TryInduceWound(targetPart, args.Effect.Wound, 25f, out _))
            return;

        var effectAmount = 1f;

        // Open ribcage for easier ascension if chest is mangled
        if (_part.GetParentPartOrNull(targetPart) == null &&
            TryComp(targetPart, out WoundableComponent? woundable) &&
            woundable.WoundableSeverity >= WoundableSeverity.Mangled &&
            (!EnsureComp<SkinRetractedComponent>(targetPart, out _) |
             !EnsureComp<IncisionOpenComponent>(targetPart, out _) |
             !EnsureComp<BonesSawedComponent>(targetPart, out _) |
             !EnsureComp<BonesOpenComponent>(targetPart, out _)))
        {
            _audio.PlayPvs(args.Effect.OpeningSound, target);
            effectAmount = 2;
        }
        else
            _audio.PlayPvs(args.Effect.WoundSound, target);

        if (!TryComp(target, out BloodstreamComponent? bloodStream))
            return;

        effectAmount *= _random.Next(3, 6);

        var coords = _transform.GetMapCoordinates(target);
        var color = _proto.Index(bloodStream.BloodReagent).SubstanceColor;

        for (var i = 0; i < effectAmount; i++)
        {
            var dir = _random.NextAngle().ToVec();
            var chunk = Spawn(args.Effect.BloodChunk, coords);
            var comp = EnsureComp<BloodSplatterOnLandComponent>(chunk);
            comp.Color = color;
            Dirty(chunk, comp);

            if (TryComp(chunk, out TrailComponent? trail))
            {
                trail.Color = color;
                Dirty(chunk, trail);
            }

            _throw.TryThrow(chunk,
                direction: dir * _random.NextVector2(1f, 3f),
                baseThrowSpeed: _random.NextFloat(1f, 2.5f),
                pushbackRatio: 0f,
                friction: 2f,
                recoil: false,
                playSound: false);
        }
    }
}
