using System.Threading;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Den.Vampire.Umbrae.Abilities.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(UmbraeSoulAnchorAbilitySystem))]
public sealed partial class UmbraeSoulAnchorAbilityComponent : Component
{
    public override bool SendOnlyToOwner => true;

    /// <summary>
    /// The prototype of the action given to the entity with this component.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId ActionProto = "ActionUmbraeSoulAnchorAbility";

    /// <summary>
    /// The prototype of the action given to the entity with this component.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId SoulAnchor = "SoulAnchor";

    /// <summary>
    /// A place to store the action entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField, AutoNetworkedField]
    public EntityUid? ActiveAnchor;


    [DataField, AutoNetworkedField]
    public float TeleportTimer = 120;


    public CancellationTokenSource? TeleportTimerToken;

}
