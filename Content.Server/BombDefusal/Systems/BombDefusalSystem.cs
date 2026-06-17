using System.Linq;
using Content.Server.BombDefusal.Components;
using Content.Server.BombDefusal.Modules;
using Content.Server.Defusable.Components;
using Content.Server.Defusable.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Administration.Logs;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Content.Shared.Construction.Components;
using Content.Shared.Database;
using Content.Shared.Defusable;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Trigger.Components;
using Content.Shared.Trigger.Systems;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.BombDefusal.Systems;

/// <summary>
/// Server system for KTANE-style bomb defusal.
/// Handles module generation, interaction validation, strike tracking, and defusal/detonation.
/// </summary>
public sealed class BombDefusalSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const string SerialChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BombDefusalComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BombDefusalComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<BombDefusalComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<BombDefusalComponent, AnchorAttemptEvent>(OnAnchorAttempt);
        SubscribeLocalEvent<BombDefusalComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);
        SubscribeLocalEvent<BombDefusalComponent, BombModuleInteractionMessage>(OnModuleInteraction);
        SubscribeLocalEvent<BombDefusalComponent, BoundUIClosedEvent>(OnUiClosed);
    }

    /// <summary>
    /// Generate serial number on map init. Modules are generated when the bomb is armed.
    /// </summary>
    private void OnMapInit(EntityUid uid, BombDefusalComponent comp, MapInitEvent args)
    {
        comp.SerialNumber = GenerateSerialNumber();
    }

    /// <summary>
    /// Generate modules when the bomb is armed (so module count can be based on timer).
    /// </summary>
    public void GenerateModules(EntityUid uid, BombDefusalComponent comp)
    {
        if (comp.ModulesGenerated)
            return;

        var moduleCount = comp.ModuleCountOverride ?? GetModuleCountFromTimer(uid);
        moduleCount = Math.Max(1, moduleCount); // At least 1 module

        var availableTypes = Enum.GetValues<BombModuleType>();

        for (var i = 0; i < moduleCount; i++)
        {
            var moduleType = _random.Pick(availableTypes);
            var module = GenerateModule(moduleType, comp.SerialNumber, comp.Modules);
            comp.Modules.Add(module);
        }

        comp.ModulesGenerated = true;

        GenerateRuleSet(uid, comp);
    }

    private BombModule GenerateModule(BombModuleType type, string serialNumber, List<BombModule> existingModules)
    {
        switch (type)
        {
            case BombModuleType.Wires:
                var wires = WiresModule.Generate(_random, serialNumber);
                var prevWires = existingModules.OfType<WiresModule>().FirstOrDefault();
                if (prevWires != null)
                {
                    wires.Rules3 = prevWires.Rules3;
                    wires.Rules4 = prevWires.Rules4;
                    wires.Rules5 = prevWires.Rules5;
                    wires.Rules6 = prevWires.Rules6;
                    wires.CorrectWireIndex = wires.EvaluateWiresRules(wires.GetRulesForCount(wires.WireColors.Count), wires.WireColors, serialNumber);
                }
                return wires;

            case BombModuleType.Symbols:
                var symbols = SymbolsModule.Generate(_random);
                var prevSymbols = existingModules.OfType<SymbolsModule>().FirstOrDefault();
                if (prevSymbols != null)
                {
                    symbols.ModuleSymbolColumns = prevSymbols.ModuleSymbolColumns;
                    var colIdx = _random.Next(symbols.ModuleSymbolColumns.Length);
                    var col = symbols.ModuleSymbolColumns[colIdx];
                    var indices = Enumerable.Range(0, col.Length).ToList();
                    _random.Shuffle(indices);
                    var chosen = indices.Take(4).ToList();
                    symbols.DisplayedSymbols = chosen.Select(pos => col[pos]).ToList();
                    _random.Shuffle(symbols.DisplayedSymbols);
                    chosen.Sort();
                    var correct = chosen.Select(pos => col[pos]).ToList();
                    symbols.CorrectOrder = correct.Select(sym => symbols.DisplayedSymbols.IndexOf(sym)).ToList();
                }
                return symbols;

            case BombModuleType.SimonSays:
                var simon = SimonSaysModule.Generate(_random, serialNumber);
                var prevSimon = existingModules.OfType<SimonSaysModule>().FirstOrDefault();
                if (prevSimon != null)
                {
                    simon.VowelMappings = prevSimon.VowelMappings;
                    simon.NoVowelMappings = prevSimon.NoVowelMappings;
                }
                return simon;

            case BombModuleType.Codewords:
                var codewords = CodewordsModule.Generate(_random);
                var prevCodewords = existingModules.OfType<CodewordsModule>().FirstOrDefault();
                if (prevCodewords != null)
                {
                    codewords.ModuleWordColumns = prevCodewords.ModuleWordColumns;
                    var key = _random.Pick(codewords.ModuleWordColumns.Keys.ToList());
                    var col = codewords.ModuleWordColumns[key];
                    var targetWords = col.ToList();
                    _random.Shuffle(targetWords);
                    var fromTarget = _random.Next(2, 4);
                    var selected = targetWords.Take(fromTarget).ToList();
                    var correct = col.First(w => selected.Contains(w));

                    var other = new List<string>();
                    foreach (var kvp in codewords.ModuleWordColumns)
                    {
                        if (kvp.Key == key) continue;
                        foreach (var w in kvp.Value)
                        {
                            if (!col.Contains(w) && !selected.Contains(w))
                                other.Add(w);
                        }
                    }
                    other = other.Distinct().ToList();
                    _random.Shuffle(other);
                    var filler = other.Take(6 - fromTarget).ToList();
                    var displayed = new List<string>();
                    displayed.AddRange(selected);
                    displayed.AddRange(filler);
                    _random.Shuffle(displayed);
                    codewords.DisplayedWords = displayed;
                    codewords.CorrectWordIndex = displayed.IndexOf(correct);
                }
                return codewords;

            case BombModuleType.Maze:
                var maze = MazeModule.Generate(_random, serialNumber);
                var prevMaze = existingModules.OfType<MazeModule>().FirstOrDefault();
                if (prevMaze != null)
                {
                    maze.Walls = prevMaze.Walls;
                    maze.PlayerX = _random.Next(0, 6);
                    maze.PlayerY = _random.Next(0, 6);
                    maze.CurrentX = maze.PlayerX;
                    maze.CurrentY = maze.PlayerY;
                    do
                    {
                        maze.GoalX = _random.Next(0, 6);
                        maze.GoalY = _random.Next(0, 6);
                    } while (maze.GoalX == maze.PlayerX && maze.GoalY == maze.PlayerY);
                    maze.PathDirections = MazeModule.FindPath(maze);
                }
                return maze;

            case BombModuleType.Memory:
                var memory = MemoryModule.Generate(_random);
                var prevMemory = existingModules.OfType<MemoryModule>().FirstOrDefault();
                if (prevMemory != null)
                {
                    memory.StageRules = prevMemory.StageRules;
                }
                return memory;

            case BombModuleType.Password:
                var password = PasswordModule.Generate(_random);
                var prevPassword = existingModules.OfType<PasswordModule>().FirstOrDefault();
                if (prevPassword != null)
                {
                    password.PoolWords = prevPassword.PoolWords;
                    password.TargetWord = _random.Pick(password.PoolWords);

                    password.Columns.Clear();
                    for (int col = 0; col < 5; col++)
                    {
                        var targetChar = password.TargetWord[col];
                        var colLetters = new HashSet<char> { targetChar };
                        while (colLetters.Count < 6)
                        {
                            var randChar = (char) ('A' + _random.Next(0, 26));
                            colLetters.Add(randChar);
                        }
                        var colList = colLetters.ToList();
                        _random.Shuffle(colList);
                        password.Columns.Add(colList);
                        password.SelectedIndices[col] = _random.Next(0, 6);
                    }
                }
                return password;

            case BombModuleType.MorseCode:
                return MorseCodeModule.Generate(_random);

            case BombModuleType.WhosOnFirst:
                var wof = WhosOnFirstModule.Generate(_random);
                var prevWof = existingModules.OfType<WhosOnFirstModule>().FirstOrDefault();
                if (prevWof != null)
                {
                    wof.DisplayToPositionMap = prevWof.DisplayToPositionMap;
                    wof.WordPriorityLists = prevWof.WordPriorityLists;
                }
                return wof;

            default:
                return WiresModule.Generate(_random, serialNumber);
        }
    }
    /// <summary>
    /// TODO: need new symbols or symbol renderer for the module, seems like the game doesn't support most symbols
    /// </summary>
    private static readonly string[] SymbolGlyphs =
    {
        "Ω", "Ψ", "Ξ", "Φ", "Σ",
        "Δ", "Π", "Θ", "Λ", "Γ",
        "ℌ", "℘", "ℜ", "ℑ", "ℵ",
        "♠", "♣", "♦", "♥", "★",
        "☆", "◆", "◇", "▲", "▼",
    };

    public void GenerateRuleSet(EntityUid uid, BombDefusalComponent comp)
    {
        if (comp.RuleSet != null)
            return;

        if (!comp.ModulesGenerated)
            GenerateModules(uid, comp);

        var ruleSet = new BombRuleSet
        {
            SerialNumber = comp.SerialNumber,
            ModuleCount = comp.Modules.Count
        };

        var hasVowel = comp.SerialNumber.Any(c => "AEIOUaeiou".Contains(c));
        var lastDigitOdd = comp.SerialNumber.Length > 0 &&
                           char.IsDigit(comp.SerialNumber[^1]) &&
                           (comp.SerialNumber[^1] - '0') % 2 != 0;

        var modulesByType = comp.Modules.GroupBy(m => m.Type);

        foreach (var group in modulesByType)
        {
            var type = group.Key;
            var rules = new BombModuleRules();
            var firstModule = group.First();

            switch (firstModule)
            {
                case WiresModule wires:
                    rules.ModuleName = Loc.GetString("bomb-defusal-module-wires");
                    GenerateWiresRules(wires, rules);
                    break;
                case SymbolsModule symbols:
                    rules.ModuleName = Loc.GetString("bomb-defusal-module-symbols");
                    GenerateSymbolsRules(symbols, rules);
                    break;
                case SimonSaysModule simon:
                    rules.ModuleName = Loc.GetString("bomb-defusal-module-simonsays");
                    GenerateSimonSaysRules(simon, rules);
                    break;
                case CodewordsModule codewords:
                    rules.ModuleName = Loc.GetString("bomb-defusal-module-codewords");
                    GenerateCodewordsRules(codewords, rules);
                    break;
                case MazeModule:
                    rules.ModuleName = Loc.GetString("bomb-defusal-module-maze");
                    rules.RuleLines.Add("[color=yellow]MAZE NAVIGATION[/color]");
                    rules.RuleLines.Add("Navigate the player (white circle) to the goal (red triangle).");
                    rules.RuleLines.Add("Do not hit walls! The defuser does not see the walls.");
                    rules.RuleLines.Add("");

                    var mazeIndex = 1;
                    foreach (var m in group.OfType<MazeModule>())
                    {
                        rules.RuleLines.Add($"[bold]Maze Module #{mazeIndex}:[/bold]");
                        rules.RuleLines.Add($"  Start: ({m.PlayerX + 1}, {m.PlayerY + 1}) | Goal: ({m.GoalX + 1}, {m.GoalY + 1})");
                        rules.RuleLines.Add($"  Path: {string.Join(" -> ", m.PathDirections)}");
                        rules.RuleLines.Add("");
                        mazeIndex++;
                    }
                    break;
                case MemoryModule memory:
                    rules.ModuleName = Loc.GetString("bomb-defusal-module-memory");
                    GenerateMemoryRules(memory, rules);
                    break;
                case PasswordModule password:
                    rules.ModuleName = Loc.GetString("bomb-defusal-module-password");
                    GeneratePasswordRules(password, rules);
                    break;
                case MorseCodeModule morse:
                    rules.ModuleName = Loc.GetString("bomb-defusal-module-morsecode");
                    GenerateMorseRules(morse, rules);
                    break;
                case WhosOnFirstModule wof:
                    rules.ModuleName = Loc.GetString("bomb-defusal-module-whosonfirst");
                    GenerateWhosOnFirstRules(wof, rules);
                    break;
            }

            ruleSet.ModuleRules[type] = rules;
        }

        comp.RuleSet = ruleSet;
    }
    /// <summary>
    /// This is VERY shit, do not do it like this, I can't figure out a better way to do this, help!!!
    /// TODO: Better Rules Generator
    /// </summary>
    /// <param name="wires"></param>
    /// <param name="rules"></param>
    private void GenerateWiresRules(WiresModule wires, BombModuleRules rules)
    {
        rules.RuleLines.Add("[color=yellow]WIRES RULE SHEET[/color]");
        rules.RuleLines.Add("Verify the number of wires on the module and follow the rules below:");
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]If there are 3 wires:[/bold]");
        foreach (var r in wires.Rules3)
        {
            rules.RuleLines.Add($"- {r.RuleText}");
        }
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]If there are 4 wires:[/bold]");
        foreach (var r in wires.Rules4)
        {
            rules.RuleLines.Add($"- {r.RuleText}");
        }
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]If there are 5 wires:[/bold]");
        foreach (var r in wires.Rules5)
        {
            rules.RuleLines.Add($"- {r.RuleText}");
        }
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]If there are 6 wires:[/bold]");
        foreach (var r in wires.Rules6)
        {
            rules.RuleLines.Add($"- {r.RuleText}");
        }
    }

    private void GenerateSymbolsRules(SymbolsModule symbols, BombModuleRules rules)
    {
        rules.RuleLines.Add("[color=yellow]SYMBOL KEYPADS[/color]");
        rules.RuleLines.Add("Only one column below will contain all four symbols displayed on the module.");
        rules.RuleLines.Add("Press the buttons in order from top to bottom of that column.");
        rules.RuleLines.Add("");
        for (int i = 0; i < symbols.ModuleSymbolColumns.Length; i++)
        {
            var col = symbols.ModuleSymbolColumns[i];
            var colStr = string.Join("  ", col.Select(id => SymbolGlyphs[id]));
            rules.RuleLines.Add($"[bold]Col {i + 1}:[/bold]  [mono]{colStr}[/mono]");
        }
    }

    private void GenerateSimonSaysRules(SimonSaysModule simon, BombModuleRules rules)
    {
        rules.RuleLines.Add("[color=yellow]SIMON SAYS RULES[/color]");
        rules.RuleLines.Add("A light flashes in a sequence. Press the mapped colors below.");
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]If the serial number has a Vowel:[/bold]");
        rules.RuleLines.Add("- No strikes:");
        foreach (var kvp in simon.VowelMappings[0])
            rules.RuleLines.Add($"  {kvp.Key.ToString().ToUpper()} -> {kvp.Value.ToString().ToUpper()}");
        rules.RuleLines.Add("- 1 strike:");
        foreach (var kvp in simon.VowelMappings[1])
            rules.RuleLines.Add($"  {kvp.Key.ToString().ToUpper()} -> {kvp.Value.ToString().ToUpper()}");
        rules.RuleLines.Add("- 2+ strikes:");
        foreach (var kvp in simon.VowelMappings[2])
            rules.RuleLines.Add($"  {kvp.Key.ToString().ToUpper()} -> {kvp.Value.ToString().ToUpper()}");

        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]If the serial number has NO Vowel:[/bold]");
        rules.RuleLines.Add("- No strikes:");
        foreach (var kvp in simon.NoVowelMappings[0])
            rules.RuleLines.Add($"  {kvp.Key.ToString().ToUpper()} -> {kvp.Value.ToString().ToUpper()}");
        rules.RuleLines.Add("- 1 strike:");
        foreach (var kvp in simon.NoVowelMappings[1])
            rules.RuleLines.Add($"  {kvp.Key.ToString().ToUpper()} -> {kvp.Value.ToString().ToUpper()}");
        rules.RuleLines.Add("- 2+ strikes:");
        foreach (var kvp in simon.NoVowelMappings[2])
            rules.RuleLines.Add($"  {kvp.Key.ToString().ToUpper()} -> {kvp.Value.ToString().ToUpper()}");
    }

    private void GenerateCodewordsRules(CodewordsModule codewords, BombModuleRules rules)
    {
        rules.RuleLines.Add("[color=yellow]CODEWORDS LOOKUP[/color]");
        rules.RuleLines.Add("Find a column category where [bold]two or more[/bold] of the displayed words appear.");
        rules.RuleLines.Add("Press the first word in that column category that is displayed on the module.");
        rules.RuleLines.Add("");
        foreach (var kvp in codewords.ModuleWordColumns)
        {
            var listStr = string.Join(", ", kvp.Value);
            rules.RuleLines.Add($"[bold]Category {kvp.Key}:[/bold] {listStr}");
        }
    }

    private void GenerateMazeRules(MazeModule maze, BombModuleRules rules)
    {
        rules.RuleLines.Add("[color=yellow]MAZE NAVIGATION[/color]");
        rules.RuleLines.Add("Navigate the player (white circle) to the goal (red triangle).");
        rules.RuleLines.Add("Do not hit walls! The defuser does not see the walls.");
        rules.RuleLines.Add("");
        rules.RuleLines.Add($"[bold]Player Start:[/bold] ({maze.PlayerX + 1}, {maze.PlayerY + 1})");
        rules.RuleLines.Add($"[bold]Goal Position:[/bold] ({maze.GoalX + 1}, {maze.GoalY + 1})");
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]Correct Path:[/bold]");
        rules.RuleLines.Add(string.Join(" -> ", maze.PathDirections));
    }

    private void GenerateMemoryRules(MemoryModule memory, BombModuleRules rules)
    {
        rules.RuleLines.Add("[color=yellow]MEMORY MODULE RULES[/color]");
        rules.RuleLines.Add("Follow the rules for the current stage. Buttons are ordered 1 to 4 from left to right.");
        rules.RuleLines.Add("");
        for (int stage = 0; stage < 5; stage++)
        {
            rules.RuleLines.Add($"[bold]Stage {stage + 1}:[/bold]");
            var stageRule = memory.StageRules[stage];
            for (int display = 1; display <= 4; display++)
            {
                var type = stageRule.Types[display - 1];
                var val = stageRule.Values[display - 1];
                var actionStr = type switch
                {
                    MemoryRuleType.Position => $"press the button in [bold]position {val + 1}[/bold].",
                    MemoryRuleType.Label => $"press the button labeled [bold]{val}[/bold].",
                    MemoryRuleType.SamePositionAsStage => $"press the button in the [bold]same position[/bold] as stage {val + 1}.",
                    MemoryRuleType.SameLabelAsStage => $"press the button with the [bold]same label[/bold] as stage {val + 1}.",
                    _ => "press any button."
                };
                rules.RuleLines.Add($"  - If display is [bold]{display}[/bold]: {actionStr}");
            }
        }
    }

    private void GeneratePasswordRules(PasswordModule password, BombModuleRules rules)
    {
        rules.RuleLines.Add("[color=yellow]PASSWORD LOOKUP[/color]");
        rules.RuleLines.Add("Cycle the columns on the module. Identify which word from the list below can be formed.");
        rules.RuleLines.Add("");
        foreach (var word in password.PoolWords)
        {
            rules.RuleLines.Add($"- {word}");
        }
    }

    private void GenerateMorseRules(MorseCodeModule morse, BombModuleRules rules)
    {
        rules.RuleLines.Add("[color=yellow]MORSE CODE FREQUENCIES[/color]");
        rules.RuleLines.Add("Decode the flashing light pattern and transmit on the correct frequency.");
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]Word -> Frequency Table:[/bold]");
        foreach (var kvp in MorseCodeModule.WordFrequencies.OrderBy(k => k.Key))
        {
            rules.RuleLines.Add($"- {kvp.Key} -> {kvp.Value:F3} MHz");
        }
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]Morse Translation Guide:[/bold]");
        rules.RuleLines.Add("[mono]A: .-   B: -...  C: -.-.  D: -..   E: .     F: ..-.[/mono]");
        rules.RuleLines.Add("[mono]G: --.  H: ....  I: ..   J: .---  K: -.-   L: .-..[/mono]");
        rules.RuleLines.Add("[mono]M: --   N: -.   O: ---  P: .--.  Q: --.-  R: .-.[/mono]");
        rules.RuleLines.Add("[mono]S: ...  T: -    U: ..-  V: ...-  W: .--   X: -..-[/mono]");
        rules.RuleLines.Add("[mono]Y: -.-- Z: --..[/mono]");
    }

    private static string GetPositionName(int pos)
    {
        return pos switch
        {
            0 => "TOP-LEFT",
            1 => "TOP-RIGHT",
            2 => "MIDDLE-LEFT",
            3 => "MIDDLE-RIGHT",
            4 => "BOTTOM-LEFT",
            5 => "BOTTOM-RIGHT",
            _ => "TOP-LEFT"
        };
    }

    private void GenerateWhosOnFirstRules(WhosOnFirstModule wof, BombModuleRules rules)
    {
        rules.RuleLines.Add("[color=yellow]WHO'S ON FIRST LOOKUP[/color]");
        rules.RuleLines.Add("[bold]Step 1:[/bold] Look at the display word and find its position below.");
        rules.RuleLines.Add("Look at the label of the button in that position.");
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]Step 1 Mappings:[/bold]");
        foreach (var word in WhosOnFirstModule.WordPool.OrderBy(w => w))
        {
            var pos = wof.DisplayToPositionMap[word];
            rules.RuleLines.Add($"- \"{word}\" -> check {GetPositionName(pos)}");
        }
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]Step 2:[/bold] Look up the word label in the priority list below.");
        rules.RuleLines.Add("Press the first word in that list that appears on any button on the module.");
        rules.RuleLines.Add("");
        rules.RuleLines.Add("[bold]Step 2 Priority Lists:[/bold]");
        foreach (var word in WhosOnFirstModule.WordPool.OrderBy(w => w))
        {
            var list = wof.WordPriorityLists[word];
            rules.RuleLines.Add($"- [bold]{word}:[/bold] {string.Join(", ", list)}");
        }
    }

    /// <summary>
    /// Calculate module count from timer: 1 module per minute.
    /// </summary>
    private int GetModuleCountFromTimer(EntityUid uid)
    {
        if (!TryComp<TimerTriggerComponent>(uid, out var timer))
            return 3; // fallback

        var seconds = timer.Delay.TotalSeconds;
        return (int) Math.Round(seconds / 60.0);
    }

    private string GenerateSerialNumber()
    {
        var length = 6;
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = SerialChars[_random.Next(SerialChars.Length)];
        }
        return new string(chars);
    }

    #region Event Handlers

    private void OnExamine(EntityUid uid, BombDefusalComponent comp, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(BombDefusalComponent)))
        {
            if (comp.IsDefused)
            {
                args.PushMarkup(Loc.GetString("bomb-defusal-examine-defused", ("name", uid)));
            }
            else if (HasComp<ActiveTimerTriggerComponent>(uid))
            {
                var remaining = _trigger.GetRemainingTime(uid);
                if (remaining != null)
                {
                    args.PushMarkup(Loc.GetString("bomb-defusal-examine-active", ("name", uid),
                        ("time", Math.Floor(remaining.Value.TotalSeconds))));
                }
                else
                {
                    args.PushMarkup(Loc.GetString("bomb-defusal-examine-active-no-time", ("name", uid)));
                }

                args.PushMarkup(Loc.GetString("bomb-defusal-examine-strikes", ("current", comp.Strikes), ("max", comp.MaxStrikes)));
                args.PushMarkup(Loc.GetString("bomb-defusal-examine-modules",
                    ("solved", comp.Modules.Count(m => m.IsSolved)),
                    ("total", comp.Modules.Count)));
            }
            else
            {
                args.PushMarkup(Loc.GetString("bomb-defusal-examine-inactive", ("name", uid)));
            }
        }
    }

    private void OnGetAltVerbs(EntityUid uid, BombDefusalComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || args.Hands == null)
            return;

        // Only show "Begin countdown" if not already armed and not defused
        if (HasComp<ActiveTimerTriggerComponent>(uid) || comp.IsDefused)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("defusable-verb-begin"),
            Priority = 10,
            Act = () =>
            {
                TryStartCountdown(uid, args.User, comp);
            }
        });
    }

    private void OnAnchorAttempt(EntityUid uid, BombDefusalComponent comp, AnchorAttemptEvent args)
    {
        // Bolted when armed
        if (HasComp<ActiveTimerTriggerComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("defusable-popup-cant-anchor", ("name", uid)), uid, args.User);
            args.Cancel();
        }
    }

    private void OnUnanchorAttempt(EntityUid uid, BombDefusalComponent comp, UnanchorAttemptEvent args)
    {
        if (HasComp<ActiveTimerTriggerComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("defusable-popup-cant-anchor", ("name", uid)), uid, args.User);
            args.Cancel();
        }
    }

    private void OnModuleInteraction(EntityUid uid, BombDefusalComponent comp, BombModuleInteractionMessage args)
    {
        if (comp.IsDefused || !HasComp<ActiveTimerTriggerComponent>(uid))
            return;

        if (args.ModuleIndex < 0 || args.ModuleIndex >= comp.Modules.Count)
            return;

        var module = comp.Modules[args.ModuleIndex];

        if (module.IsSolved)
            return;

        bool success;

        // Simon Says needs strike count context
        if (module is SimonSaysModule simon)
        {
            success = simon.ValidateActionWithStrikes(args.Action, comp.Strikes);
        }
        else
        {
            success = module.ValidateAction(args.Action);
        }

        if (success)
        {
            if (module.IsSolved)
            {
                _audio.PlayPvs(comp.SolveSound, uid);

                _adminLogger.Add(LogType.Explosion, LogImpact.Medium,
                    $"{ToPrettyString(args.Actor):user} solved module {args.ModuleIndex} ({module.Type}) on {ToPrettyString(uid):entity}");

                // Check if all modules are solved
                if (comp.Modules.All(m => m.IsSolved))
                {
                    DefuseBomb(uid, args.Actor, comp);
                }
            }
        }
        else
        {
            AddStrike(uid, args.Actor, comp);
        }

        UpdateUiState(uid, comp);
    }

    private void OnUiClosed(EntityUid uid, BombDefusalComponent comp, BoundUIClosedEvent args)
    {
        // Closing the UI while the bomb is active and not defused = 1 strike
        if (comp.IsDefused || !HasComp<ActiveTimerTriggerComponent>(uid))
            return;

        // Make sure this is the defusal UI, not some other UI
        if (args.UiKey is not BombDefusalUiKey)
            return;

        _popup.PopupEntity(Loc.GetString("bomb-defusal-popup-strike-exit", ("name", uid)), uid, args.Actor, PopupType.MediumCaution);
        AddStrike(uid, args.Actor, comp);
        UpdateUiState(uid, comp);
    }

    #endregion

    #region Public API

    public void TryStartCountdown(EntityUid uid, EntityUid user, BombDefusalComponent comp)
    {
        if (comp.IsDefused)
        {
            _popup.PopupEntity(Loc.GetString("bomb-defusal-popup-already-defused", ("name", uid)), uid);
            return;
        }

        var xform = Transform(uid);
        if (!xform.Anchored)
            _transform.AnchorEntity(uid, xform);

        // Generate modules based on timer
        GenerateModules(uid, comp);

        // Start the timer
        if (TryComp<TimerTriggerComponent>(uid, out var timerTrigger))
        {
            _trigger.ActivateTimerTrigger((uid, timerTrigger));
        }

        _popup.PopupEntity(Loc.GetString("defusable-popup-begun", ("name", uid)), uid);

        _appearance.SetData(uid, DefusableVisuals.Active, true);

        _adminLogger.Add(LogType.Explosion, LogImpact.High,
            $"{ToPrettyString(user):user} armed bomb {ToPrettyString(uid):entity} with {comp.Modules.Count} modules");

        UpdateUiState(uid, comp);
    }

    public void AddStrike(EntityUid uid, EntityUid? user, BombDefusalComponent comp)
    {
        comp.Strikes++;

        _audio.PlayPvs(comp.StrikeSound, uid);

        var userStr = user != null ? ToPrettyString(user.Value) : "unknown";
        _adminLogger.Add(LogType.Explosion, LogImpact.Medium,
            $"Strike {comp.Strikes}/{comp.MaxStrikes} on {ToPrettyString(uid):entity} by {userStr}");

        if (comp.Strikes >= comp.MaxStrikes)
        {
            DetonateBomb(uid, user, comp);
        }
    }

    public void DefuseBomb(EntityUid uid, EntityUid? user, BombDefusalComponent comp)
    {
        comp.IsDefused = true;

        // Stop the timer
        RemComp<ActiveTimerTriggerComponent>(uid);

        // Unanchor
        var xform = Transform(uid);
        if (xform.Anchored)
            _transform.Unanchor(uid, xform);

        _audio.PlayPvs(comp.DefuseSound, uid);
        _popup.PopupEntity(Loc.GetString("bomb-defusal-popup-defused", ("name", uid)), uid);

        _appearance.SetData(uid, DefusableVisuals.Active, false);

        if (user != null)
        {
            _adminLogger.Add(LogType.Explosion, LogImpact.High,
                $"{ToPrettyString(user.Value):user} defused bomb {ToPrettyString(uid):entity}!");
        }

        UpdateUiState(uid, comp);
    }

    public void DetonateBomb(EntityUid uid, EntityUid? user, BombDefusalComponent comp)
    {
        _popup.PopupEntity(Loc.GetString("bomb-defusal-popup-detonated", ("name", uid)), uid, PopupType.LargeCaution);

        if (user != null)
        {
            _adminLogger.Add(LogType.Explosion, LogImpact.Extreme,
                $"Bomb {ToPrettyString(uid):entity} detonated (3 strikes) by {ToPrettyString(user.Value):user}");
        }

        _explosion.TriggerExplosive(uid, user: user);
        QueueDel(uid);
    }

    #endregion

    #region UI

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Periodically update UI for active bombs (timer display)
        var query = EntityQueryEnumerator<BombDefusalComponent, ActiveTimerTriggerComponent>();
        while (query.MoveNext(out var uid, out var comp, out _))
        {
            if (comp.IsDefused)
                continue;

            UpdateUiState(uid, comp);
        }
    }

    private void UpdateUiState(EntityUid uid, BombDefusalComponent comp)
    {
        if (!_ui.HasUi(uid, BombDefusalUiKey.Key))
            return;

        var moduleStates = new List<BombDefusalModuleState>();
        foreach (var module in comp.Modules)
        {
            moduleStates.Add(module.GetVisibleState());
        }

        var remaining = _trigger.GetRemainingTime(uid);

        var state = new BombDefusalUiState
        {
            Modules = moduleStates,
            Strikes = comp.Strikes,
            MaxStrikes = comp.MaxStrikes,
            SerialNumber = comp.SerialNumber,
            RemainingTime = remaining != null ? (float) remaining.Value.TotalSeconds : 0f,
            IsActive = HasComp<ActiveTimerTriggerComponent>(uid) && !comp.IsDefused,
        };

        _ui.SetUiState(uid, BombDefusalUiKey.Key, state);
    }

    #endregion
}
