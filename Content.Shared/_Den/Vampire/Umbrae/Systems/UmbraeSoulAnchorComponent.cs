using System.Threading;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Den.Vampire.Umbrae.Systems;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class UmbraeSoulAnchorComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? OwnerSoulAnchor;
}
