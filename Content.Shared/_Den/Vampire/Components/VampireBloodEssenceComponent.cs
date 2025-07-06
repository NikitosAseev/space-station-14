using Content.Shared.Store;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Den.Vampire.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VampireBloodEssenceComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<CurrencyPrototype> BloodEssenceCurrencyPrototype = "BloodEssence";
}
