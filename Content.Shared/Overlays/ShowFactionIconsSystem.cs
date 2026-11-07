using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Shared.Overlays;

public abstract class SharedFactionIconsSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    protected static readonly Dictionary<string, SquadConfig> Squads = new()
    {
        { "Alpha", new SquadConfig("JobIconSquad1", "JobIconSquad1", 20) },
        { "Bravo", new SquadConfig("JobIconSquad2", "JobIconSquad2", 20) },
        { "Charlie", new SquadConfig("JobIconSquad3", "JobIconSquad3", 20) },
    };
    protected record SquadConfig(string MemberIconId, string SergeantIconId, int MaxSize);

    public virtual bool TryAssignToSquad(EntityUid uid, string squadName, bool assignAsSergeant, ShowFactionIconsComponent? component = null)
    {
        if (!Resolve(uid, ref component, logMissing: false))
            return false;

        if (!Squads.TryGetValue(squadName, out var config))
            return false;

        int currentOccupantsInSquad = 0;
        bool sergeantRoleFilledInSquad = false;
        EntityUid? existingSergeantUid = null;
        ShowFactionIconsComponent? oldSergeantComp = null;

        var query = AllEntityQuery<ShowFactionIconsComponent>();
        while (query.MoveNext(out var otherUid, out var otherComp))
        {
            if (otherComp.AssignSquad && otherComp.AssignedSquadNameKey == squadName)
            {
                currentOccupantsInSquad++;
                if (otherComp.IsSergeantInSquad)
                {
                    sergeantRoleFilledInSquad = true;
                    existingSergeantUid = otherUid;
                }
            }
        }

        bool entityIsAlreadyInThisSquad = component.AssignSquad && component.AssignedSquadNameKey == squadName;
        bool entityIsAlreadySergeantOfThisSquad = entityIsAlreadyInThisSquad && component.IsSergeantInSquad;

        if (assignAsSergeant)
        {
            if (!entityIsAlreadyInThisSquad && currentOccupantsInSquad >= config.MaxSize)
                return false;
        }
        else
        {
            if (entityIsAlreadySergeantOfThisSquad)
            {
            }
            else if (entityIsAlreadyInThisSquad && !component.IsSergeantInSquad)
            {
            }
            else
            {
                if (currentOccupantsInSquad >= config.MaxSize)
                    return false;
            }
        }

        component.AssignSquad = true;
        component.AssignedSquadNameKey = squadName;
        component.IsSergeantInSquad = assignAsSergeant;
        component.SquadIcon = assignAsSergeant ? config.SergeantIconId : config.MemberIconId;
        Dirty(uid, component);

        if (assignAsSergeant && sergeantRoleFilledInSquad && existingSergeantUid.HasValue && existingSergeantUid.Value != uid)
        {
            if (Resolve(existingSergeantUid.Value, ref oldSergeantComp, logMissing: false))
            {
                if (oldSergeantComp.AssignSquad && oldSergeantComp.AssignedSquadNameKey == squadName && oldSergeantComp.IsSergeantInSquad)
                {
                    oldSergeantComp.IsSergeantInSquad = false;
                    oldSergeantComp.SquadIcon = config.MemberIconId;
                    Dirty(existingSergeantUid.Value, oldSergeantComp);
                }
            }
        }

        return true;
    }

    public virtual void RemoveFromSquad(EntityUid uid, ShowFactionIconsComponent? component = null)
    {
        if (!Resolve(uid, ref component, logMissing: false))
            return;

        component.AssignSquad = false;
        component.SquadIcon = null;
        component.AssignedSquadNameKey = null;
        component.IsSergeantInSquad = false;
        Dirty(uid, component);
    }
}