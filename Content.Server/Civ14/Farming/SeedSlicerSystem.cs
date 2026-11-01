using Content.Shared.Botany;
using Content.Shared.Botany.Components;
using Content.Shared.Botany.Items.Components;
using Content.Shared.Botany.Systems;
using Content.Shared.Botany.Traits.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;

namespace Content.Server.Botany.Systems;

public sealed class SeedSlicerSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly BotanySystem _botanySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SeedSlicerComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, SeedSlicerComponent slicer, AfterInteractEvent args)
    {
        var target = args.Target;
        if (target == null)
            return;

        var user = args.User;

        if (!TryComp<ProduceComponent>(target.Value, out var produce))
            return;

        if (produce.PlantProtoId == null)
            return;

        EntityUid? snapshot = produce.PlantData;

        if (_botanySystem.TryGetPlantComponent<PlantTraitSeedlessComponent>(snapshot, produce.PlantProtoId, out _))
        {
            _popup.PopupCursor("This produce is seedless.", user, PopupType.SmallCaution);
            return;
        }

        _popup.PopupCursor($"You extract a seed from the produce.", user, PopupType.Medium);

        QueueDel(target.Value);

        var coords = Transform(uid).Coordinates;
        if (_botanySystem.TryGetPlantComponent<PlantDataComponent>(snapshot, produce.PlantProtoId, out var plantData))
        {
            _botanySystem.SpawnSeedPacket(plantData, produce.PlantProtoId.Value, snapshot, coords, user);
        }

        args.Handled = true;
    }
}
