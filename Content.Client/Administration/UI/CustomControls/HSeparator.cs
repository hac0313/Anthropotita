// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface.Controls;

namespace Content.Client.Administration.UI.CustomControls;

/// <summary>
/// A simple horizontal separator control.
/// </summary>
public sealed partial class HSeparator : BoxContainer
{
    public HSeparator()
    {
        Orientation = BoxContainer.LayoutOrientation.Horizontal;
    }
}
