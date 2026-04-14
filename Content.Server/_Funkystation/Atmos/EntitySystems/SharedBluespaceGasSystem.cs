using Content.Shared._Funkystation.CCVars;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Content.Shared._Funkystation.Atmos.Components;
using Content.Server._Funkystation.Atmos.Components;
using Content.Server._Funkystation.Atmos.EntitySystems;

namespace Content.Server._Funkystation.Atmos.Systems
{
    public sealed class SharedBluespaceGasSystem : EntitySystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly SharedDeviceLinkSystem DeviceLink = default!;

        private bool _bluespaceGasEnabled;

        private ProtoId<SourcePortPrototype> SourcePort = "BluespaceSender";
        private ProtoId<SinkPortPrototype> SinkPort = "BluespaceGasUtilizer";

        public override void Initialize()
        {
            base.Initialize();

            _cfg.OnValueChanged(CCVars_Funky.BluespaceGasEnabled, enabled => _bluespaceGasEnabled = enabled, true);

            SubscribeLocalEvent<BluespaceSenderComponent, NewLinkEvent>(OnNewLink);
            SubscribeLocalEvent<BluespaceGasUtilizerComponent, PortDisconnectedEvent>(OnPortDisconnected);
        }

        private void OnPortDisconnected(Entity<BluespaceGasUtilizerComponent> ent, ref PortDisconnectedEvent args)
        {
            if (args.Port != SinkPort)
                return;

            if (!TryComp(ent, out BluespaceVendorComponent? vendor))
                return;

            vendor.BluespaceGasMixture = new();
            vendor.BluespaceSenderConnected = false;
            ent.Comp.BluespaceSender = null;
            Dirty(ent);
            EntityManager.System<BluespaceVendorSystem>().OnBluespaceSenderConnected(ent, vendor);
        }

        private void OnNewLink(Entity<BluespaceSenderComponent> ent, ref NewLinkEvent args)
        {
            if (args.SinkPort != SinkPort || args.SourcePort != SourcePort)
                return;

            if (!TryComp(args.Sink, out BluespaceGasUtilizerComponent? utilizer))
                return;

            if (utilizer.BluespaceSender != null)
                DeviceLink.RemoveSinkFromSource(utilizer.BluespaceSender.Value, args.Sink);

            if (!TryComp(args.Sink, out BluespaceVendorComponent? vendor))
                return;

            vendor.BluespaceGasMixture = ent.Comp.BluespaceGasMixture;
            vendor.BluespaceSenderConnected = true;

            utilizer.BluespaceSender = ent;
            Dirty(args.Sink, utilizer);
            EntityManager.System<BluespaceVendorSystem>().OnBluespaceSenderConnected(args.Sink, vendor);
        }
    }
}
