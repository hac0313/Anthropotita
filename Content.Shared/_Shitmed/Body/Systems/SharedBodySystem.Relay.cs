// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared.Body;
using Content.Shared.Body.Part;
using Content.Shared.Body.Events;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Prototypes;
using Content.Shared.Standing;
using JetBrains.Annotations;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly Content.Shared.Body.BodySystem _bodySys = default!;

    public IEnumerable<EntityUid> GetBodyChildren(EntityUid uid, BodyComponent? body = null)
    {
        if (!Resolve(uid, ref body))
            yield break;

        if (body.Organs != null)
        {
            foreach (var organ in body.Organs.ContainedEntities)
                yield return organ;
        }
    }

    public IEnumerable<EntityUid> GetBodyChildrenOfType(EntityUid uid, BodyPartType type, BodyPartSymmetry? symmetry = null, BodyComponent? body = null)
    {
        foreach (var child in GetBodyChildren(uid, body))
        {
            if (TryComp(child, out BodyPartComponent? part) &&
                part.PartType == type &&
                (symmetry == null || part.Symmetry == symmetry))
                yield return child;
        }
    }

    public TargetBodyPart? GetTargetBodyPart(Entity<BodyPartComponent> part) =>
        GetTargetBodyPart(part.Comp.PartType, part.Comp.Symmetry);

    public TargetBodyPart? GetTargetBodyPart(BodyPartComponent part) =>
        GetTargetBodyPart(part.PartType, part.Symmetry);

    public TargetBodyPart? GetTargetBodyPart(BodyPartType type, BodyPartSymmetry symmetry)
    {
        return (type, symmetry) switch
        {
            (BodyPartType.Head, _) => TargetBodyPart.Head,
            (BodyPartType.Torso, _) => TargetBodyPart.Torso,
            (BodyPartType.Arm, BodyPartSymmetry.Left) => TargetBodyPart.LeftArm,
            (BodyPartType.Arm, BodyPartSymmetry.Right) => TargetBodyPart.RightArm,
            (BodyPartType.Hand, BodyPartSymmetry.Left) => TargetBodyPart.LeftHand,
            (BodyPartType.Hand, BodyPartSymmetry.Right) => TargetBodyPart.RightHand,
            (BodyPartType.Leg, BodyPartSymmetry.Left) => TargetBodyPart.LeftLeg,
            (BodyPartType.Leg, BodyPartSymmetry.Right) => TargetBodyPart.RightLeg,
            (BodyPartType.Foot, BodyPartSymmetry.Left) => TargetBodyPart.LeftFoot,
            (BodyPartType.Foot, BodyPartSymmetry.Right) => TargetBodyPart.RightFoot,
            _ => null
        };
    }

    public DamageSpecifier GetHealingSpecifier(BodyPartComponent part)
    {
        var damage = new DamageSpecifier()
        {
            DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>()
        };
        return damage;
    }

    public bool CanAttachToSlot(EntityUid part, string connection) => true;

    public bool TryCreatePartSlot(EntityUid part, string slotName, BodyPartType partType, out EntityUid slot)
    {
        slot = EntityUid.Invalid;
        return false;
    }

    public void AttachPart(EntityUid part, string slotName, EntityUid tool) { }

    public bool CanInsertOrgan(EntityUid part, string organSlot) => true;

    public bool TryGetBodyPartOrgans(EntityUid part, Type organType, out List<EntityUid> organs)
    {
        organs = new List<EntityUid>();
        return false;
    }

    public bool InsertOrgan(EntityUid part, EntityUid tool, string slotId, Entity<BodyPartComponent> partComp, OrganComponent organ) => false;

    public bool TrySetOrganUsed(EntityUid tool, bool used, OrganComponent organ) => false;

    public void RemoveOrgan(EntityUid organId, OrganComponent organ) { }

    public IEnumerable<(EntityUid, OrganComponent)> GetPartOrgans(EntityUid part)
    {
        yield break;
    }

    public void ModifyMarkings(EntityUid uid, Entity<BodyPartAppearanceComponent> partAppearance,
        HumanoidVisualLayers targetLayer, string markingId, bool remove = false) { }

    public bool TryCreateOrganSlot(EntityUid part, string organSlot, out object slot)
    {
        slot = null!;
        return false;
    }

    public void RelayEvent<T>(Entity<BodyComponent> body, ref T args) where T : struct { }

    public void RelayEvent<T>(Entity<BodyComponent> body, T args) where T : class { }

    [PublicAPI]
    public ComponentRegistry? GetOrganOnAdd(EntityUid organId) => _bodySys.GetOrganOnAdd(organId);

    [PublicAPI]
    public void SetOrganOnAdd(EntityUid organId, ComponentRegistry? registry) => _bodySys.SetOrganOnAdd(organId, registry);

    [PublicAPI]
    public void AddOrganOnAddComponent(EntityUid organId, string key, EntityPrototype.ComponentRegistryEntry value) => _bodySys.AddOrganOnAddComponent(organId, key, value);

    [PublicAPI]
    public void RemoveOrganOnAddComponent(EntityUid organId, string key) => _bodySys.RemoveOrganOnAddComponent(organId, key);

    [PublicAPI]
    public bool OrganOnAddContainsKey(EntityUid organId, string key) => _bodySys.OrganOnAddContainsKey(organId, key);

    /// <summary>
    /// Gets body part status dictionary for an entity.
    /// </summary>
    [PublicAPI]
    public Dictionary<TargetBodyPart, TargetIntegrity> GetBodyPartStatus(EntityUid uid)
    {
        return new Dictionary<TargetBodyPart, TargetIntegrity>();
    }

    /// <summary>
    /// Tries to get body organ entity components of type T.
    /// </summary>
    [PublicAPI]
    public bool TryGetBodyOrganEntityComps<T>(EntityUid uid, out T? comp) where T : IComponent
    {
        comp = default;
        if (_bodySys.TryGetBodyOrganEntityComps(uid, out comp))
            return true;
        return false;
    }
}
