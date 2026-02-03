namespace Content.Server._Gabystation.Jukebox;

[RegisterComponent]
public sealed partial class JukeboxMusicComponent : Component
{
    [DataField]
    public EntityUid Jukebox;
}