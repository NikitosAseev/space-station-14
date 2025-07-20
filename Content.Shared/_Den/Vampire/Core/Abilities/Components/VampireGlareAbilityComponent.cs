using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Den.Vampire.Core.Abilities.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(VampireGlareAbilitySystem))]
public sealed partial class VampireGlareAbilityComponent : Component
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
    /// The radial distance around the vampire where the glare ability will hit.
    /// </summary>
    [DataField]
    public float Range = 1.0f;

    /// <summary>
    /// The amount of stamina damage to cause to the entity standing in front of the vampire.
    /// </summary>
    [DataField]
    public float DamageFront = 70;

    /// <summary>
    /// The amount of stamina damage to cause to the entity standing to the sides of the vampire.
    /// </summary>
    [DataField]
    public float DamageSides = 35;

    /// <summary>
    /// The amount of stamina damage to cause to the entity standing behind the vampire (should be rather small).
    /// </summary>
    [DataField]
    public float DamageRear = 10;

    /// <summary>
    /// The amount of time to knock down any entities to the front and sides of the vampire.
    /// </summary>
    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2.0);

    /// <summary>
    /// The amount of stun time given to any entities to the front of the vampire.
    /// </summary>
    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(8);

    /// <summary>
    /// The Effect to spawn when the glare is performed.
    /// </summary>
    [DataField]
    public EntProtoId FlashEffectProto = "ReactionFlash";

}
