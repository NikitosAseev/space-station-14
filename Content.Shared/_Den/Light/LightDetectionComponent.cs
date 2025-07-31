using System.Numerics;
using Robust.Shared.Physics;

namespace Content.Shared._Den.Light;

/// <summary>
/// </summary>
[RegisterComponent]
public sealed partial class LightDetectionComponent : Component
{
    /// <summary>
    ///  Is user standing on a lighted area?
    /// </summary>
    [DataField]
    public bool IsOnLight;

    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.25);

    [DataField]
    public Vector2 LastKnownPosition;

    [ViewVariables(VVAccess.ReadOnly)]
    public float CurrentIntensity;
}
