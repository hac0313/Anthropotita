using Content.Shared.Body;

namespace Content.Shared.Body.Events;

/// <summary>
///     Raised when an organ is added to a body.
/// </summary>
[ByRefEvent]
public readonly record struct OrganAddedToBodyEvent(EntityUid Body);

/// <summary>
///     Raised when an organ is removed from a body.
/// </summary>
[ByRefEvent]
public readonly record struct OrganRemovedFromBodyEvent(EntityUid OldBody);
