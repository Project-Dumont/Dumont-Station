// SPDX-FileCopyrightText: 2024 Daniela <43686351+Day-OS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Diego Leite Asprino <98828735+dasprino007@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DoutorWhite <thedoctorwhite@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 RadsammyT <32146976+RadsammyT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Daniela <daniela.paladinof@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._EstacaoPirata.Cards.Card;
using Content.Shared._EstacaoPirata.Cards.Stack;
using Content.Shared.Audio;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._EstacaoPirata.Cards.Deck;

/// <summary>
/// This handles card decks
///
/// </summary>
public sealed class CardDeckSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly CardStackSystem _cardStackSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<CardDeckComponent, GetVerbsEvent<AlternativeVerb>>(AddTurnOnVerb);
    }

    private void AddTurnOnVerb(EntityUid uid, CardDeckComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (!TryComp(uid, out CardStackComponent? comp))
            return;

        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () => TryShuffle(uid, component, comp),
            Text = Loc.GetString("cards-verb-shuffle"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/die.svg.192dpi.png")),
            Priority = 4
        });
        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () => TrySplit(args.Target, component, comp, args.User),
            Text = Loc.GetString("cards-verb-split"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/dot.svg.192dpi.png")),
            Priority = 3
        });
        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () => TryOrganize(uid, component, comp, true),
            Text = Loc.GetString("cards-verb-organize-down"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/flip.svg.192dpi.png")),
            Priority = 2
        });
        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () => TryOrganize(uid, component, comp, false),
            Text = Loc.GetString("cards-verb-organize-up"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/flip.svg.192dpi.png")),
            Priority = 1
        });
    }

    private void TrySplit(EntityUid uid, CardDeckComponent deck, CardStackComponent stack, EntityUid user)
    {
        if (stack.Cards.Count <= 1)
            return;

        if (!TryComp(stack.Cards.First(), out CardComponent? firstCardComp))
            return;

        _audio.PlayPredicted(deck.PickUpSound, Transform(uid).Coordinates, user);

        if (!_net.IsServer)
            return;

        var cardDeck = SpawnInSameParent(firstCardComp.CardDeckBaseName, uid);

        EnsureComp<CardStackComponent>(cardDeck, out var deckStack);

        _cardStackSystem.TransferNLastCardFromStacks(user, stack.Cards.Count / 2, uid, stack, cardDeck, deckStack);
        _hands.PickupOrDrop(user, cardDeck);
    }

    private void TryShuffle(EntityUid deck, CardDeckComponent comp, CardStackComponent? stack)
    {
        _cardStackSystem.ShuffleCards(deck, stack);
        if (_net.IsClient)
            return;

        _audio.PlayPvs(comp.ShuffleSound, deck, AudioHelpers.WithVariation(0.05f, _random));
        _popup.PopupEntity(Loc.GetString("card-verb-shuffle-success", ("target", MetaData(deck).EntityName)), deck);
    }

    private void TryOrganize(EntityUid deck, CardDeckComponent comp, CardStackComponent? stack, bool isFlipped)
    {
        if (_net.IsClient)
            return;
        _cardStackSystem.FlipAllCards(deck, stack, isFlipped: isFlipped);

        _audio.PlayPvs(comp.ShuffleSound, deck, AudioHelpers.WithVariation(0.05f, _random));
        _popup.PopupEntity(Loc.GetString("card-verb-organize-success", ("target", MetaData(deck).EntityName), ("facedown", isFlipped)), deck);
    }

    private EntityUid SpawnInSameParent(string prototype, EntityUid uid)
    {
        if (_container.IsEntityOrParentInContainer(uid) &&
            _container.TryGetOuterContainer(uid, Transform(uid), out var container))
        {
            return SpawnInContainerOrDrop(prototype, container.Owner, container.ID);
        }
        return Spawn(prototype, Transform(uid).Coordinates);
    }
}
