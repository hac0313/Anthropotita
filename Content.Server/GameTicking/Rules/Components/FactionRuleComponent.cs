using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Network;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
///     Gamerule that enables the faction system.
/// </summary>
[RegisterComponent, Access(typeof(FactionRuleSystem))]
public sealed partial class FactionRuleComponent : Component
{
    /// <summary>
    ///     The list of factions available for players to join.
    /// </summary>
    [DataField]
    public List<ProtoId<StatusIconPrototype>> Factions = new();
}