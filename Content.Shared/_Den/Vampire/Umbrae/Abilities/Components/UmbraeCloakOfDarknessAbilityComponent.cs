using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Den.Vampire.Umbrae.Abilities.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedUmbraeCloakOfDarknessAbilitySystem))]
public sealed partial class UmbraeCloakOfDarknessAbilityComponent : Component
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

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);
}
