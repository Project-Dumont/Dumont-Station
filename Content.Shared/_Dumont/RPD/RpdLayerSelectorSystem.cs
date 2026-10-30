using Content.Shared.Atmos.Components;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Shared._Dumont.RPD;

public sealed partial class RpdLayerSelectorSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RpdLayerSelectorComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerbs);
        SubscribeLocalEvent<RpdLayerSelectorComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<RpdLayerSelectorComponent> ent, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange)
            args.PushText(Loc.GetString("rpd-layer-selector-examine", ("layer", GetLayerName(ent.Comp.SelectedLayer))));
    }

    private void OnGetAlternativeVerbs(Entity<RpdLayerSelectorComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("rpd-layer-selector-cycle"),
            Act = () =>
            {
                ent.Comp.SelectedLayer = ent.Comp.SelectedLayer switch
                {
                    null => AtmosPipeLayer.Primary,
                    AtmosPipeLayer.Primary => AtmosPipeLayer.Secondary,
                    AtmosPipeLayer.Secondary => AtmosPipeLayer.Tertiary,
                    _ => null,
                };
                Dirty(ent);
                _popup.PopupClient(Loc.GetString("rpd-layer-selector-changed", ("layer", GetLayerName(ent.Comp.SelectedLayer))), ent, user);
            },
        });
    }

    private string GetLayerName(AtmosPipeLayer? layer)
    {
        return Loc.GetString(layer switch
        {
            AtmosPipeLayer.Primary => "rpd-layer-selector-1",
            AtmosPipeLayer.Secondary => "rpd-layer-selector-2",
            AtmosPipeLayer.Tertiary => "rpd-layer-selector-3",
            _ => "rpd-layer-selector-auto",
        });
    }
}
