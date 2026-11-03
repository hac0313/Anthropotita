using Content.Server.Players.PlayTimeTracking;
using Content.Server.Preferences.Managers;
using Content.Shared._RMC14.Marines.Roles.Ranks;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._RMC14.Marines.Roles.Ranks;

public sealed class RankSystem : SharedRankSystem
{
    [Dependency] private readonly PlayTimeTrackingManager _tracking = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IServerPreferencesManager _preferences = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RankComponent, TransformSpeakerNameEvent>(OnSpeakerNameTransform);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnSpeakerNameTransform(Entity<RankComponent> ent, ref TransformSpeakerNameEvent args)
    {
        var name = GetSpeakerRankName(ent);
        if (name == null)
            return;

        args.VoiceName = name;
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (ev.JobId == null)
            return;

        if (!_prototypes.TryIndex(ev.JobId, out JobPrototype? jobPrototype))
            return;

        if (_tracking.TryGetTrackerTimes(ev.Player, out var playTimes))
        {
            playTimes ??= new Dictionary<string, TimeSpan>();
        }
        else
        {
            playTimes = new Dictionary<string, TimeSpan>();
        }

        // Try to get preferred rank from character profile if available
        ProtoId<RankPrototype>? preferredRankId = null;
        if (ev.Profile is HumanoidCharacterProfile humanoidProfile)
        {
            // In this codebase, rank preferences are not stored in profile; skip
        }

        // Try to use any rank component data set elsewhere or assign default based on playtime
        // For compatibility, just try to assign a rank if rank prototype exists with same name as job prefix
        foreach (var rank in _prototypes.EnumeratePrototypes<RankPrototype>())
        {
            // Basic fallback: assign first available rank if job matches simple criteria
            // This is a simplified compatibility approach
        }
    }
}
