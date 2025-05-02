using Content.Shared.Language;

namespace Content.Server.Andromeda.TTS;

public sealed class RadioSpokeEvent : EntityEventArgs
{
    public EntityUid Source { get; set; }
    public string Message { get; set; } = null!;
    public EntityUid[] Receivers { get; set; } = null!;
    public LanguagePrototype Language { get; set; } = new LanguagePrototype();
}
