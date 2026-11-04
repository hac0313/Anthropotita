using Content.Shared.Bed.Sleep;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Chat;
using Content.Shared.Cloning.Events;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Eye.Blinking;
using Content.Shared.Gibbing;
using Content.Shared.Humanoid;
using Content.Shared.Medical;
using Content.Shared.Mobs;
using JetBrains.Annotations;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Body;

public sealed partial class BodySystem
{
    // Refrain from adding an infinite block of relays here - consuming systems can use RelayEvent
    private void InitializeRelay()
    {
        SubscribeLocalEvent<BodyComponent, ApplyMetabolicMultiplierEvent>(RefRelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, TryVomitEvent>(RefRelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, BeingGibbedEvent>(RefRelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, ApplyOrganProfileDataEvent>(RefRelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, ApplyOrganMarkingsEvent>(RefRelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, OrganCopyAppearanceEvent>(RefRelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, HumanoidLayerVisibilityChangedEvent>(RefRelayBodyEvent);

        SubscribeLocalEvent<BodyComponent, EmoteEvent>(RefRelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, SleepStateChangedEvent>(RefRelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, MobStateChangedEvent>(RefRelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, BlindnessChangedEvent>(RefRelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, CloningEvent>(RefRelayBodyEvent);

        SubscribeLocalEvent<BodyComponent, ToggleEyesActionEvent>(RelayBodyEvent);
        SubscribeLocalEvent<BodyComponent, CanSeeAttemptEvent>(RelayBodyEvent);
    }

    private void RefRelayBodyEvent<T>(EntityUid uid, BodyComponent component, ref T args) where T : struct
    {
        RelayEvent((uid, component), ref args);
    }

    private void RelayBodyEvent<T>(EntityUid uid, BodyComponent component, T args) where T : class
    {
        RelayEvent((uid, component), args);
    }

    /// <summary>
    /// Relays the given event to organs within a body.
    /// </summary>
    /// <param name="ent">The body to relay the event within</param>
    /// <param name="args">The event to relay</param>
    /// <typeparam name="T">The type of the event</typeparam>
    [PublicAPI]
    public void RelayEvent<T>(Entity<BodyComponent> ent, ref T args) where T : struct
    {
        var ev = new BodyRelayedEvent<T>(ent, args);
        foreach (var organ in ent.Comp.Organs?.ContainedEntities ?? [])
        {
            RaiseLocalEvent(organ, ref ev);
        }
        args = ev.Args;
    }

    /// <summary>
    /// Relays the given event to organs within a body.
    /// </summary>
    /// <param name="ent">The body to relay the event within</param>
    /// <param name="args">The event to relay</param>
    /// <typeparam name="T">The type of the event</typeparam>
    [PublicAPI]
    public void RelayEvent<T>(Entity<BodyComponent> ent, T args) where T : class
    {
        var ev = new BodyRelayedEvent<T>(ent, args);
        foreach (var organ in ent.Comp.Organs?.ContainedEntities ?? [])
        {
            RaiseLocalEvent(organ, ref ev);
        }
    }

    /// <summary>
    /// Gets the OnAdd component registry for an organ, if any.
    /// </summary>
    [PublicAPI]
    public ComponentRegistry? GetOrganOnAdd(EntityUid organId)
    {
        if (TryComp(organId, out OrganComponent? organ))
            return organ.OnAdd;
        return null;
    }

    /// <summary>
    /// Sets the OnAdd component registry for an organ.
    /// </summary>
    [PublicAPI]
    public void SetOrganOnAdd(EntityUid organId, ComponentRegistry? registry)
    {
        if (TryComp(organId, out OrganComponent? organ))
            organ.OnAdd = registry;
    }

    /// <summary>
    /// Adds a component to an organ's OnAdd registry.
    /// </summary>
    [PublicAPI]
    public void AddOrganOnAddComponent(EntityUid organId, string key, EntityPrototype.ComponentRegistryEntry value)
    {
        if (TryComp(organId, out OrganComponent? organ))
        {
            organ.OnAdd ??= new ComponentRegistry();
            organ.OnAdd[key] = value;
        }
    }

    /// <summary>
    /// Removes a component from an organ's OnAdd registry.
    /// </summary>
    [PublicAPI]
    public void RemoveOrganOnAddComponent(EntityUid organId, string key)
    {
        if (TryComp(organId, out OrganComponent? organ) && organ.OnAdd != null)
            organ.OnAdd.Remove(key);
    }

    /// <summary>
    /// Checks if an organ's OnAdd registry contains a key.
    /// </summary>
    [PublicAPI]
    public bool OrganOnAddContainsKey(EntityUid organId, string key)
    {
        if (TryComp(organId, out OrganComponent? organ) && organ.OnAdd != null)
            return organ.OnAdd.ContainsKey(key);
        return false;
    }

    /// <summary>
    /// Gets all body children (organs) of an entity.
    /// </summary>
    [PublicAPI]
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

    /// <summary>
    /// Gets all body organs of an entity as (EntityUid, OrganComponent) pairs.
    /// </summary>
    [PublicAPI]
    public IEnumerable<(EntityUid, OrganComponent)> GetBodyOrgans(EntityUid uid, BodyComponent? body = null)
    {
        if (!Resolve(uid, ref body))
            yield break;

        if (body.Organs != null)
        {
            foreach (var organ in body.Organs.ContainedEntities)
            {
                if (TryComp(organ, out OrganComponent? organComp))
                    yield return (organ, organComp);
            }
        }
    }

    /// <summary>
    /// Tries to get body organ entity components of type T.
    /// </summary>
    [PublicAPI]
    public bool TryGetBodyOrganEntityComps<T>(EntityUid uid, out T? comp) where T : IComponent
    {
        comp = default;
        if (!TryComp(uid, out BodyComponent? body))
            return false;

        if (body.Organs != null)
        {
            foreach (var organ in body.Organs.ContainedEntities)
            {
                if (TryComp(organ, out T? organComp))
                {
                    comp = organComp;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the target body part for a body part component.
    /// </summary>
    [PublicAPI]
    public Content.Shared._Shitmed.Targeting.TargetBodyPart? GetTargetBodyPart(BodyPartComponent part)
    {
        return (part.PartType, part.Symmetry) switch
        {
            (BodyPartType.Head, _) => Content.Shared._Shitmed.Targeting.TargetBodyPart.Head,
            (BodyPartType.Torso, _) => Content.Shared._Shitmed.Targeting.TargetBodyPart.Torso,
            (BodyPartType.Arm, BodyPartSymmetry.Left) => Content.Shared._Shitmed.Targeting.TargetBodyPart.LeftArm,
            (BodyPartType.Arm, BodyPartSymmetry.Right) => Content.Shared._Shitmed.Targeting.TargetBodyPart.RightArm,
            (BodyPartType.Hand, BodyPartSymmetry.Left) => Content.Shared._Shitmed.Targeting.TargetBodyPart.LeftHand,
            (BodyPartType.Hand, BodyPartSymmetry.Right) => Content.Shared._Shitmed.Targeting.TargetBodyPart.RightHand,
            (BodyPartType.Leg, BodyPartSymmetry.Left) => Content.Shared._Shitmed.Targeting.TargetBodyPart.LeftLeg,
            (BodyPartType.Leg, BodyPartSymmetry.Right) => Content.Shared._Shitmed.Targeting.TargetBodyPart.RightLeg,
            (BodyPartType.Foot, BodyPartSymmetry.Left) => Content.Shared._Shitmed.Targeting.TargetBodyPart.LeftFoot,
            (BodyPartType.Foot, BodyPartSymmetry.Right) => Content.Shared._Shitmed.Targeting.TargetBodyPart.RightFoot,
            _ => null
        };
    }

    /// <summary>
    /// Returns whether an entity is missing a brain and heart.
    /// </summary>
    [PublicAPI]
    public bool MissingVitalOrgans(EntityUid uid)
    {
        if (!TryComp<BodyComponent>(uid, out var body))
            return false;

        if (!TryGetBodyOrganEntityComps<BrainComponent>(uid, out _))
            return false;

        return TryGetBodyOrganEntityComps<HeartComponent>(uid, out _);
    }
}

/// <summary>
/// Event wrapper for events being relayed to organs within a body.
/// </summary>
[ByRefEvent]
public record struct BodyRelayedEvent<TEvent>(Entity<BodyComponent> Body, TEvent Args);
