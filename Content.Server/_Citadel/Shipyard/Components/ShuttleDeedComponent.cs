using Content.Shared.Shipyard;

namespace Content.Server.Shipyard.Components;

/// <summary>
/// Present on cargo shuttles to provide metadata such as preventing spam calling.
/// </summary>
[RegisterComponent, Access(typeof(SharedShipyardSystem))]
public sealed class ShuttleDeedComponent : Component
{
    [DataField("shuttleuid")]
    public EntityUid? ShuttleUid;
}
