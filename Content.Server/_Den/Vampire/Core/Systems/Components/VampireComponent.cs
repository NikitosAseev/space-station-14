using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server._Den.Vampire.Components;

/// <summary>
/// </summary>
[RegisterComponent]
public sealed partial class VampireComponent : Component
{
    /// <summary>
    /// The BloodEssence prototype for the shop.
    /// </summary>
    [DataField]
    public ProtoId<CurrencyPrototype> BloodEssenceCurrencyPrototype = "BloodEssence";

    /// <summary>
    /// The vampire's shop action prototype.
    /// </summary>
    [DataField]
    public EntProtoId ActionVampireShopProto = "ActionVampireShop";

    /// <summary>
    /// A place for the action to be stored.
    /// </summary>
    [DataField]
    public EntityUid? ShopAction;
}
