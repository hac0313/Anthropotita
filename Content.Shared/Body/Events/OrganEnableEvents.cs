using Content.Shared.Body;

namespace Content.Shared.Body.Events;

/// <summary>
///     Raised when an organ's enabled state changes.
/// </summary>
[ByRefEvent]
public readonly record struct OrganEnableChangedEvent(bool Enabled);

/// <summary>
///     Raised when an organ is enabled.
/// </summary>
[ByRefEvent]
public readonly record struct OrganEnabledEvent(Entity<OrganComponent> Organ);

/// <summary>
///     Raised when an organ is disabled.
/// </summary>
[ByRefEvent]
public readonly record struct OrganDisabledEvent(Entity<OrganComponent> Organ);
