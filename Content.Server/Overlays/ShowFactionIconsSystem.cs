using Content.Shared.Overlays;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.Overlays;

/// <summary>
///     Server-side system for managing faction icon assignments.
/// </summary>
public sealed class ShowFactionIconsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShowFactionIconsComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<ShowFactionIconsComponent> ent, ref ComponentStartup args)
    {
        // Could add server-side logic here if needed (e.g., default icon assignment based on faction)
    }
}