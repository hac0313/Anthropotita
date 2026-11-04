// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body;
using Content.Shared.Body.Systems;
using Content.Shared.Body.Events;
using Content.Shared._Shitmed.Body.Events;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Robust.Shared.Network;
using System.Numerics;

namespace Content.Shared._Shitmed.BodyEffects.Subsystems;

public sealed class GenerateChildPartSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GenerateChildPartComponent, OrganComponentsModifyEvent>(OnPartComponentsModify);
    }

    private void OnPartComponentsModify(EntityUid uid, GenerateChildPartComponent component, ref OrganComponentsModifyEvent args)
    {
        if (args.Add)
            CreatePart(uid, component);
    }

    private void CreatePart(EntityUid uid, GenerateChildPartComponent component)
    {
        if (!TryComp(uid, out BodyPartComponent? partComp)
            || partComp.Body is null
            || component.Active)
            return;

        if (_net.IsServer)
        {
            var childPart = Spawn(component.Id, new EntityCoordinates(partComp.Body.Value, Vector2.Zero));

            if (!TryComp(childPart, out BodyPartComponent? childPartComp))
                return;

            _bodySystem.TryCreatePartSlot(uid, childPartComp.Slot ?? string.Empty, childPartComp.PartType, out var _);
            _bodySystem.AttachPart(uid, childPartComp.Slot ?? string.Empty, childPart);
            component.ChildPart = childPart;
            component.Active = true;
            Dirty(childPart, childPartComp);
        }
    }

    private void DeletePart(EntityUid uid, GenerateChildPartComponent component)
    {
        if (!TryComp(uid, out BodyPartComponent? partComp))
            return;

        var ev = new BodyPartRemovedEvent((uid, partComp));
        RaiseLocalEvent(uid, ref ev);
        QueueDel(uid);
    }
}


