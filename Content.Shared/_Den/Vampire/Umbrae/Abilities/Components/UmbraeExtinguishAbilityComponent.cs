using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Den.Vampire.Umbrae.Abilities.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedUmbraeExtinguishAbilitySystem))]
public sealed partial class UmbraeExtinguishAbilityComponent : Component
{
    public override bool SendOnlyToOwner => true;

    /// <summary>
    /// The prototype of the action given to the entity with this component.
    /// </summary>
    [DataField]
    public EntProtoId ActionProto = "ActionToggleUmbraeCloakOfDarknessAbility";

    /// <summary>
    /// A place to store the action entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField, AutoNetworkedField]
    public float Range = 10f;
}
