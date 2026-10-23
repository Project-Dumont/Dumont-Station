// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server.Actions;
using Content.Server.Humanoid;
using Content.Server.IdentityManagement;
using Content.Shared._EinsteinEngines.HeightAdjust;
using Content.Shared._Shitmed.Antags.Abductor;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mobs.Components;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Server._Trauma.Abductor;

public sealed partial class AbductorVestDisguiseSystem : EntitySystem
{
    [Dependency] private HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private MetaDataSystem _metaData = default!;
    [Dependency] private IdentitySystem _identity = default!;
    [Dependency] private ActionsSystem _actions = default!;
    [Dependency] private HeightAdjustSystem _heightAdjust = default!;

    private static readonly ProtoId<SpeciesPrototype> DisguiseSpecies = "Human";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbductorDisguiseStateComponent, DisguiseRevertEvent>(OnDisguiseRevert);
        SubscribeLocalEvent<AbductorVestDisguiseComponent, ComponentInit>(OnDisguiseAdded);
        SubscribeLocalEvent<AbductorVestDisguiseComponent, ComponentShutdown>(OnDisguiseRemoved);
    }

    private void OnDisguiseRevert(Entity<AbductorDisguiseStateComponent> ent, ref DisguiseRevertEvent args)
    {
        args.Handled = true;
        RestoreAppearance(ent.AsNullable(), args.RaiseRenameEvents);
    }

    private void OnDisguiseAdded(Entity<AbductorVestDisguiseComponent> ent, ref ComponentInit args)
    {
        var user = Transform(ent).ParentUid;
        if (!HasComp<MobStateComponent>(user))
            return;

        ApplyDisguise(user);
    }

    private void OnDisguiseRemoved(Entity<AbductorVestDisguiseComponent> ent, ref ComponentShutdown args)
    {
        var user = Transform(ent).ParentUid;
        if (!HasComp<MobStateComponent>(user))
            return;

        RestoreAppearance(user);
    }

    public void ApplyDisguise(EntityUid user,
        HumanoidCharacterProfile? disguiseProfile = null,
        EntProtoId? revertAction = null,
        bool allowRepeatedDisguise = false,
        bool raiseRenameEvents = true)
    {
        if (!TryComp<HumanoidAppearanceComponent>(user, out var humanoid))
            return;

        var name = Name(user);
        if (disguiseProfile?.Name == name)
            return;

        var disguise = EnsureComp<AbductorDisguiseStateComponent>(user);
        if (disguise.OriginalAppearance != null && !allowRepeatedDisguise)
            return;

        disguise.OriginalAppearance ??= SaveAppearance(humanoid);
        disguise.OriginalName ??= name;

        humanoid.CustomBaseLayers.Clear();

        disguiseProfile ??= HumanoidCharacterProfile.RandomWithSpecies(DisguiseSpecies);
        _humanoid.LoadProfile(user, disguiseProfile, humanoid);
        _metaData.SetEntityName(user, disguiseProfile.Name, raiseEvents: raiseRenameEvents);
        _identity.QueueIdentityUpdate(user);

        if (disguise.RevertAction != null || revertAction is not { } action)
            return;

        if (_actions.AddAction(user, ref disguise.RevertAction, action))
            _actions.SetEntityIcon(disguise.RevertAction.Value, user);
    }

    public void RestoreAppearance(Entity<AbductorDisguiseStateComponent?, HumanoidAppearanceComponent?> user,
        bool raiseRenameEvents = true)
    {
        if (!Resolve(user, ref user.Comp1, ref user.Comp2, false) ||
            user.Comp1.OriginalAppearance is not { } original ||
            user.Comp1.OriginalName is not { } name)
            return;

        var humanoid = user.Comp2;
        _humanoid.SetSpecies(user, original.Species, false, humanoid);
        _humanoid.SetSex(user, original.Sex, false, humanoid);
        _humanoid.SetSkinColor(user, original.SkinColor, false, false, humanoid);
        humanoid.EyeColor = original.EyeColor;
        humanoid.Gender = original.Gender;
        humanoid.Age = original.Age;
        humanoid.CustomBaseLayers = new(original.CustomBaseLayers);
        humanoid.MarkingSet = new MarkingSet(original.Markings);
        _humanoid.SetScale(user, new Vector2(original.Width, original.Height), false, humanoid);
        _heightAdjust.SetScale(user, new Vector2(humanoid.Width, humanoid.Height));
        Dirty(user, humanoid);

        _metaData.SetEntityName(user, name, raiseEvents: raiseRenameEvents);
        _identity.QueueIdentityUpdate(user);

        _actions.RemoveAction(user.Owner, user.Comp1.RevertAction);
        RemComp(user, user.Comp1);
    }

    private static DisguiseAppearance SaveAppearance(HumanoidAppearanceComponent humanoid)
    {
        return new DisguiseAppearance
        {
            Species = humanoid.Species,
            SkinColor = humanoid.SkinColor,
            EyeColor = humanoid.EyeColor,
            Sex = humanoid.Sex,
            Gender = humanoid.Gender,
            Age = humanoid.Age,
            Height = humanoid.Height,
            Width = humanoid.Width,
            CustomBaseLayers = new(humanoid.CustomBaseLayers),
            Markings = new MarkingSet(humanoid.MarkingSet),
        };
    }
}
