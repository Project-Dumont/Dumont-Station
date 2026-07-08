// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared.Mutatrix.GreyMatter.Events;

// These event types are intentionally kept as harmless compatibility stubs.
// The action prototypes were removed from greymatter.yml, so they are not granted.
public sealed partial class MutatrixGreyMatterDisableApcActionEvent : EntityTargetActionEvent
{
}

public sealed partial class MutatrixGreyMatterHackAccessActionEvent : WorldTargetActionEvent
{
}

public sealed partial class MutatrixGreyMatterAnalyzeSystemActionEvent : EntityTargetActionEvent
{
}
