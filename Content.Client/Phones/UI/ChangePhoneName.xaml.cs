// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Shared.Phones;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.Phones.UI;

public sealed partial class ChangePhoneName : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public event Action<string>? OnTextChanged;
    public event Action<string>? OnCategoryChanged;

    private LineEdit _messageEdit = default!;
    private OptionButton _categoryOption = default!;

    public ChangePhoneName()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        // FIND XAML CONTROLS
        _messageEdit = this.FindControl<LineEdit>("MessageEdit");
        _categoryOption = this.FindControl<OptionButton>("CategoryOption");

        FillCategories();

        _messageEdit.OnTextChanged += _ =>
        {
            OnTextChanged?.Invoke(_messageEdit.Text);
        };

        _categoryOption.OnItemSelected += args =>
        {
            _categoryOption.SelectId(args.Id);

            var selectedText =
                _categoryOption.GetItemMetadata(args.Id)?.ToString()
                ?? "hidden";

            OnCategoryChanged?.Invoke(selectedText);
        };
    }

    private void FillCategories()
    {
        var sortedList = _prototype
            .EnumeratePrototypes<PhoneCategoryPrototype>()
            .OrderBy(p => p.Index)
            .ToList();

        for (var i = 0; i < sortedList.Count; i++)
        {
            _categoryOption.AddItem(
                Loc.GetString(sortedList[i].Text));

            _categoryOption.SetItemMetadata(
                i,
                sortedList[i].ID);
        }
    }
}