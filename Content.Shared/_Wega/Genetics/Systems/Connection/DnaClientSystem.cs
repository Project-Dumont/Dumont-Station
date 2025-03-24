using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Serialization;

namespace Content.Shared.Genetics.Systems;

public sealed class DnaClientSystem : EntitySystem
{
    [Dependency] private readonly DnaServerSystem _dnaServer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DnaClientComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<DnaClientComponent> ent, ref ComponentInit args)
    {
        foreach (var server in _dnaServer.GetServers())
        {
            _dnaServer.RegisterClient((server, server.Comp), (ent, ent.Comp));
        }
    }

    public bool TryGetBufferData(Entity<DnaClientComponent?> client, int bufferIndex, [NotNullWhen(true)] out EnzymeInfo? data)
    {
        data = null;

        if (!TryGetServer(client, out var server))
            return false;

        return _dnaServer.TryGetBufferData((server.Value.Owner, server.Value.Comp), bufferIndex, out data);
    }

    public bool TryAddToBuffer(Entity<DnaClientComponent?> client, int bufferIndex, EnzymeInfo data)
    {
        if (!TryGetServer(client, out var server))
            return false;

        return _dnaServer.AddToBuffer((server.Value.Owner, server.Value.Comp), bufferIndex, data);
    }

    public bool TryClearBuffer(Entity<DnaClientComponent?> client, int bufferIndex)
    {
        if (!TryGetServer(client, out var server))
            return false;

        return _dnaServer.ClearBuffer((server.Value.Owner, server.Value.Comp), bufferIndex);
    }

    public bool TryRenameBuffer(Entity<DnaClientComponent?> client, int bufferIndex, string name)
    {
        if (!TryGetServer(client, out var server))
            return false;

        return _dnaServer.RenameBuffer((server.Value.Owner, server.Value.Comp), bufferIndex, name);
    }

    public bool TryGetServer(Entity<DnaClientComponent?> client, [NotNullWhen(true)] out Entity<DnaServerComponent>? serverEnt)
    {
        serverEnt = null;

        if (!Resolve(client, ref client.Comp))
            return false;

        if (!client.Comp.ConnectedToServer)
            return false;

        if (!TryComp<DnaServerComponent>(client.Comp.Server!.Value, out var serverComponent))
            return false;

        serverEnt = (client.Comp.Server!.Value, serverComponent);
        return true;
    }
}
