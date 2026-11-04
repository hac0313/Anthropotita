using Robust.Shared.Prototypes;

namespace Content.Shared.Body;

/// <summary>
///     Marker prototype that defines well-known types of bodies, e.g. "human" or "silicon".
/// </summary>
[Prototype]
public sealed partial class BodyPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
}
