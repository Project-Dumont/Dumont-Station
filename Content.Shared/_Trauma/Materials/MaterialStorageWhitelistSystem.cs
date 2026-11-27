// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Materials;

namespace Content.Trauma.Shared.Materials;

public sealed class MaterialStorageWhitelistSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MaterialStorageWhitelistComponent, GetMaterialWhitelistEvent>(OnGetMaterialWhitelist);
    }

    private void OnGetMaterialWhitelist(Entity<MaterialStorageWhitelistComponent> ent, ref GetMaterialWhitelistEvent args)
    {
        args.Whitelist.AddRange(ent.Comp.Whitelist);
    }
}
