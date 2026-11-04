// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Forensics.Systems;
using Content.Shared._Shitmed.StatusEffects;
using Content.Shared.Forensics;
using Content.Shared.Forensics.Components;
using Content.Shared.Popups;

namespace Content.Server._Shitmed.StatusEffects;

public sealed class ScrambleDnaEffectSystem : EntitySystem
{
    [Dependency] private readonly ForensicsSystem _forensicsSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ScrambleDnaEffectComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, ScrambleDnaEffectComponent component, ComponentInit args)
    {
        if (TryComp<DnaComponent>(uid, out var dna))
        {
            dna.DNA = _forensicsSystem.GenerateDNA();
        }
        if (TryComp<FingerprintComponent>(uid, out var fingerprint))
        {
            fingerprint.Fingerprint = _forensicsSystem.GenerateFingerprint();
        }
        _popup.PopupEntity(Loc.GetString("scramble-implant-activated-popup"), uid, uid);
    }
}
