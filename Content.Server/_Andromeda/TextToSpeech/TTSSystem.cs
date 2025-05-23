// SPDX-FileCopyrightText: 2025 Dreykor <160512778+Dreykor@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 cosmosgc <cosmoskitsune@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Content.Server.Chat.Systems;
using Content.Server.Andromeda.TextToSpeech;
using Content.Shared.Humanoid;
using Content.Shared.Andromeda;
using Content.Shared.Andromeda.CCVar;
using Content.Shared.Andromeda.TextToSpeech;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.Language;
using static Content.Shared.Administration.Notes.AdminMessageEuiState;

-namespace Content.Server.Andromeda.TTS;
+namespace Content.Server._Andromeda.TextToSpeech;

public sealed partial class TTSSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ITTSManager _ttsManager = default!;
    [Dependency] private readonly IRobustRandom _rng = default!;
    [Dependency] private readonly LanguageSystem _language = default!;

    private readonly List<string> _sampleText =
    [
        "Alguém pode me trazer um par de luvas isolantes, por favor?",
        "Segurança, o palhaço roubou a identidade do capitão!",
        "A singularidade chegou à área de desembarque!",
        "Os robustos salvadores mais uma vez detiveram os agentes nucleares."
    ];

    private const int DefaultAnnounceVoice = 92;
    private const int MaxChars = 200;
    private const float WhisperVoiceVolumeModifier = 0.6f;
    private const int WhisperVoiceRange = 3;

    private readonly ISawmill _sawmill = Logger.GetSawmill("tts-system");
    private readonly List<ICommonSession> _ignoredRecipients = [];

    private bool _isEnabled;

    public override void Initialize()
    {
        _cfg.OnValueChanged(AndromedaCCVars.TTSEnabled, v => _isEnabled = v, true);

        SubscribeNetworkEvent<PreviewTTSRequestEvent>(OnRequestPreviewTTS);
        SubscribeNetworkEvent<ClientOptionTTSEvent>(OnClientOptionTTS);

        SubscribeLocalEvent<TransformSpeechEvent>(OnTransformSpeech);
        SubscribeLocalEvent<TextToSpeechComponent, EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<RadioSpokeEvent>(OnRadioReceiveEvent);
        SubscribeLocalEvent<AnnouncementSpokeEvent>(OnAnnouncementSpoke);
    }

    private async void OnRequestPreviewTTS(PreviewTTSRequestEvent ev, EntitySessionEventArgs args)
    {
        if (!_isEnabled ||
            !_prototypeManager.TryIndex<VoicePrototype>(ev.VoiceId, out var protoVoice))
            return;

        var previewText = _rng.Pick(_sampleText);
        var soundData = await GenerateTTS(previewText, protoVoice.Voice);
        if (soundData is null)
            return;

        RaiseNetworkEvent(new PlayTTSEvent { Data = soundData }, Robust.Shared.Player.Filter.SinglePlayer(args.SenderSession));
    }

    private async void OnClientOptionTTS(ClientOptionTTSEvent ev, EntitySessionEventArgs args)
    {
        if (ev.Enabled)
            _ignoredRecipients.Remove(args.SenderSession);
        else
            _ignoredRecipients.Add(args.SenderSession);
    }

    private void OnRadioReceiveEvent(RadioSpokeEvent args)
    {
        if (!_isEnabled
            || args.Message.Length > MaxChars)
            return;
        string newMessage = args.Message;
        _sawmill.Info(args.Language.ID);
        if (args.Language != null && args.Language.ID != "TauCetiBasic")
            newMessage = _language.ObfuscateSpeech(args.Message, args.Language);

        if (!TryComp(args.Source, out TextToSpeechComponent? senderComponent)
            || senderComponent.VoicePrototypeId is not string voiceId)
        {
            HandleRadio(args.Receivers, newMessage, 92);
        }
        else
        {
            var voice = _prototypeManager.TryIndex(voiceId, out VoicePrototype? proto) ? proto.Voice : 1;
            HandleRadio(args.Receivers, newMessage, voice);
        }
    }

    private async void OnAnnouncementSpoke(AnnouncementSpokeEvent args)
    {
        if (!_isEnabled
            || args.Message.Length > MaxChars * 2)
            return;

        var voice = _prototypeManager.TryIndex(args.AnnounceVoice ?? "", out VoicePrototype? proto)
            ? proto.Voice
            : DefaultAnnounceVoice;

        var soundData = await GenerateTTS(args.Message, voice, isAnnounce: true);
        soundData ??= [];
        RaiseNetworkEvent(new AnnounceTtsEvent
        {
            Data = soundData,
            AnnouncementSound = args.AnnouncementSound
        }, args.Source.RemovePlayers(_ignoredRecipients));
    }

    private async void OnEntitySpoke(EntityUid uid, TextToSpeechComponent component, EntitySpokeEvent args)
    {
        if (!_isEnabled || args.Message.Length > MaxChars) return;

        //Adicione condições para linguas aqui

        if (args.Language.ID == "Sign")
            return;

        //if (args.Language.ID != "TauCetiBasic")
        //    return;

        var voice = DefaultAnnounceVoice;
        if (!_prototypeManager.TryIndex(component.VoicePrototypeId ?? "", out VoicePrototype? proto))
        {
            var voices = _prototypeManager.TryGetInstances<VoicePrototype>(out var v) ? v.AsEnumerable() : [];
            if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoidAppearanceComponent)
                && humanoidAppearanceComponent?.Sex is Sex sex)
            {
                var voicePrototypes = voices.Where(x => !x.Value.Silicon
                    && (x.Value.Sex == Sex.Unsexed || sex == Sex.Unsexed || x.Value.Sex == sex)).ToArray();
                if (voicePrototypes.Length != 0)
                {
                    var index = Random.Shared.Next(voicePrototypes.Length);
                    var prototype = voicePrototypes[index];
                    voice = prototype.Value.Voice;
                    component.VoicePrototypeId = prototype.Value.ID;
                }
            }
            else
            {
                var voicePrototypes = voices.Where(x => x.Value.Silicon).ToArray();
                if (voicePrototypes.Length != 0)
                {
                    var index = Random.Shared.Next(voicePrototypes.Length);
                    var prototype = voicePrototypes[index];
                    voice = prototype.Value.Voice;
                    component.VoicePrototypeId = prototype.Value.ID;
                }
            }
        }
        else
            voice = proto.Voice;

        string newMessage = args.Message;
        if (args.Language.ID != "TauCetiBasic")
            newMessage = _language.ObfuscateSpeech(args.Message, args.Language);

        if (args.IsWhisper)
        {
            HandleWhisper(uid, newMessage, voice);
            return;
        }

        HandleSay(uid, newMessage, voice);
    }
    private void OnTransformSpeech(TransformSpeechEvent args)
    {
        if (!_isEnabled) return;
        args.Message = args.Message.Replace("+", "");
    }
    private async void HandleSay(EntityUid uid, string message, int voice)
    {
        var recipients = Robust.Shared.Player.Filter.Pvs(uid, 1F).RemovePlayers(_ignoredRecipients);

        var soundData = await GenerateTTS(message, voice);

        if (soundData is null)
            return;

        var netEntity = GetNetEntity(uid);

        if (TryComp<EyeComponent>(uid, out var eye) && eye is not null)
        {
            recipients.RemovePlayerByAttachedEntity(uid);
            RaiseNetworkEvent(new PlayTTSEvent
            {
                Data = soundData,
                SourceUid = GetNetEntity(eye.Target)
            }, Filter.Empty().FromEntities(uid));
        }

        RaiseNetworkEvent(new PlayTTSEvent
        {
            Data = soundData,
            SourceUid = netEntity
        }, recipients);
    }

    private async void HandleWhisper(EntityUid uid, string message, int voice)
    {
        var soundData = await GenerateTTS(message, voice);
        if (soundData is null)
            return;

        var transformQuery = GetEntityQuery<TransformComponent>();
        var sourcePos = _xforms.GetWorldPosition(transformQuery.GetComponent(uid), transformQuery);
        var receptions = Filter.Pvs(uid).Recipients;
        foreach (var session in receptions)
        {
            if (!session.AttachedEntity.HasValue
                || _ignoredRecipients.Contains(session))
                continue;

            var transform = transformQuery.GetComponent(session.AttachedEntity.Value);
            var distance = (sourcePos - _xforms.GetWorldPosition(transform, transformQuery)).LengthSquared();

            if (distance > WhisperVoiceRange)
                continue;

            if (session.AttachedEntity == uid && TryComp<EyeComponent>(uid, out var eye) && eye is not null)
            {
                RaiseNetworkEvent(new PlayTTSEvent
                {
                    Data = soundData,
                    SourceUid = GetNetEntity(eye.Target)
                }, Filter.Empty().FromEntities(uid));
            }
            else
            {
                RaiseNetworkEvent(new PlayTTSEvent
                {
                    Data = soundData,
                    SourceUid = GetNetEntity(uid),
                    VolumeModifier = WhisperVoiceVolumeModifier * (1f - distance / WhisperVoiceRange)
                }, session);
            }
        }
    }

    private async void HandleRadio(EntityUid[] uIds, string message, int voice)
    {
        var soundData = await GenerateTTS(message, voice, isRadio: true);
        if (soundData is null)
            return;

        RaiseNetworkEvent(new PlayTTSEvent { IsRadio = true, Data = soundData }, Filter.Entities(uIds).RemovePlayers(_ignoredRecipients));
    }

    private async Task<byte[]?> GenerateTTS(string text, int voice, bool isRadio = false, bool isAnnounce = false)
    {
        _sawmill.Warning($"TTS System Log a: {text}");
        try
        {
            text = DecimalConverter().Replace(text, " point ");
            text = Number2Word().Replace(text, ReplaceNumber2Word);
            text = CyrillicCharFilter().Replace(text, ReplaceCyrillicChar);
            text = SymbolFilter().Replace(text, ReplaceAbbreviations);
            text = CharFilter().Replace(text.Trim(), "");

            if (text == "") return null;
            if (char.IsLetter(text[^1]))
                text += ".";
            _sawmill.Warning($"TTS System Log f: {text}");

            return isRadio
               ? await _ttsManager.ConvertTextToSpeechRadio(voice, text)
               : isAnnounce
                   ? await _ttsManager.ConvertTextToSpeechAnnounce(voice, text)
                   : await _ttsManager.ConvertTextToSpeechStandard(voice, text);

        }
        catch (Exception e)
        {
            _sawmill.Error($"TTS System error: {e.Message}");
        }

        return null;
    }
    private string ReplaceNumber2Word(Match word)
        => !long.TryParse(word.Value, out var number) ? word.Value : NumberConverter.NumberToText(number);
    private string ReplaceAbbreviations(Match word)
        => _wordReplacement.TryGetValue(word.Value.ToLower(), out var replace) ? replace : word.Value;

    private static readonly IReadOnlyDictionary<string, string> _wordReplacement =
        new Dictionary<string, string>()
        {
            {"id", "Ai Di"},
            {"pda", "P D A"},
            {"sci", "sai"},
            {"vdd", "verdade"},
            {"fds", "foda se"},
            {"blz", "beleza"},
            {"vlw", "valeu"},
            {"flw", "falou"},
            {"qq", "qualquer coisa"},
            {"pq", "porque"},
            {"q", "que"},
            {"kd", "cadê"},
            {"tmj", "tamo junto"},
            {"obg", "obrigado"},
            {"obgda", "obrigada"},
            {"vc", "você"},
            {"vcs", "vocês"},
            {"mt", "muito"},
            {"mto", "muito"},
            {"msm", "mesmo"},
            {"aq", "aqui"},
            {"aki", "aqui"},
            {"cmg", "comigo"},
            {"ctz", "certeza"},
            {"n", "não"},
            {"ñ", "não"},
            {"s", "sim"},
            {"bjs", "beijos"},
            {"bj", "beijo"},
            {"pfv", "por favor"},
            {"pls", "por favor"},
            {"gr4", "grana"},
            {"dps", "depois"},
            {"hj", "hoje"},
            {"amanha", "amanhã"},
            {"mlk", "moleque"},
            {"crlh", "caralho"},

            //owo
            {"(•`ω´•)", "meow"},
            {";;w;;", "meow"},
            {"owo", "meow"},
            {"UwU", "meow"},
            {">w<", "meow"},
            {"^w^", "meow"},

            //russian
            {"Д", "A"},
            {"в", "b"},
            {"И", "N"},
            {"и", "n"},
            {"К", "K"},
            {"к", "k"},
            {"м", "m"},
            {"н", "h"},
            {"т", "t"},
            {"Я", "R"},
            {"я", "r"},
            {"У", "Y"},
            {"Ш", "W"},
            {"ш", "w"},
        };

    private string ReplaceCyrillicChar(Match match)
    => _cyrillicReplacement.TryGetValue(match.Value, out var replace) ? replace : match.Value;

    private static readonly IReadOnlyDictionary<string, string> _cyrillicReplacement =
        new Dictionary<string, string>()
        {
        {"Д", "A"},
        {"в", "b"},
        {"И", "N"},
        {"и", "n"},
        {"К", "K"},
        {"к", "k"},
        {"м", "m"},
        {"н", "h"},
        {"т", "t"},
        {"Я", "R"},
        {"я", "r"},
        {"У", "Y"},
        {"Ш", "W"},
        {"ш", "w"},
        };


    [GeneratedRegex(@"[^\p{L}\p{N},\-+?!. ]")]
    private static partial Regex CharFilter();

    [GeneratedRegex(@"(?<=\d)[.,](?=\d)")]
    private static partial Regex DecimalConverter();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Number2Word();

    [GeneratedRegex(@"[а-яА-ЯёЁ]", RegexOptions.IgnoreCase | RegexOptions.Multiline, "en-US")]
    private static partial Regex CyrillicCharFilter();


    [GeneratedRegex(@"\b([a-zA-Zа-яёА-ЯЁ]+|(\(•`ω´•\)|;;w;;|owo|UwU|>w<|\^w\^))\b", RegexOptions.IgnoreCase | RegexOptions.Multiline, "en-US")]
    private static partial Regex SymbolFilter();

}
