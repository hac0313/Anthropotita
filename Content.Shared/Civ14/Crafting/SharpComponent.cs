using Robust.Shared.Serialization;

namespace Content.Shared.Crafting;

[RegisterComponent]
public sealed partial class SharpComponent : Component
{
    /// <summary>
    /// Whether this item is sharp enough to be used for sharpening/cutting tasks.
    /// </summary>
    [DataField]
    public bool Sharp = true;
}
