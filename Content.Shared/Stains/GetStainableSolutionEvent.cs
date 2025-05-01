using Content.Shared.Chemistry.Components;

namespace Content.Shared.Stains;

public sealed partial class GetStainableSolutionEvent : HandledEntityEventArgs
{
    public EntityUid Stained;
    public Solution? Solution;

    public GetStainableSolutionEvent(EntityUid stained)
    {
        Stained = stained;
    }
}
