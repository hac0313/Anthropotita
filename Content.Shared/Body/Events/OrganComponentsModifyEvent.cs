using Content.Shared.Body;

namespace Content.Shared.Body.Events;

/// <summary>
///     Raised when components are added to or removed from an organ in a body.
/// </summary>
[ByRefEvent]
public readonly record struct OrganComponentsModifyEvent(EntityUid Body, EntityUid Organ, bool Add);
