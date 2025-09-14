using Robust.Shared.Configuration;
using Content.Shared.ADT.Supermatter;
using Content.Shared.ADT.Supermatter.Components;
using Content.Shared.Atmos;
using Robust.Shared;

namespace Content.Shared.ADT.CCVar;

[CVarDefs]
public sealed class ADTCCVars
{
    /*
    * BookPrinter
    */

    public static readonly CVarDef<float> BookPrinterUploadCooldown =
        CVarDef.Create("bookprinter.upload_cooldown", 3600.0f, CVar.SERVERONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> BookPrinterUploadCooldownEnabled =
        CVarDef.Create("bookprinter.upload_cooldown_enabled", true, CVar.SERVERONLY | CVar.ARCHIVE);
}

