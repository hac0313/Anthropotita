namespace Content.Shared._Stalker.Modifier;

public abstract partial class BaseFloatModifierComponent : Component
{
    [DataField, ViewVariables]
    public float Modifier { get; set; }
}


