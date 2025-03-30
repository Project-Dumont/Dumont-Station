using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Random;

namespace Content.Shared.Genetics.Systems;

public sealed class DnaServerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DnaServerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<DnaServerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<DnaServerComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.ServerId = GenerateId();
        Dirty(ent);
    }

    private void OnShutdown(Entity<DnaServerComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.Buffer1 = null;
        ent.Comp.Buffer2 = null;
        ent.Comp.Buffer3 = null;

        Dirty(ent);
    }

    public void RegisterClient(Entity<DnaServerComponent?> server, Entity<DnaClientComponent?> client)
    {
        if (!Resolve(server, ref server.Comp) || !Resolve(client, ref client.Comp))
            return;

        server.Comp.Clients.Add(client);
        client.Comp.ConnectedToServer = true;
        client.Comp.Server = server;

        Dirty(client.Owner, client.Comp);
        Dirty(server.Owner, server.Comp);
    }

    public IEnumerable<Entity<DnaServerComponent>> GetServers()
    {
        var query = EntityQueryEnumerator<DnaServerComponent>();
        while (query.MoveNext(out var uid, out var serverComponent))
        {
            yield return (uid, serverComponent);
        }
    }

    public bool AddToBuffer(Entity<DnaServerComponent?> server, int bufferIndex, EnzymeInfo data)
    {
        if (!Resolve(server, ref server.Comp))
            return false;

        var sampleName = GenerateSampleName();
        EnzymeInfo? buffer = bufferIndex switch
        {
            1 => server.Comp.Buffer1,
            2 => server.Comp.Buffer2,
            3 => server.Comp.Buffer3,
            _ => null
        };

        data.SampleName = sampleName;

        if (buffer == null)
        {
            switch (bufferIndex)
            {
                case 1: server.Comp.Buffer1 = data; break;
                case 2: server.Comp.Buffer2 = data; break;
                case 3: server.Comp.Buffer3 = data; break;
                default: return false;
            }
        }

        Dirty(server.Owner, server.Comp);
        return true;
    }

    public bool AddToBufferDisk(Entity<DnaServerComponent?> server, int bufferIndex, EnzymeInfo data)
    {
        if (!Resolve(server, ref server.Comp))
            return false;

        EnzymeInfo? buffer = bufferIndex switch
        {
            1 => server.Comp.Buffer1,
            2 => server.Comp.Buffer2,
            3 => server.Comp.Buffer3,
            _ => null
        };

        if (buffer == null)
        {
            switch (bufferIndex)
            {
                case 1: server.Comp.Buffer1 = data; break;
                case 2: server.Comp.Buffer2 = data; break;
                case 3: server.Comp.Buffer3 = data; break;
                default: return false;
            }
        }

        Dirty(server.Owner, server.Comp);
        return true;
    }

    public bool ClearBuffer(Entity<DnaServerComponent?> server, int bufferIndex)
    {
        if (!Resolve(server, ref server.Comp))
            return false;

        switch (bufferIndex)
        {
            case 1: server.Comp.Buffer1 = null; break;
            case 2: server.Comp.Buffer2 = null; break;
            case 3: server.Comp.Buffer3 = null; break;
            default: return false;
        }

        Dirty(server.Owner, server.Comp);
        return true;
    }

    public bool RenameBuffer(Entity<DnaServerComponent?> server, int bufferIndex, string name)
    {
        if (!Resolve(server, ref server.Comp))
            return false;

        if (string.IsNullOrWhiteSpace(name))
            return false;

        var buffer = bufferIndex switch
        {
            1 => server.Comp.Buffer1,
            2 => server.Comp.Buffer2,
            3 => server.Comp.Buffer3,
            _ => null
        };

        if (buffer == null)
            return false;

        buffer.SampleName = name;
        Dirty(server, server.Comp);

        return true;
    }

    public bool TryGetBufferData(Entity<DnaServerComponent?> server, int bufferIndex, [NotNullWhen(true)] out EnzymeInfo? data)
    {
        data = null;

        if (!Resolve(server, ref server.Comp))
            return false;

        data = bufferIndex switch
        {
            1 => server.Comp.Buffer1,
            2 => server.Comp.Buffer2,
            3 => server.Comp.Buffer3,
            _ => null
        };

        if (data != null)
            Logger.Debug($"{data.Info}, {data.Identifier}");

        return data != null;
    }

    private int GenerateId()
    {
        return EntityQuery<DnaServerComponent>(true).Max(server => server.ServerId) + 1;
    }

    private string GenerateSampleName()
    {
        var randomNumber = _random.Next(1000, 10000);
        return Loc.GetString("dna-disk-sample") + randomNumber;
    }
}
