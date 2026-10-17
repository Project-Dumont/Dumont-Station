using System.Linq;
using System.Numerics;
using Content.Client.Lobby;
using Content.Client.Resources;
using Content.Shared._Dumont.Election;
using Content.Shared.Examine;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Roles;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Dumont.Election;

public sealed partial class ElectionScreenOverlay : Overlay
{
    [Dependency] private IClyde _clyde = default!;
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IResourceCache _cache = default!;
    [Dependency] private IUserInterfaceManager _ui = default!;

    private readonly ExamineSystemShared _examine;
    private readonly SharedPowerReceiverSystem _power;
    private readonly SharedTransformSystem _xform;
    private readonly Font _font;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    private const float SightRange = 25f;

    // everything here is in sprite pixels, from the top left of the lit area
    private const float HeaderHeight = 6f;
    private const float RowHeight = 11f;
    private const float FooterHeight = 7f;
    private const float PageSeconds = 6f;
    private const int PortraitFrames = 8;

    // where the head sits on a 32x32 mob drawn at double size
    private static readonly UIBox2 HeadRegion = new(16f, 2f, 48f, 34f);

    // the background comes from the sprite, which is white
    private static readonly Color Ink = Color.FromHex("#14181f");
    private static readonly Color Accent = Color.FromHex("#a3541a");
    private static readonly Color Dim = Color.FromHex("#5a6472");

    private sealed class Portrait
    {
        public EntityUid Dummy;
        public IRenderTexture Target = default!;
        public int Frames;
    }

    private readonly Dictionary<string, Portrait> _portraits = new();
    private ElectionStateEvent? _state;

    // lit area of the screen being drawn right now
    private float _width;
    private float _height;
    private int _rows;

    public ElectionScreenOverlay()
    {
        IoCManager.InjectDependencies(this);

        _examine = _entMan.System<ExamineSystemShared>();
        _power = _entMan.System<SharedPowerReceiverSystem>();
        _xform = _entMan.System<SharedTransformSystem>();
        _font = _cache.GetFont("/Fonts/TinyUnicode.ttf", 16);
    }

    public void SetState(ElectionStateEvent? state)
    {
        _state = state;

        var wanted = state?.Candidates.ToDictionary(Key) ?? new Dictionary<string, ElectionCandidate>();
        foreach (var (key, portrait) in _portraits.ToList())
        {
            if (wanted.ContainsKey(key))
                continue;

            if (_entMan.EntityExists(portrait.Dummy))
                _entMan.DeleteEntity(portrait.Dummy);

            portrait.Target.Dispose();
            _portraits.Remove(key);
        }

        var lobby = _ui.GetUIController<LobbyUIController>();
        foreach (var (key, candidate) in wanted)
        {
            if (_portraits.ContainsKey(key))
                continue;

            JobPrototype? job = null;
            if (candidate.Job != null)
                _proto.TryIndex(candidate.Job, out job);

            _portraits[key] = new Portrait
            {
                Dummy = lobby.LoadProfileEntity(candidate.Profile, job, true),
                Target = _clyde.CreateRenderTarget(new Vector2i(64, 64),
                    new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb),
                    new TextureSampleParameters { Filter = false },
                    "election-portrait"),
            };
        }
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_state == null || args.ViewportControl == null)
            return;

        var handle = args.ScreenHandle;
        RenderPortraits(handle);

        var matrix = args.ViewportControl.GetWorldToScreenMatrix();
        var bounds = args.WorldBounds.Enlarged(4f);

        var query = _entMan.EntityQueryEnumerator<ElectionScreenComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var screen, out var sprite, out var xform))
        {
            if (sprite.BaseRSI is not { } rsi || xform.MapID != args.MapId)
                continue;

            var world = _xform.GetWorldPosition(xform);
            if (!bounds.Contains(world) || !_power.IsPowered(uid) || !CanSee(uid))
                continue;

            var center = Vector2.Transform(world, matrix);
            var unit = (Vector2.Transform(world + Vector2.UnitX, matrix) - center).Length() / EyeManager.PixelsPerMeter;

            var size = (Vector2) rsi.Size;
            var topLeft = new Vector2(-size.X / 2f + sprite.Offset.X * EyeManager.PixelsPerMeter,
                -size.Y / 2f - sprite.Offset.Y * EyeManager.PixelsPerMeter) + screen.ScreenPosition;
            _width = screen.ScreenSize.X;
            _height = screen.ScreenSize.Y;
            _rows = Math.Max(1, (int) ((_height - HeaderHeight - 1f - FooterHeight) / RowHeight));

            // follows the sprite when it fades
            handle.Modulate = Color.White.WithAlpha(sprite.Color.A);
            DrawScreen(handle, center + topLeft * unit, unit, screen.Mode);
            handle.Modulate = Color.White;
        }
    }

    // the doll takes a few frames to finish dressing, so redraw for a bit before throwing it away
    private void RenderPortraits(DrawingHandleScreen handle)
    {
        foreach (var portrait in _portraits.Values)
        {
            if (portrait.Frames >= PortraitFrames)
                continue;

            var dummy = portrait.Dummy;
            if (_entMan.HasComponent<SpriteComponent>(dummy))
            {
                var target = portrait.Target;
                handle.RenderInRenderTarget(target,
                    () => handle.DrawEntity(dummy, target.Size / 2, new Vector2(2f, 2f), Angle.Zero, Angle.Zero, Direction.South),
                    Color.Transparent);
            }

            portrait.Frames++;
            if (portrait.Frames >= PortraitFrames && _entMan.EntityExists(dummy))
                _entMan.DeleteEntity(dummy);
        }
    }

    private bool CanSee(EntityUid screen)
    {
        if (_player.LocalEntity is not { } local)
            return true;

        if (_entMan.TryGetComponent<EyeComponent>(local, out var eye) && !eye.DrawFov)
            return true;

        return _examine.InRangeUnOccluded(local, screen, SightRange);
    }

    private void DrawScreen(DrawingHandleScreen handle, Vector2 origin, float unit, ElectionScreenMode mode)
    {
        var state = _state!;
        var scale = unit / 2f;

        if (state.Phase == ElectionPhase.Standby)
        {
            var left = state.StartsAt - _timing.CurTime;
            if (left < TimeSpan.Zero)
                left = TimeSpan.Zero;

            Text(handle, origin, unit, scale, Loc.GetString("election-screen-title"), _width / 2f, _height / 2f - 6f, Accent, 0.5f);
            Text(handle, origin, unit, scale, Loc.GetString("election-screen-standby"), _width / 2f, _height / 2f + 3f, Ink, 0.5f);
            Text(handle, origin, unit, scale,
                Loc.GetString("election-screen-standby-timer", ("time", $"{(int) left.TotalMinutes:00}:{left.Seconds:00}")),
                _width / 2f, _height / 2f + 10f, Dim, 0.5f);
            return;
        }

        if (state.Phase == ElectionPhase.Elected && state.Candidates.Count > 0)
        {
            DrawElected(handle, origin, unit, scale, state.Candidates);
            return;
        }

        if (state.Phase == ElectionPhase.SecondRound)
        {
            var runoff = state.Candidates.Where(c => c.Outcome == ElectionOutcome.SecondRound).ToList();
            if (runoff.Count >= 2)
            {
                DrawVersus(handle, origin, unit, scale, runoff[0], runoff[1]);
                return;
            }
        }

        var sections = Loc.GetString("election-screen-sections", ("percent", Percent(state.Sections)));
        var sectionsWidth = handle.GetDimensions(_font, sections, scale).X / unit;
        Text(handle, origin, unit, scale, sections, _width - 1f, 4.5f, Ink, 1f);
        Text(handle, origin, unit, scale, Loc.GetString("election-screen-title"), 1f, 4.5f, Accent,
            maxWidth: _width - sectionsWidth - 4f);
        handle.DrawRect(Box(origin, unit, 0f, HeaderHeight, _width, 0.5f), Accent);

        var candidates = state.Candidates;
        var top = mode == ElectionScreenMode.Top3;
        var rows = top ? Math.Min(3, _rows) : _rows;
        var pages = Math.Max(1, (candidates.Count + rows - 1) / rows);
        var page = top ? 0 : (int) (_timing.RealTime.TotalSeconds / PageSeconds) % pages;

        // the top 3 gets the whole screen, so its rows stretch to fill it
        var rowHeight = top ? MathF.Floor((_height - HeaderHeight - 1f - FooterHeight) / rows) : RowHeight;

        for (var i = 0; i < rows; i++)
        {
            var index = page * rows + i;
            if (index >= candidates.Count)
                break;

            DrawRow(handle, origin, unit, scale, candidates[index], HeaderHeight + 1f + i * rowHeight, rowHeight);
        }

        var status = state.Phase switch
        {
            ElectionPhase.Waiting => Loc.GetString("election-screen-waiting"),
            ElectionPhase.SecondRound => Loc.GetString("election-screen-status-second-round"),
            ElectionPhase.Elected => Loc.GetString("election-screen-status-elected"),
            _ => Loc.GetString("election-screen-updated", ("time", state.LastUpdate)),
        };
        var statusColor = state.Phase is ElectionPhase.SecondRound or ElectionPhase.Elected ? Accent : Dim;

        if (top)
        {
            var others = candidates.Skip(rows).Sum(c => c.Percent);
            Text(handle, origin, unit, scale, Loc.GetString("election-screen-others", ("percent", Percent(others))), 1f, _height - 2f, Dim);
            Text(handle, origin, unit, scale, status, _width - 1f, _height - 2f, statusColor, 1f);
            return;
        }

        Text(handle, origin, unit, scale, status, 1f, _height - 2f, statusColor);
        Text(handle, origin, unit, scale,
            Loc.GetString("election-screen-page", ("page", page + 1), ("pages", pages)),
            _width - 1f, _height - 2f, Dim, 1f);
    }

    private void DrawElected(DrawingHandleScreen handle, Vector2 origin, float unit, float scale, List<ElectionCandidate> candidates)
    {
        var winner = candidates.Find(c => c.Outcome == ElectionOutcome.Elected) ?? candidates[0];
        var color = PartyColor(winner.Party);
        var body = Math.Min(_height - HeaderHeight - FooterHeight - 4f, _width / 2f);
        var textLeft = body + 5f;
        var textWidth = _width - textLeft - 2f;
        var top = HeaderHeight + 2f + (body - 34f) / 2f;

        Text(handle, origin, unit, scale, Loc.GetString("election-screen-final-elected"), _width / 2f, 4.5f, Accent, 0.5f);
        handle.DrawRect(Box(origin, unit, 0f, HeaderHeight, _width, 0.5f), Accent);

        DrawBody(handle, origin, unit, winner, 2f, HeaderHeight + 2f, body);
        Text(handle, origin, unit, scale, winner.Name, textLeft, top + 5f, Ink, maxWidth: textWidth);
        Text(handle, origin, unit, scale, winner.Party, textLeft, top + 10f, Dim, maxWidth: textWidth);
        Text(handle, origin, unit, scale * 2f, Percent(winner.Percent) + "%", textLeft, top + 21f, Accent);
        handle.DrawRect(Box(origin, unit, textLeft, top + 23f, textWidth, 2f), color.WithAlpha(0.15f));
        handle.DrawRect(Box(origin, unit, textLeft, top + 23f, textWidth * Math.Clamp(winner.Percent / 100f, 0f, 1f), 2f), color);
        Text(handle, origin, unit, scale, Loc.GetString("election-screen-votes", ("votes", Thousands(winner.Votes))), textLeft, top + 30f, Dim);

        handle.DrawRect(Box(origin, unit, 0f, _height - FooterHeight - 1f, _width, 0.5f), Dim);
        var others = candidates.Where(c => c != winner).Take(2).ToList();
        for (var i = 0; i < others.Count; i++)
        {
            var text = Loc.GetString("election-screen-rank",
                ("rank", i + 2),
                ("name", others[i].Name),
                ("percent", Percent(others[i].Percent)));
            Text(handle, origin, unit, scale, text, i == 0 ? 1f : _width - 1f, _height - 2f, Dim, i, _width / 2f - 2f);
        }
    }

    private void DrawVersus(DrawingHandleScreen handle, Vector2 origin, float unit, float scale, ElectionCandidate first, ElectionCandidate second)
    {
        Text(handle, origin, unit, scale, Loc.GetString("election-screen-final-second-round"), _width / 2f, 4.5f, Accent, 0.5f);
        handle.DrawRect(Box(origin, unit, 0f, HeaderHeight, _width, 0.5f), Accent);
        var size = Math.Min(_height - HeaderHeight - 18f, _width / 2f - 8f);
        Text(handle, origin, unit, scale * 2f, "X", _width / 2f, HeaderHeight + 1f + size / 2f + 3f, Accent, 0.5f);

        DrawContender(handle, origin, unit, scale, first, _width * 0.25f, size);
        DrawContender(handle, origin, unit, scale, second, _width * 0.75f, size);
    }

    private void DrawContender(DrawingHandleScreen handle, Vector2 origin, float unit, float scale, ElectionCandidate candidate, float x, float size)
    {
        var maxWidth = _width / 2f - 2f;
        var bottom = HeaderHeight + 1f + size;

        DrawBody(handle, origin, unit, candidate, x - size / 2f, HeaderHeight + 1f, size);
        Text(handle, origin, unit, scale, candidate.Name, x, bottom + 4f, Ink, 0.5f, maxWidth);
        Text(handle, origin, unit, scale, ShortParty(candidate.Party), x, bottom + 9f, PartyColor(candidate.Party), 0.5f, maxWidth);
        Text(handle, origin, unit, scale, Percent(candidate.Percent) + "%", x, bottom + 14.5f, Ink, 0.5f, maxWidth);
    }

    private void DrawBody(DrawingHandleScreen handle, Vector2 origin, float unit, ElectionCandidate candidate, float x, float y, float size)
    {
        if (_portraits.TryGetValue(Key(candidate), out var portrait))
            handle.DrawTextureRect(portrait.Target.Texture, Box(origin, unit, x, y, size, size));
    }

    private void DrawRow(DrawingHandleScreen handle, Vector2 origin, float unit, float scale, ElectionCandidate candidate, float y, float height)
    {
        var color = PartyColor(candidate.Party);
        var portraitSize = height - 1f;
        var textLeft = portraitSize + 3f;
        var first = y + portraitSize / 2f - 1f;
        var second = y + height - 2f;
        var barWidth = _width - textLeft - 1f;

        handle.DrawRect(Box(origin, unit, textLeft - 1f, y, barWidth + 1f, portraitSize), color.WithAlpha(0.15f));
        handle.DrawRect(Box(origin, unit, textLeft - 1f, y, (barWidth + 1f) * Math.Clamp(candidate.Percent / 100f, 0f, 1f), portraitSize),
            color.WithAlpha(0.45f));

        if (_portraits.TryGetValue(Key(candidate), out var portrait))
        {
            handle.DrawTextureRectRegion(portrait.Target.Texture,
                Box(origin, unit, 1f, y, portraitSize, portraitSize),
                HeadRegion);
        }

        var won = candidate.Outcome != ElectionOutcome.None;
        var percent = Percent(candidate.Percent) + "%";
        var percentWidth = handle.GetDimensions(_font, percent, scale).X / unit;
        Text(handle, origin, unit, scale, percent, _width - 2f, first, Ink, 1f);
        Text(handle, origin, unit, scale, candidate.Name, textLeft, first, won ? Accent : Ink,
            maxWidth: barWidth - percentWidth - 3f);

        var tag = candidate.Outcome switch
        {
            ElectionOutcome.Elected => Loc.GetString("election-screen-elected"),
            ElectionOutcome.SecondRound => Loc.GetString("election-screen-second-round"),
            _ => null,
        };
        var votes = ShortVotes(candidate.Votes);
        var party = ShortParty(candidate.Party);
        var votesWidth = handle.GetDimensions(_font, votes, scale).X / unit;
        Text(handle, origin, unit, scale, votes, _width - 2f, second, Dim, 1f);
        Text(handle, origin, unit, scale, tag == null ? party : $"{tag} - {party}", textLeft, second,
            won ? Accent : Dim,
            maxWidth: barWidth - votesWidth - 3f);
    }

    // anchor: 0 starts at x, 0.5 centers, 1 ends at x
    private void Text(DrawingHandleScreen handle, Vector2 origin, float unit, float scale, string text, float x, float baseline,
        Color color, float anchor = 0f, float maxWidth = float.MaxValue)
    {
        maxWidth = Math.Min(maxWidth, _width);
        var width = handle.GetDimensions(_font, text, scale).X;
        while (width > maxWidth * unit && text.Length > 1)
        {
            text = text[..^1];
            width = handle.GetDimensions(_font, text, scale).X;
        }

        var pos = origin + new Vector2(x * unit - width * anchor, baseline * unit - _font.GetAscent(scale));
        handle.DrawString(_font, pos, text, scale, color);
    }

    private static UIBox2 Box(Vector2 origin, float unit, float x, float y, float width, float height)
    {
        return UIBox2.FromDimensions(origin + new Vector2(x, y) * unit, new Vector2(width, height) * unit);
    }

    private static string Key(ElectionCandidate candidate)
    {
        return $"{candidate.Name}|{candidate.Job}";
    }

    // has to give the same hue on every client
    private static Color PartyColor(string party)
    {
        var hash = 2166136261u;
        foreach (var c in party)
        {
            hash = (hash ^ c) * 16777619u;
        }

        return Color.FromHsl(new Vector4(hash % 360u / 360f, 0.7f, 0.5f, 1f));
    }

    private static string Percent(float value)
    {
        var hundredths = (int) MathF.Round(value * 100f);
        return $"{hundredths / 100},{hundredths % 100:00}";
    }

    private static string ShortParty(string party)
    {
        return party.StartsWith("Partido ") ? "P. " + party["Partido ".Length..] : party;
    }

    private static string ShortVotes(long votes)
    {
        if (votes >= 1_000_000)
            return $"{votes / 1_000_000},{votes % 1_000_000 / 100_000} mi";

        if (votes >= 1_000)
            return $"{votes / 1_000},{votes % 1_000 / 100} mil";

        return votes.ToString();
    }

    private static string Thousands(long value)
    {
        var text = value.ToString();
        for (var i = text.Length - 3; i > 0; i -= 3)
        {
            text = text.Insert(i, ".");
        }

        return text;
    }
}
