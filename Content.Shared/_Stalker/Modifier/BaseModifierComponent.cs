namespace Content.Shared._Stalker.Modifier;

public abstract partial class BaseModifierComponent<T> : Component where T : class
{
    [DataField, ViewVariables]
    public T Modifier { get; set; }
}


