using Content.Client.Humanoid;
using Robust.Shared.IoC;

namespace Content.Client.IoC;

internal static class ContentClientIoC
{
    internal static void Register()
    {
        IoCManager.Register<ShaderMarkingManager>();
    }
}
