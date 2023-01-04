using Content.Server.Shipyard.Systems;

namespace Content.Server.Shipyard.Components;

/// <summary>
/// Present on cargo shuttles to provide metadata such as preventing spam calling.
/// </summary>
[RegisterComponent, Access(typeof(ShipyardSystem))]
public sealed class ShuttleDeedComponent : Component
{
    [DataField("shuttleuid")]
    public EntityUid? ShuttleUid;
}
