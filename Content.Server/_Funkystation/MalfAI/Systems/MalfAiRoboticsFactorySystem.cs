// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Funkystation.Factory.Systems;
using Robust.Shared.Prototypes;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared._Funkystation.Actions.Events;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Robotics Factory action by requesting the AIBuild system to spawn a RoboticsFactoryGrid.
/// The server knows exactly what prototype to spawn (RoboticsFactoryGrid) for security.
/// </summary>
public sealed partial class MalfAiRoboticsFactorySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    private static readonly ISawmill Sawmill = Logger.GetSawmill("malf.ai.factory");

    private static readonly EntProtoId RoboticsFactoryPrototype = "RoboticsFactoryGrid";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfunctioningAiComponent, MalfAiRoboticsFactoryActionEvent>(OnRoboticsFactory);
    }

    private void OnRoboticsFactory(Entity<MalfunctioningAiComponent> malf, ref MalfAiRoboticsFactoryActionEvent args)
    {
        if (args.Handled)
            return;

        // Check if target coordinates are valid
        if (!args.Target.IsValid(EntityManager))
            return;

        var refundAction = MetaData(args.Action).EntityPrototype?.ID;
        var ev = new AIBuildRequestEvent(malf.Owner, args.Target, RoboticsFactoryPrototype, refundAction);
        RaiseLocalEvent(ev);

        args.Handled = true; // consume the action
    }

}
