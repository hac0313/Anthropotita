using Content.Shared.Body;

namespace Content.Shared.Body.Events;

/// <summary>
///     Raised on a body entity when a body part is added to it.
/// </summary>
[ByRefEvent]
public readonly record struct BodyPartAddedEvent(Entity<BodyPartComponent> Part);

/// <summary>
///     Raised on a body entity when a body part is removed from it.
/// </summary>
[ByRefEvent]
public readonly record struct BodyPartRemovedEvent(Entity<BodyPartComponent> Part);
