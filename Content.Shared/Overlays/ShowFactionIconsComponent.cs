using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Overlays;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShowFactionIconsComponent : Component
{
    [DataField("factionIcon"), AutoNetworkedField]
    public ProtoId<StatusIconPrototype> FactionIcon { get; set; } = "HostileFaction";

    [DataField("jobIcon"), AutoNetworkedField]
    public ProtoId<JobIconPrototype> JobIcon { get; set; } = "JobIconSoldier";

    [DataField("assignSquad"), AutoNetworkedField]
    public bool AssignSquad { get; set; } = false;

    [DataField("squadIcon"), AutoNetworkedField]
    public ProtoId<JobIconPrototype>? SquadIcon { get; set; }

    [DataField("assignedSquadNameKey"), AutoNetworkedField]
    public string? AssignedSquadNameKey { get; set; }

    [DataField("isSergeantInSquad"), AutoNetworkedField]
    public bool IsSergeantInSquad { get; set; }

    [DataField("belongsToCivFactionId"), AutoNetworkedField]
    public string? BelongsToCivFactionId { get; set; }
}