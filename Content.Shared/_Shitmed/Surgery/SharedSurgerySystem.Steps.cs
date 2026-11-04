// SPDX-FileCopyrightText: 2024 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Janet Blackquill <uhhadd@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body;
using Content.Shared.Body.Events;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Damage.Components;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared._Shitmed.BodyEffects;
using Content.Shared.Buckle.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared._Shitmed.Medical.Surgery.Conditions;
using Content.Shared._Shitmed.Medical.Surgery.Effects.Step;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
using Content.Shared._Shitmed.Medical.Surgery.Tools;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Shared._Shitmed.Medical.Surgery;

public abstract partial class SharedSurgerySystem
{
    private static readonly string[] BruteDamageTypes = { "Slash", "Blunt", "Piercing" };
    private static readonly string[] BurnDamageTypes = { "Heat", "Shock", "Cold", "Caustic" };

    private void InitializeSteps()
    {
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryStepEvent>(OnToolStep);
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryStepCompleteCheckEvent>(OnToolCheck);
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryCanPerformStepEvent>(OnToolCanPerform);

        SubSurgery<SurgeryTendWoundsEffectComponent>(OnTendWoundsStep, OnTendWoundsCheck);
        SubSurgery<SurgeryStepCavityEffectComponent>(OnCavityStep, OnCavityCheck);
        SubSurgery<SurgeryAddPartStepComponent>(OnAddPartStep, OnAddPartCheck);
        SubSurgery<SurgeryAffixPartStepComponent>(OnAffixPartStep, OnAffixPartCheck);
        SubSurgery<SurgeryRemovePartStepComponent>(OnRemovePartStep, OnRemovePartCheck);
        SubSurgery<SurgeryAddOrganStepComponent>(OnAddOrganStep, OnAddOrganCheck);
        SubSurgery<SurgeryRemoveOrganStepComponent>(OnRemoveOrganStep, OnRemoveOrganCheck);
        SubSurgery<SurgeryAffixOrganStepComponent>(OnAffixOrganStep, OnAffixOrganCheck);
        SubSurgery<SurgeryAddMarkingStepComponent>(OnAddMarkingStep, OnAddMarkingCheck);
        SubSurgery<SurgeryRemoveMarkingStepComponent>(OnRemoveMarkingStep, OnRemoveMarkingCheck);
        SubSurgery<SurgeryAddOrganSlotStepComponent>(OnAddOrganSlotStep, OnAddOrganSlotCheck);
        Subs.BuiEvents<SurgeryTargetComponent>(SurgeryUIKey.Key, subs =>
        {
            subs.Event<SurgeryStepChosenBuiMsg>(OnSurgeryTargetStepChosen);
        });
    }

    private void SubSurgery<TComp>(EntityEventRefHandler<TComp, SurgeryStepEvent> onStep,
        EntityEventRefHandler<TComp, SurgeryStepCompleteCheckEvent> onComplete) where TComp : IComponent
    {
        SubscribeLocalEvent(onStep);
        SubscribeLocalEvent(onComplete);
    }

    #region Event Methods
    private void OnToolStep(Entity<SurgeryStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryToolAudio(ent, args))
            return;

        AddOrRemoveComponentsToEntity(args.Part, ent.Comp.Add);
        AddOrRemoveComponentsToEntity(args.Part, ent.Comp.Remove, true);
        AddOrRemoveComponentsToEntity(args.Body, ent.Comp.BodyAdd);
        AddOrRemoveComponentsToEntity(args.Body, ent.Comp.BodyRemove, true);

        HandleOrganModification(args.Part, args.Body, ent.Comp.AddOrganOnAdd);
        HandleOrganModification(args.Part, args.Body, ent.Comp.RemoveOrganOnAdd, false);

        HandleSanitization(args);
    }

    private void OnToolCheck(Entity<SurgeryStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (TryToolCheck(ent.Comp.Add, args.Part) ||
            TryToolCheck(ent.Comp.Remove, args.Part, checkMissing: false) ||
            TryToolCheck(ent.Comp.BodyAdd, args.Body) ||
            TryToolCheck(ent.Comp.BodyRemove, args.Body, checkMissing: false))
        {
            args.Cancelled = true;
            return;
        }

        if (TryToolOrganCheck(ent.Comp.AddOrganOnAdd, args.Part)
            || TryToolOrganCheck(ent.Comp.RemoveOrganOnAdd, args.Part, checkMissing: false))
            args.Cancelled = true;
    }

    private void OnToolCanPerform(Entity<SurgeryStepComponent> ent, ref SurgeryCanPerformStepEvent args)
    {
        if (HasComp<SurgeryOperatingTableConditionComponent>(ent))
        {
            if (!TryComp(args.Body, out BuckleComponent? buckle) ||
                !HasComp<OperatingTableComponent>(buckle.BuckledTo))
            {
                args.Invalid = StepInvalidReason.NeedsOperatingTable;
                return;
            }
        }

        if (_inventory.TryGetContainerSlotEnumerator(args.Body, out var containerSlotEnumerator, args.TargetSlots))
        {
            if (HasComp<SurgeryIgnoreClothingComponent>(args.User))
                return;

            while (containerSlotEnumerator.MoveNext(out var containerSlot))
            {
                if (!containerSlot.ContainedEntity.HasValue)
                    continue;

                args.Invalid = StepInvalidReason.Armor;
                args.Popup = Loc.GetString("surgery-ui-window-steps-error-armor");
                return;
            }
        }

        RaiseLocalEvent(args.Body, ref args);

        if (args.Invalid != StepInvalidReason.None)
            return;

        if (ent.Comp.Tool != null)
        {
            args.ValidTools ??= new Dictionary<EntityUid, float>();

            foreach (var reg in ent.Comp.Tool.Values)
            {
                if (!AnyHaveComp(args.Tools, reg.Component, out var tool, out var speed))
                {
                    args.Invalid = StepInvalidReason.MissingTool;

                    if (reg.Component is ISurgeryToolComponent required)
                        args.Popup = $"You need {required.ToolName} to perform this step!";

                    return;
                }

                args.ValidTools[tool] = speed;
            }
        }
    }

    private void OnTendWoundsStep(Entity<SurgeryTendWoundsEffectComponent> ent, ref SurgeryStepEvent args)
    {
        var group = ent.Comp.MainGroup == "Brute" ? BruteDamageTypes : BurnDamageTypes;

        if (!HasDamageGroup(args.Body, group, out var damageable)
            && !HasDamageGroup(args.Part, group, out var _)
            || damageable == null)
            return;

        var allDamage = _damageable.GetAllDamage((args.Body, damageable));
        var bonus = FixedPoint2.Zero;
        foreach (var type in group)
            if (allDamage.DamageDict.TryGetValue(type, out var v))
                bonus += v;
        bonus *= ent.Comp.HealMultiplier;
        if (_mobState.IsDead(args.Body))
            bonus *= 0.2;

        var adjustedDamage = new DamageSpecifier(allDamage);

        foreach (var type in group)
            adjustedDamage.DamageDict[type] -= bonus;

        var ev = new SurgeryStepDamageEvent(args.User, args.Body, args.Part, args.Surgery, adjustedDamage, 0.5f);
        RaiseLocalEvent(args.Body, ref ev);
    }

    private void OnTendWoundsCheck(Entity<SurgeryTendWoundsEffectComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        var group = ent.Comp.MainGroup == "Brute" ? BruteDamageTypes : BurnDamageTypes;

        if (HasDamageGroup(args.Body, group, out var _)
            || HasDamageGroup(args.Part, group, out var _))
            args.Cancelled = true;
    }

    private void OnCavityStep(Entity<SurgeryStepCavityEffectComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp) || partComp.PartType != BodyPartType.Torso)
            return;

        var activeHandEntity = _hands.EnumerateHeld(args.User).FirstOrDefault();
        if (activeHandEntity != default
            && ent.Comp.Action == "Insert"
            && TryComp(activeHandEntity, out ItemComponent? itemComp)
            && (itemComp.Size.Id == "Tiny"
            || itemComp.Size.Id == "Small")
            && !string.IsNullOrEmpty(partComp.ItemInsertionSlot)
            && TryComp(args.Part, out ItemSlotsComponent? itemSlotsComp)
            && _itemSlotsSystem.TryGetSlot((args.Part, itemSlotsComp), partComp.ItemInsertionSlot, out var slot))
        {
            _itemSlotsSystem.TryInsert((args.Part, itemSlotsComp), partComp.ItemInsertionSlot, activeHandEntity, args.User);
        }
        else if (ent.Comp.Action == "Remove"
            && !string.IsNullOrEmpty(partComp.ItemInsertionSlot)
            && TryComp(args.Part, out ItemSlotsComponent? itemSlotsComp2)
            && _itemSlotsSystem.TryGetSlot((args.Part, itemSlotsComp2), partComp.ItemInsertionSlot, out var slot2))
        {
            _itemSlotsSystem.TryEjectToHands(args.Part, slot2, args.User);
        }
    }

    private void OnCavityCheck(Entity<SurgeryStepCavityEffectComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || !TryComp(args.Part, out ItemSlotsComponent? itemComp)
            || string.IsNullOrEmpty(partComp.ContainerName))
            args.Cancelled = true;
        else if (ent.Comp.Action == "Insert"
            && !itemComp.Slots[partComp.ContainerName].HasItem)
            args.Cancelled = true;
        else if (ent.Comp.Action == "Remove"
            && itemComp.Slots[partComp.ContainerName].HasItem)
            args.Cancelled = true;
    }

    private void OnAddPartStep(Entity<SurgeryAddPartStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp))
            return;

        foreach (var tool in args.Tools)
        {
            if (TryComp(tool, out BodyPartComponent? partComp)
                && partComp.PartType == removedComp.Part
                && (removedComp.Symmetry == null || partComp.Symmetry == removedComp.Symmetry))
            {
                var slotName = removedComp.Symmetry != null
                    ? $"{removedComp.Symmetry.Value.ToString().ToLower()} {removedComp.Part.ToString().ToLower()}"
                    : removedComp.Part.ToString().ToLower();
                _body.TryCreatePartSlot(args.Part, slotName, partComp.PartType, out var _);
                _body.AttachPart(args.Part, slotName, tool);
                EnsureComp<BodyPartReattachedComponent>(tool);
                var ev = new BodyPartAttachedEvent();
                RaiseLocalEvent(args.Body, ref ev);
            }
        }
    }

    private void OnAddOrganSlotStep(Entity<SurgeryAddOrganSlotStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryOrganSlotConditionComponent? condition))
            return;

        _body.TryCreateOrganSlot(args.Part, condition.OrganSlot, out _);
    }

    private void OnAddOrganSlotCheck(Entity<SurgeryAddOrganSlotStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryOrganSlotConditionComponent? condition))
            return;

        args.Cancelled = !_body.CanInsertOrgan(args.Part, condition.OrganSlot);
    }

    private void OnAffixPartStep(Entity<SurgeryAffixPartStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp))
            return;

        var targetPart = _body.GetBodyChildrenOfType(args.Body, removedComp.Part, symmetry: removedComp.Symmetry).FirstOrDefault();

        if (targetPart != default)
        {
            var ev = new BodyPartEnableChangedEvent();
            RaiseLocalEvent(args.Body, ref ev);
            _damageable.TryChangeDamage((args.Body, null),
                _body.GetHealingSpecifier(new BodyPartComponent()) * 2,
                ignoreResistances: false,
                interruptsDoAfters: true);
            RemComp<BodyPartReattachedComponent>(targetPart);
        }
    }

    private void OnAffixPartCheck(Entity<SurgeryAffixPartStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp))
            return;

        var targetPart = _body.GetBodyChildrenOfType(args.Body, removedComp.Part, symmetry: removedComp.Symmetry).FirstOrDefault();

        if (targetPart != default
            && HasComp<BodyPartReattachedComponent>(targetPart))
            args.Cancelled = true;
    }

    private void OnAddPartCheck(Entity<SurgeryAddPartStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp)
            || !_body.GetBodyChildrenOfType(args.Body, removedComp.Part, symmetry: removedComp.Symmetry).Any())
            args.Cancelled = true;
    }

    private void OnRemovePartStep(Entity<SurgeryRemovePartStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body)
            return;

        var ev = new AmputateAttemptEvent();
        RaiseLocalEvent(args.Part, ref ev);
        _hands.TryPickupAnyHand(args.User, args.Part);
    }

    private void OnRemovePartCheck(Entity<SurgeryRemovePartStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body == args.Body)
            args.Cancelled = true;
    }

    private void OnAddOrganStep(Entity<SurgeryAddOrganStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body
            || !TryComp(args.Surgery, out SurgeryOrganConditionComponent? organComp)
            || organComp.Organ == null)
            return;

        var firstOrgan = organComp.Organ.Values.FirstOrDefault();
        if (firstOrgan.Component == null)
            return;

        foreach (var tool in args.Tools)
        {
            if (HasComp(tool, firstOrgan.Component.GetType())
                && TryComp<OrganComponent>(tool, out var insertedOrgan)
                && _body.InsertOrgan(args.Part, tool, insertedOrgan.SlotId ?? string.Empty, (args.Part, partComp), insertedOrgan))
            {
                EnsureComp<OrganReattachedComponent>(tool);
                if (_body.TrySetOrganUsed(tool, true, insertedOrgan)
                    && insertedOrgan.OriginalBody != args.Body)
                {
                    var ev = new SurgeryStepDamageChangeEvent(args.User, args.Body, args.Part, ent);
                    RaiseLocalEvent(ent, ref ev);
                    args.Complete = true;
                }
                break;
            }
        }
    }

    private void OnAddOrganCheck(Entity<SurgeryAddOrganStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ is null
            || !TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body)
            return;

        foreach (var reg in organComp.Organ.Values)
        {
            if (!_body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var _))
            {
                args.Cancelled = true;
            }
        }
    }

    private void OnAffixOrganStep(Entity<SurgeryAffixOrganStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryOrganConditionComponent? removedOrganComp)
            || removedOrganComp.Organ == null
            || !removedOrganComp.Reattaching)
            return;

        foreach (var reg in removedOrganComp.Organ.Values)
        {
            _body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs);
            if (organs != null && organs.Count > 0)
                RemComp<OrganReattachedComponent>(organs[0]);
        }
    }

    private void OnAffixOrganCheck(Entity<SurgeryAffixOrganStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryOrganConditionComponent? removedOrganComp)
            || removedOrganComp.Organ == null
            || !removedOrganComp.Reattaching)
            return;

        foreach (var reg in removedOrganComp.Organ.Values)
        {
            _body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs);
            if (organs != null
                && organs.Count > 0
                && organs.Any(organ => HasComp<OrganReattachedComponent>(organ)))
                args.Cancelled = true;
        }
    }

    private void OnRemoveOrganStep(Entity<SurgeryRemoveOrganStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ == null)
            return;

        foreach (var reg in organComp.Organ.Values)
        {
            _body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs);
            if (organs != null && organs.Count > 0)
            {
                if (TryComp<OrganComponent>(organs[0], out var organ))
                    _body.RemoveOrgan(organs[0], organ);
                _hands.TryPickupAnyHand(args.User, organs[0]);
            }
        }
    }

    private void OnRemoveOrganCheck(Entity<SurgeryRemoveOrganStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ == null
            || !TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body)
            return;

        foreach (var reg in organComp.Organ.Values)
        {
            if (_body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs)
                && organs != null
                && organs.Count > 0)
            {
                args.Cancelled = true;
                return;
            }
        }
    }

    private void OnAddMarkingStep(Entity<SurgeryAddMarkingStepComponent> ent, ref SurgeryStepEvent args)
    {
    }

    private void OnAddMarkingCheck(Entity<SurgeryAddMarkingStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        args.Cancelled = true;
    }

    private void OnRemoveMarkingStep(Entity<SurgeryRemoveMarkingStepComponent> ent, ref SurgeryStepEvent args)
    {
    }

    private void OnRemoveMarkingCheck(Entity<SurgeryRemoveMarkingStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
    }

    private void OnSurgeryTargetStepChosen(Entity<SurgeryTargetComponent> ent, ref SurgeryStepChosenBuiMsg args)
    {
        var user = args.Actor;
        if (GetEntity(args.Entity) is {} body &&
            GetEntity(args.Part) is {} targetPart)
        {
            TryDoSurgeryStep(body, targetPart, user, args.Surgery, args.Step);
        }
    }
    #endregion

    #region Helper Methods
    private bool HasDamageGroup(EntityUid entity, string[] group, out DamageableComponent? damageable)
    {
        if (!TryComp<DamageableComponent>(entity, out var damageableComp))
        {
            damageable = null;
            return false;
        }

        damageable = damageableComp;
        var allDamage = _damageable.GetAllDamage((entity, damageableComp));
        return group.Any(damageType => allDamage.DamageDict.TryGetValue(damageType, out var value) && value > FixedPoint2.Zero);
    }

    private void HandleSanitization(SurgeryStepEvent args)
    {
        if (_inventory.TryGetSlotEntity(args.User, "gloves", out var _)
            && _inventory.TryGetSlotEntity(args.User, "mask", out var _))
            return;

        if (HasComp<SanitizedComponent>(args.User))
            return;
        var sepsis = new DamageSpecifier(_prototypes.Index<DamageTypePrototype>("Poison"), 5);
        var ev = new SurgeryStepDamageEvent(args.User, args.Body, args.Part, args.Surgery, sepsis, 0.5f);
        RaiseLocalEvent(args.Body, ref ev);
    }

    private bool TryToolAudio(Entity<SurgeryStepComponent> ent, SurgeryStepEvent args)
    {
        if (ent.Comp.Tool == null)
            return true;
        foreach (var reg in ent.Comp.Tool.Values)
        {
            if (!AnyHaveComp(args.Tools, reg.Component, out var tool, out _))
                return false;

            if (_net.IsServer &&
                TryComp(tool, out SurgeryToolComponent? toolComp) &&
                toolComp.EndSound != null)
            {
                _audio.PlayPvs(toolComp.EndSound, tool);
            }
        }

        return true;
    }

    private void HandleOrganModification(EntityUid organTarget,
        EntityUid bodyTarget,
        Dictionary<string, ComponentRegistry>? modifications,
        bool remove = false)
    {
        if (modifications == null)
            return;

        var organSlotIdToOrgan = _body.GetPartOrgans(organTarget).ToDictionary(o => o.Item2.SlotId ?? string.Empty, o => o);

        foreach (var (slotId, components) in modifications)
        {
            if (!organSlotIdToOrgan.TryGetValue(slotId, out var organValue))
                continue;

            var (organId, organ) = organValue;

            if (remove)
            {
                if (!_body.OrganOnAddContainsKey(organId, string.Empty))
                    continue;
                var ev = new OrganComponentsModifyEvent(bodyTarget, organId, false);
                RaiseLocalEvent(organId, ref ev);
                foreach (var key in components.Keys)
                    _body.RemoveOrganOnAddComponent(organId, key);
            }
            else
            {
                foreach (var (key, compToAdd) in components)
                    _body.AddOrganOnAddComponent(organId, key, compToAdd);

                EnsureComp<OrganEffectComponent>(organId);
                var ev = new OrganComponentsModifyEvent(bodyTarget, organId, true);
                RaiseLocalEvent(organId, ref ev);
            }
        }
    }

    private void AddOrRemoveComponentsToEntity(EntityUid ent, ComponentRegistry? componentRegistry, bool remove = false)
    {
        if (componentRegistry == null)
            return;
        foreach (var reg in componentRegistry.Values)
        {
            var compType = reg.Component.GetType();
            if (remove)
                RemComp(ent, compType);
            else
            {
                if (HasComp(ent, compType))
                    continue;
                AddComp(ent, _compFactory.GetComponent(compType));
            }
        }
    }

    private bool TryToolCheck(ComponentRegistry? components, EntityUid target, bool checkMissing = true)
    {
        if (components == null)
            return false;

        foreach (var (key, entry) in components)
        {
            var hasComponent = HasComp(target, entry.Component.GetType());
            if (checkMissing != hasComponent)
                return true;
        }

        return false;
    }

    private bool TryToolOrganCheck(IReadOnlyDictionary<string, ComponentRegistry>? organChanges, EntityUid part, bool checkMissing = true)
    {
        if (organChanges == null)
            return false;

        var organSlotIdToOrgan = _body.GetPartOrgans(part).ToDictionary(o => o.Item2.SlotId ?? string.Empty, o => o.Item1);
        foreach (var (organSlotId, compsToAdd) in organChanges)
        {
            if (!organSlotIdToOrgan.TryGetValue(organSlotId, out var organId))
                continue;

            if (checkMissing)
            {
                var onAdd = _body.GetOrganOnAdd(organId);
                if (onAdd == null || compsToAdd.Keys.Any(key => !onAdd.ContainsKey(key)))
                {
                    return true;
                }
            }
            else
            {
                var onAdd = _body.GetOrganOnAdd(organId);
                if (onAdd == null)
                    continue;

                if (compsToAdd.Keys.Any(key => onAdd.ContainsKey(key)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryDoSurgeryStep(EntityUid body, EntityUid targetPart, EntityUid user, EntProtoId surgeryId, EntProtoId stepId)
    {
        if (!IsSurgeryValid(body, targetPart, surgeryId, stepId, user, out var surgery, out var part, out var step))
            return false;

        if (!PreviousStepsComplete(body, part, surgery, stepId) ||
            IsStepComplete(body, part, stepId, surgery))
            return false;

        if (!CanPerformStep(user, body, part, step, true, out _, out _, out var validTools))
            return false;

        var speed = 1f;
        var usedEv = new SurgeryToolUsedEvent(user, body);
        if (validTools?.Count > 0)
        {
            foreach (var (tool, toolSpeed) in validTools)
            {
                RaiseLocalEvent(tool, ref usedEv);
                if (usedEv.Cancelled)
                    return false;

                speed *= toolSpeed;
            }

            if (_net.IsServer)
            {
                foreach (var tool in validTools.Keys)
                {
                    if (TryComp(tool, out SurgeryToolComponent? toolComp) &&
                        toolComp.StartSound != null)
                    {
                        _audio.PlayPvs(toolComp.StartSound, tool);
                    }
                }
            }
        }

        if (TryComp(body, out TransformComponent? xform))
            _rotateToFace.TryFaceCoordinates(user, _transform.GetMapCoordinates(body, xform).Position);

        var ev = new SurgeryDoAfterEvent(surgeryId, stepId);
        var duration = GetSurgeryDuration(step, user, body, speed);

        if (TryComp(user, out SurgerySpeedModifierComponent? surgerySpeedMod)
            && surgerySpeedMod is not null)
            duration = duration / surgerySpeedMod.SpeedModifier;

        var doAfter = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(duration), ev, body, part)
        {
            BreakOnMove = true,
            CancelDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameEvent,
            NeedHand = true,
            BreakOnHandChange = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return false;

        var userName = Identity.Entity(user, EntityManager);
        var targetName = Identity.Entity(body, EntityManager);

        var locName = $"surgery-popup-procedure-{surgeryId}-step-{stepId}";
        var locResult = Loc.GetString(locName,
            ("user", userName), ("target", targetName), ("part", part));

        if (locResult == locName)
            locResult = Loc.GetString($"surgery-popup-step-{stepId}",
                ("user", userName), ("target", targetName), ("part", part));

        _popup.PopupEntity(locResult, user);
        return true;
    }

    private float GetSurgeryDuration(EntityUid surgeryStep, EntityUid user, EntityUid target, float toolSpeed)
    {
        if (!TryComp(surgeryStep, out SurgeryStepComponent? stepComp))
            return 2f;

        var speed = toolSpeed;

        if (TryComp(user, out SurgerySpeedModifierComponent? surgerySpeedMod))
            speed *= surgerySpeedMod.SpeedModifier;

        return stepComp.Duration / speed;
    }
    private (Entity<SurgeryComponent> Surgery, int Step)? GetNextStep(EntityUid body, EntityUid part, Entity<SurgeryComponent?> surgery, List<EntityUid> requirements)
    {
        if (!Resolve(surgery, ref surgery.Comp))
            return null;

        if (requirements.Contains(surgery))
            throw new ArgumentException($"Surgery {surgery} has a requirement loop: {string.Join(", ", requirements)}");

        requirements.Add(surgery);

        if (surgery.Comp.Requirement is { } requirementId &&
            GetSingleton(requirementId) is { } requirement &&
            GetNextStep(body, part, requirement, requirements) is { } requiredNext)
        {
            return requiredNext;
        }

        for (var i = 0; i < surgery.Comp.Steps.Count; i++)
        {
            var surgeryStep = surgery.Comp.Steps[i];
            if (!IsStepComplete(body, part, surgeryStep, surgery))
                return ((surgery, surgery.Comp), i);
        }

        return null;
    }

    public (Entity<SurgeryComponent> Surgery, int Step)? GetNextStep(EntityUid body, EntityUid part, EntityUid surgery)
    {
        return GetNextStep(body, part, surgery, new List<EntityUid>());
    }

    public bool PreviousStepsComplete(EntityUid body, EntityUid part, Entity<SurgeryComponent> surgery, EntProtoId step)
    {
        if (surgery.Comp.Requirement is { } requirement)
        {
            if (GetSingleton(requirement) is not { } requiredEnt ||
                !TryComp(requiredEnt, out SurgeryComponent? requiredComp) ||
                !PreviousStepsComplete(body, part, (requiredEnt, requiredComp), step))
            {
                return false;
            }
        }

        foreach (var surgeryStep in surgery.Comp.Steps)
        {
            if (surgeryStep == step)
                break;

            if (!IsStepComplete(body, part, surgeryStep, surgery))
                return false;
        }

        return true;
    }

    public bool CanPerformStep(EntityUid user, EntityUid body, EntityUid part,
        EntityUid step, bool doPopup, out string? popup, out StepInvalidReason reason,
        out Dictionary<EntityUid, float>? validTools)
    {
        var type = BodyPartType.Other;
        if (TryComp(part, out BodyPartComponent? partComp))
        {
            type = partComp.PartType;
        }

        var slot = type switch
        {
            BodyPartType.Head => SlotFlags.HEAD,
            BodyPartType.Torso => SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING,
            BodyPartType.Arm => SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING,
            BodyPartType.Hand => SlotFlags.GLOVES,
            BodyPartType.Leg => SlotFlags.OUTERCLOTHING | SlotFlags.LEGS,
            BodyPartType.Foot => SlotFlags.FEET,
            BodyPartType.Tail => SlotFlags.NONE,
            BodyPartType.Other => SlotFlags.NONE,
            _ => SlotFlags.NONE
        };

        var check = new SurgeryCanPerformStepEvent(user, body, GetTools(user), slot);
        RaiseLocalEvent(step, ref check);
        popup = check.Popup;
        validTools = check.ValidTools;

        if (check.Invalid != StepInvalidReason.None)
        {
            if (doPopup && check.Popup != null)
                _popup.PopupEntity(check.Popup, user, user, PopupType.SmallCaution);

            reason = check.Invalid;
            return false;
        }

        reason = default;
        return true;
    }

    public bool CanPerformStep(EntityUid user, EntityUid body, EntityUid part, EntityUid step, bool doPopup)
    {
        return CanPerformStep(user, body, part, step, doPopup, out _, out _, out _);
    }

    public bool IsStepComplete(EntityUid body, EntityUid part, EntProtoId step, EntityUid surgery)
    {
        if (GetSingleton(step) is not { } stepEnt)
            return false;

        var ev = new SurgeryStepCompleteCheckEvent(body, part, surgery);
        RaiseLocalEvent(stepEnt, ref ev);
        return !ev.Cancelled;
    }

    private bool AnyHaveComp(List<EntityUid> tools, IComponent component, out EntityUid withComp, out float speed)
    {
        foreach (var tool in tools)
        {
            if (EntityManager.TryGetComponent(tool, component.GetType(), out var found) && found is ISurgeryToolComponent toolComp)
            {
                withComp = tool;
                speed = toolComp.Speed;
                return true;
            }
        }

        withComp = EntityUid.Invalid;
        speed = 1f;
        return false;
    }
    #endregion
}
