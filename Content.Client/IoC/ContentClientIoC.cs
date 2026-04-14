using Content.Client.Humanoid;

namespace Content.Client.IoC;

internal static class ContentClientIoC
{
    internal static void Register(IDependencyCollection collection)
    {
        collection.Register<ShaderMarkingManager>();
    }
}
