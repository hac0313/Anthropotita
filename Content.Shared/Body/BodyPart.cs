using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared._Shitmed.Targeting;

namespace Content.Shared.Body;

/// <summary>
///     Types of body parts that can exist on a body.
/// </summary>
public enum BodyPartType
{
    Head,
    Torso,
    Arm,
    Hand,
    Leg,
    Foot,
    Tail,
    Other
}

/// <summary>
///     Symmetry of a body part (left/right/none).
/// </summary>
public enum BodyPartSymmetry
{
    None,
    Left,
    Right
}

/// <summary>
///     Component on an entity representing a body part attached to a body.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BodyPartComponent : Component
{
    /// <summary>
    ///     The type of this body part.
    /// </summary>
    [DataField]
    public BodyPartType PartType;

    /// <summary>
    ///     The symmetry of this body part (left, right, or none).
    /// </summary>
    [DataField]
    public BodyPartSymmetry Symmetry;

    /// <summary>
    ///     The body entity this part is attached to, if any.
    /// </summary>
    [DataField]
    public EntityUid? Body;

    /// <summary>
    ///     Whether this part is currently enabled/functional.
    /// </summary>
    [DataField]
    public bool Enabled = true;

    /// <summary>
    ///     The name of the slot this part is attached in.
    /// </summary>
    [DataField]
    public string? Slot;

    /// <summary>
    ///     Components to add when this part is attached to a body.
    /// </summary>
    [DataField]
    public ComponentRegistry? OnAdd;

    /// <summary>
    ///     Components to remove when this part is removed from a body.
    /// </summary>
    [DataField]
    public ComponentRegistry? OnRemove;

    /// <summary>
    ///     Minimum integrity before the part is considered heavily wounded.
    /// </summary>
    [DataField]
    public float MinIntegrity = 30;

    /// <summary>
    ///     Integrity thresholds for different wound states.
    /// </summary>
    [DataField]
    public Dictionary<TargetIntegrity, float> IntegrityThresholds = new()
    {
        [TargetIntegrity.Healthy] = 100,
        [TargetIntegrity.LightlyWounded] = 75,
        [TargetIntegrity.HeavilyWounded] = 50,
        [TargetIntegrity.CriticallyWounded] = 25,
        [TargetIntegrity.Disabled] = 0,
    };

    /// <summary>
    ///     Integrity threshold at which the part can sever.
    /// </summary>
    [DataField]
    public float SeverIntegrity = 0;

    /// <summary>
    ///     Whether this part can be severed.
    /// </summary>
    [DataField]
    public bool CanSever = true;

    /// <summary>
    ///     Amount of self-healing per tick.
    /// </summary>
    [DataField]
    public float SelfHealingAmount = 0;

    /// <summary>
    ///     Time between self-healing ticks.
    /// </summary>
    [DataField]
    public float HealingTime = 1f;

    /// <summary>
    ///     Timer for self-healing.
    /// </summary>
    [DataField]
    public float HealingTimer;

    /// <summary>
    ///     Integrity threshold at which the part becomes enabled.
    /// </summary>
    [DataField]
    public float EnableIntegrity = 0;

    /// <summary>
    ///     The name of the item insertion slot for this part, if any.
    /// </summary>
    [DataField]
    public string? ItemInsertionSlot;

    /// <summary>
    ///     The name of the container for this part, if any.
    /// </summary>
    [DataField]
    public string? ContainerName;
}

