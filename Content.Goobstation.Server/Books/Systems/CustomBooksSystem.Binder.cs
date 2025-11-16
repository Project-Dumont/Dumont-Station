// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Books;

namespace Content.Goobstation.Server.Books;

public sealed partial class CustomBooksSystem
{
    private void InitializeBinder()
    {
        SubscribeLocalEvent<BookBinderComponent, MapInitEvent>(OnBinderInit);
        SubscribeLocalEvent<BookBinderComponent, CreateBookMessage>(OnCreateBookMessage);
        SubscribeLocalEvent<BookBinderComponent, EjectBinderPageMessage>(OnEjectBinderPage);
    }

    private void OnBinderInit(Entity<BookBinderComponent> ent, ref MapInitEvent args)
    {
        Appearance.SetData(ent.Owner, BookBinderVisuals.Inserting, false);
    }

    private void OnCreateBookMessage(Entity<BookBinderComponent> ent, ref CreateBookMessage args)
    {
        Container.EmptyContainer(ent.Comp.PaperContainer, true);
        _audio.PlayPvs(ent.Comp.BookCreatedSound, ent.Owner);
        UpdateBinderUi(ent);

        var book = Spawn("CustomBookTemplate", Transform(ent.Owner).Coordinates);
        var comp = EnsureComp<CustomBookComponent>(book);

        comp.Author = args.AuthorName;
        comp.Genre = args.Genre;
        comp.Title = args.Title;
        comp.Pages = new(args.Pages);
        comp.Binding = args.Binding;
        comp.Desc = args.Description;

        RegenerateBook((book, comp));
    }

    private void OnEjectBinderPage(Entity<BookBinderComponent> ent, ref EjectBinderPageMessage args)
    {
        Container.TryRemoveFromContainer(GetEntity(args.Page), true);
        UpdateBinderUi(ent);
    }
}
