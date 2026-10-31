// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Random;

namespace Content.Shared.Goobstation.Weapons.DodgeWideswing;

public sealed class DodgeWideswingSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;

    /// <summary>
    /// Subscribes to damage change events for entities with the <see cref="DodgeWideswingComponent"/>.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
    }

    // Adapted for current engine
}



