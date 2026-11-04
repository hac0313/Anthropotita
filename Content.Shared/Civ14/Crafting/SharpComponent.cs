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

    /// <summary>
    /// Whether this item has been used as a surgery tool.
    /// </summary>
    [DataField]
    public bool HadSurgeryTool;

    /// <summary>
    /// Whether this item has been used as a scalpel.
    /// </summary>
    [DataField]
    public bool HadScalpel;

    /// <summary>
    /// Whether this item has been used as a bone saw.
    /// </summary>
    [DataField]
    public bool HadBoneSaw;
}
