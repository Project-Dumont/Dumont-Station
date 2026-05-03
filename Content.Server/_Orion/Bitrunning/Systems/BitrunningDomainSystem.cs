using System.Diagnostics.CodeAnalysis;
using Content.Shared._Orion.Bitrunning.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Orion.Bitrunning.Systems;

public sealed class BitrunningDomainSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly List<BitrunningVirtualDomainPrototype> _allDomains = new();

    public override void Initialize()
    {
        base.Initialize();

        ReloadDomains();
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.ByType.TryGetValue(typeof(BitrunningVirtualDomainPrototype), out _))
            return;

        ReloadDomains();
    }

    private void ReloadDomains()
    {
        _allDomains.Clear();

        foreach (var domain in _prototype.EnumeratePrototypes<BitrunningVirtualDomainPrototype>())
        {
            _allDomains.Add(domain);
        }
    }

    public IReadOnlyList<BitrunningVirtualDomainPrototype> GetAllDomains()
    {
        return _allDomains.AsReadOnly();
    }

    public bool TryGetDomain(string id, [NotNullWhen(true)] out BitrunningVirtualDomainPrototype? domain)
    {
        return _prototype.TryIndex(id, out domain);
    }

    public string GetDisplayName(BitrunningVirtualDomainPrototype domain, int scannerTier, int points)
    {
        if (CanViewName(domain, scannerTier, points))
            return Loc.GetString(domain.Name);

        return Loc.GetString("bitrunning-console-redacted");
    }

    public string GetDisplayDescription(BitrunningVirtualDomainPrototype domain, int scannerTier, int points)
    {
        if (CanViewName(domain, scannerTier, points))
            return Loc.GetString(domain.Description);

        return Loc.GetString("bitrunning-console-redacted-desc");
    }

    public string GetDisplayReward(BitrunningVirtualDomainPrototype domain, int scannerTier, int points)
    {
        if (CanViewReward(domain, scannerTier, points))
        {
            return Loc.GetString("bitrunning-ui-domain-reward",
                ("server", domain.ServerRewardPoints),
                ("np", domain.BitrunningRewardPoints));
        }

        return Loc.GetString("bitrunning-console-redacted");
    }

    private static bool CanViewName(BitrunningVirtualDomainPrototype domain, int scannerTier, int points)
    {
        if (!domain.HiddenUntilScanned)
            return true;

        return scannerTier >= domain.RequiredScannerTier && points + domain.NameRevealPointBuffer >= domain.Cost;
    }

    private static bool CanViewReward(BitrunningVirtualDomainPrototype domain, int scannerTier, int points)
    {
        return scannerTier >= domain.RequiredScannerTier + 1 && points >= domain.RequiredPointsToRevealReward;
    }
}
