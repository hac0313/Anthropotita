using Content.Shared._RMC14.CCVar;
using Content.Shared.CombatMode;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Shared._RMC14.Weapons.Ranged.Prediction;

public abstract class SharedGunPredictionSystem : EntitySystem
{
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    public bool GunPrediction { get; private set; }

    public override void Initialize()
    {
        Subs.CVar(_config, RMCCVars.RMCGunPrediction, v => GunPrediction = v, true);
    }

    public List<EntityUid>? ShootRequested(NetEntity netGun, NetCoordinates coordinates, NetEntity? target, ICommonSession session)
    {
        var user = session.AttachedEntity;

        if (user == null ||
            !_combatMode.IsInCombatMode(user.Value) ||
            !_gun.TryGetGun(user.Value, out var ent))
        {
            return null;
        }

        if (ent.Owner != GetEntity(netGun))
            return null;

        var shootCoordinates = GetCoordinates(coordinates);
        var targetUid = GetEntity(target);

#pragma warning disable RA0002
        ent.Comp.ShootCoordinates = shootCoordinates;
        ent.Comp.Target = targetUid;
#pragma warning restore RA0002

        _gun.AttemptShoot(user.Value, ent, shootCoordinates, targetUid);
        return new List<EntityUid>();
    }
}
