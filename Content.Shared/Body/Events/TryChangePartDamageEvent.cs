using Content.Shared.Body;
using Content.Shared.Damage;
using Content.Shared._Shitmed.Targeting;

namespace Content.Shared.Body.Events;

/// <summary>
///     Raised when damage is about to be applied to a body part, allowing the target part to be modified.
/// </summary>
[ByRefEvent]
public record struct TryChangePartDamageEvent
{
    public DamageSpecifier? Damage;
    public TargetBodyPart? TargetPart;
    public EntityUid? Origin;
    public bool IgnoreResistances;
    public float ArmorPenetration;
    public bool CanSever;
    public bool CanEvade;
    public float PartMultiplier;
    public bool Evaded;
}
