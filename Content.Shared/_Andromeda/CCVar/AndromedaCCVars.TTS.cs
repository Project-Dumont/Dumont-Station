using Robust.Shared.Configuration;

namespace Content.Shared.Andromeda.CCVar;
[CVarDefs]

public sealed partial class AndromedaCCVars
{
    /// <summary>
    /// Liga TTS
    /// </summary>
    public static readonly CVarDef<bool> TTSEnabled =
        CVarDef.Create("tts.enabled", false, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    /// URL of the TTS server API.
    /// </summary>
    public static readonly CVarDef<string> TTSApiUrl =
        CVarDef.Create("tts.api_url", "http://example:3000/api/tts", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Auth token of the TTS server API.
    /// </summary>
    public static readonly CVarDef<string> TTSApiToken =
        CVarDef.Create("tts.api_token", "1111", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Amount of seconds before timeout for API
    /// </summary>
    public static readonly CVarDef<int> TTSApiTimeout =
        CVarDef.Create("tts.api_timeout", 5, CVar.SERVERONLY | CVar.ARCHIVE);


    /// <summary>
    /// Esses anuncios são eventos como ánúncio do console de comunicação, eventos anunciados na radio e alertas
    /// </summary>
    public static readonly CVarDef<bool> TTsAnnounceGlobalEnabled =
        CVarDef.Create("tts.announce_global_enabled", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);


    /// <summary>
    /// Esses anuncios são eventos como "Nome da pessoa (Capitão) chegou a estação
    /// </summary>
    public static readonly CVarDef<bool> TTsAnnounceDispatchEnabled =
        CVarDef.Create("tts.announce_dispatch_enabled", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    /// Option to disable TTS events for client
    /// </summary>
    public static readonly CVarDef<bool> TTSClientEnabled =
        CVarDef.Create("tts.client_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Default volume setting of TTS sound
    /// </summary>
    public static readonly CVarDef<float> TTSVolume =
        CVarDef.Create("tts.volume", 0.50f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> TTSRadioVolume =
        CVarDef.Create("tts.radio_volume", 0.50f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> TTSRadioQueueEnabled =
        CVarDef.Create("tts.radio_queue_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> TTSAnnounceVolume =
        CVarDef.Create("tts.announce_volume", 0.50f, CVar.CLIENTONLY | CVar.ARCHIVE);
}
