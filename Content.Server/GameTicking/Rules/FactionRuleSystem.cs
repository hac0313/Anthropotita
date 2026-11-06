using Content.Shared.GameTicking.Rules;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;

namespace Content.Server.GameTicking.Rules;

/// <summary>
///     Manages <see cref="FactionRuleComponent"/>
/// </summary>
public sealed partial class FactionRuleSystem : GameRuleSystem<FactionRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();
    }
}