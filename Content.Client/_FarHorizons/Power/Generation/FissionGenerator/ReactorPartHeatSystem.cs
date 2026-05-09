// SPDX-FileCopyrightText: 2026 jhrushbe <capnmerry@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared._Trauma.Nuclear.Reactor;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Power.Generation.FissionGenerator;

public sealed partial class ReactorPartHeatSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SpriteSystem _sprite = default!;

    private static readonly ProtoId<ShaderPrototype> HeatShader = "HeatDistortionFH";
    private const string ShaderKey = "reactor-part-heat";
    private ShaderInstance _heatShader = default!;

    public override void Initialize()
    {
        base.Initialize();

        _heatShader = _proto.Index(HeatShader).InstanceUnique();

        SubscribeLocalEvent<ReactorPartComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<ReactorPartComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        // other stuff can have post shaders too so only touch ours
        if (args.AppearanceData.TryGetValue(ReactorPartVisuals.HeatDistort, out var value) && value is true)
            _sprite.SetPostShader(args.Sprite, new SpriteComponent.PostShaderArgs(ShaderKey, _heatShader));
        else
            _sprite.RemovePostShader(args.Sprite, ShaderKey);
    }
}
