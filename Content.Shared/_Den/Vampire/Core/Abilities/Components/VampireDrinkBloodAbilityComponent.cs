using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Den.Vampire.Core.Abilities.Components;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(VampireDrinkBloodAbilitySystem))]

public sealed partial class VampireDrinkBloodAbilityComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan DrinkBloodDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public EntProtoId ActionProto = "ActionVampireDrinkBlood";

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField, AutoNetworkedField]
    public List<ProtoId<ReagentPrototype>> BloodTarget = new()
    {
        "Blood",
        "CopperBlood",
        "InsectBlood"
    };

    [DataField, AutoNetworkedField]
    public FixedPoint2 BloodDrainAmountRemove = 20;

    [DataField, AutoNetworkedField]
    public float BloodDrainAmountGive = 20;
}
