using Content.Shared.Overlays;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Overlays;

public sealed class ShowFactionIconsSystem : EquipmentHudSystem<ShowFactionIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShowFactionIconsComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, ShowFactionIconsComponent component, ref GetStatusIconsEvent ev)
    {
        if (!IsActive)
            return;

        if (_prototype.TryIndex<StatusIconPrototype>(component.FactionIcon, out var iconPrototype))
            ev.StatusIcons.Add(iconPrototype);

        if (component.AssignSquad && component.SquadIcon != default)
        {
            if (_prototype.TryIndex<JobIconPrototype>(component.SquadIcon, out var squadIconPrototype))
                ev.StatusIcons.Add(squadIconPrototype);
        }

        if (component.JobIcon != default && component.JobIcon != "JobIconSoldier" && component.JobIcon != "JobIconRifleman" && component.JobIcon != "JobIconMg")
        {
            if (_prototype.TryIndex<JobIconPrototype>(component.JobIcon, out var jobIconPrototype))
                ev.StatusIcons.Add(jobIconPrototype);
        }
    }
}