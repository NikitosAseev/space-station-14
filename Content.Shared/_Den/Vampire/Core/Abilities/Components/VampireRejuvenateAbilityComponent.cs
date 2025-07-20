using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Den.Vampire.Core.Abilities.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(VampireRejuvenateAbilitySystem))]
public sealed partial class VampireRejuvenateAbilityComponent : Component
{
    public override bool SendOnlyToOwner => true;

    /// <summary>
    /// The prototype of the action given to the entity with this component.
    /// </summary>
    [DataField]
    public EntProtoId ActionProto = "ActionVampireGlareAbility";

    /// <summary>
    /// A place to store the action entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    /// <summary>
    /// Amount of "damage" to be done to the vampire using rejuvenate. This value should be negative.
    /// </summary>
    [DataField]
    public float StamHealing = -100.0f;

    /// <summary>
    /// The amount of time to remove from any status effects affecting the vampire (stuns, knockdown).
    /// </summary>
    [DataField]
    public TimeSpan StatusEffectReductionTime = TimeSpan.FromSeconds(120);

}
