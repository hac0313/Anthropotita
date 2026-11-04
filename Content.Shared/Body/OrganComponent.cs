using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Body;

/// <summary>
/// Marks an entity as being able to be inserted into an entity with <seealso cref="BodyComponent" />.
/// </summary>
/// <seealso cref="BodySystem" />
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(BodySystem))]
public sealed partial class OrganComponent : Component
{
    /// <summary>
    /// The body entity containing this organ, if any
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Body;

    /// <summary>
    /// What kind of organ is this, if any
    /// </summary>
    [DataField]
    public ProtoId<OrganCategoryPrototype>? Category;

    /// <summary>
    /// Components to add when this organ is inserted into a body.
    /// </summary>
    [DataField]
    public ComponentRegistry? OnAdd;

    /// <summary>
    /// Components to remove when this organ is removed from a body.
    /// </summary>
    [DataField]
    public ComponentRegistry? OnRemove;

    /// <summary>
    /// Whether this organ is currently enabled/functional.
    /// </summary>
    [DataField]
    public bool Enabled = true;

    /// <summary>
    /// The slot ID this organ occupies in the body.
    /// </summary>
    [DataField]
    public string? SlotId;

    /// <summary>
    /// The original body this organ belonged to, if any.
    /// </summary>
    [DataField]
    public EntityUid? OriginalBody;
}
