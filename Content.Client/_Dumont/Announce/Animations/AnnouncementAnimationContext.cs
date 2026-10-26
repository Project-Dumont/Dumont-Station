using Content.Shared._Dumont.Announce;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Client._Dumont.Announce.Animations;

public sealed record AnnouncementAnimationContext(
    ActiveAnnouncement State,
    AnnouncementStyle Style,
    string[] OriginalText,
    string[] CleanText,
    RichTextLabel[] Labels,
    int TitleOffset,
    Func<string, AnnouncementStyle, FormattedMessage> FormatMessage,
    Action SetAllLabels,
    Control? VisualContainer,
    IRobustRandom Random);
