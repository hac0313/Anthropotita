using Content.Shared._Stalker.Characteristics;
using Content.Shared.Movement.Systems;

namespace Content.Server._Stalker.Characteristics.Modifiers.MovementSpeed;

public sealed class CharacteristicModifierMovementSpeedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CharacteristicModifierMovementSpeedComponent, CharacteristicUpdatedEvent>(OnUpdate);
    }

    private void OnUpdate(Entity<CharacteristicModifierMovementSpeedComponent> modifier, ref CharacteristicUpdatedEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(modifier.Owner);
    }

}
