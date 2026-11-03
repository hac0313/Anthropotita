using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._RMC14.Mortar;

public abstract class SharedMortarSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly FixtureSystem _fixture = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MortarComponent, UseInHandEvent>(OnMortarUseInHand, before: [typeof(ActivatableUISystem)]);
        SubscribeLocalEvent<MortarComponent, InteractUsingEvent>(OnMortarInteractUsing);
        SubscribeLocalEvent<MortarComponent, ExaminedEvent>(OnMortarExamined);
        SubscribeLocalEvent<MortarComponent, ActivatableUIOpenAttemptEvent>(OnMortarActivatableUIOpenAttempt);
    }

    private void OnMortarUseInHand(Entity<MortarComponent> mortar, ref UseInHandEvent args)
    {
        args.Handled = true;
    }

    private void OnMortarInteractUsing(Entity<MortarComponent> mortar, ref InteractUsingEvent args)
    {
    }

    private void OnMortarExamined(Entity<MortarComponent> mortar, ref ExaminedEvent args)
    {
    }

    private void OnMortarActivatableUIOpenAttempt(Entity<MortarComponent> mortar, ref ActivatableUIOpenAttemptEvent args)
    {
    }
}
