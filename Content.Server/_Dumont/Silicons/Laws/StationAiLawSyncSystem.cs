// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Radio.EntitySystems;
using Content.Server.Silicons.Laws;
using Content.Server.Station.Systems;
using Content.Server._DV.Silicons.Laws;
using Content.Shared._DV.Silicons.Laws;
using Content.Shared.GameTicking;
using Content.Shared.Radio;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Silicons.Laws;

/// <summary>
/// espelha as leis da IA nos borgs escravizados a ela
/// o upstream já faz os borgs da NT obedecerem a IA mas só por uma lei estática. o conjunto
/// de leis real nunca chegava neles, então uma IA com ion storm mandava em borg rodando
/// Crewsimov..
/// de propósito não engancha na mudança de lei: compara por tempo, o que dá o atraso que o
/// balanço precisa. IA subvertida não vira o departamento inteiro no mesmo fôlego, e a
/// estação recebe aviso no rádio enquanto acontece
/// </summary>
public sealed class StationAiLawSyncSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly SiliconLawSystem _laws = default!;
    [Dependency] private readonly SlavedBorgSystem _slaved = default!;
    [Dependency] private readonly StationSystem _station = default!;

    /// <summary>
    /// quanto tempo as leis da IA precisam ficar paradas antes de chegar nos borgs. é a
    /// janela que a tripulação tem pra perceber e cortar o vínculo
    /// </summary>
    private static readonly TimeSpan SyncDelay = TimeSpan.FromSeconds(45);

    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(5);

    private const string AnnouncementChannel = "Science";

    private TimeSpan _nextCheck;

    /// <summary>
    /// última assinatura de leis conhecida por IA e quando mudou
    /// </summary>
    private readonly Dictionary<EntityUid, (string Signature, TimeSpan ChangedAt, bool Propagated)> _seen = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _seen.Clear();
        _nextCheck = TimeSpan.Zero;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        if (now < _nextCheck)
            return;

        _nextCheck = now + CheckInterval;

        var query = EntityQueryEnumerator<StationAiHeldComponent, SiliconLawProviderComponent>();
        while (query.MoveNext(out var ai, out _, out _))
        {
            Track(ai, now);
        }
    }

    private void Track(EntityUid ai, TimeSpan now)
    {
        var laws = _laws.GetLaws(ai);
        var signature = Signature(laws);

        if (!_seen.TryGetValue(ai, out var previous))
        {
            _seen[ai] = (signature, now, true);
            return;
        }

        if (previous.Signature != signature)
        {
            _seen[ai] = (signature, now, false);
            Warn(ai, "station-ai-law-sync-detected");
            return;
        }

        if (previous.Propagated || now < previous.ChangedAt + SyncDelay)
            return;

        _seen[ai] = (signature, previous.ChangedAt, true);
        Propagate(ai, laws);
    }

    /// <summary>
    /// empurra as leis da IA pra todo borg escravizado na mesma estação
    /// aqui é fail-closed de propósito, IA sem estação não manda lei pra ninguém. o filtro
    /// de alarme é fail-open porque ouvir um alarme a mais não custa nada, lei custa
    /// </summary>
    private void Propagate(EntityUid ai, SiliconLawset laws)
    {
        var station = _station.GetOwningStation(ai);
        if (station == null)
            return;

        var count = 0;

        var query = EntityQueryEnumerator<SlavedBorgComponent, SiliconLawProviderComponent>();
        while (query.MoveNext(out var borg, out var slaved, out _))
        {
            if (_station.GetOwningStation(borg) != station)
                continue;

            if (!_proto.TryIndex(slaved.Law, out var slaveLawProto))
                continue;

            var copy = laws.Laws.Select(law => law.ShallowClone()).ToList();
            copy.RemoveAll(law => law.LawString == slaveLawProto.LawString);

            var lawset = new SiliconLawset { Laws = copy };
            _slaved.AddLaw(lawset, slaved.Law);

            _laws.SetLaws(lawset.Laws, borg);
            count++;
        }

        if (count > 0)
            Warn(ai, "station-ai-law-sync-applied");
    }

    /// <summary>
    /// mudança de lei de silicon tem que ser perceptível, anunciar no canal da ciência é o
    /// contrajogo.. a robótica consegue tirar um borg do vínculo antes ou depois de cair
    /// </summary>
    private void Warn(EntityUid ai, LocId message)
    {
        _radio.SendRadioMessage(ai, Loc.GetString(message), AnnouncementChannel, ai);
    }

    private static string Signature(SiliconLawset laws)
    {
        return string.Join("", laws.Laws.Select(law => law.LawString));
    }
}
