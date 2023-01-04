using Robust.Shared.Serialization;

namespace Content.Shared.Shipyard;

[NetSerializable, Serializable]
public enum ShipyardConsoleUiKey : byte
{
    Shipyard,
    HoSConsole,
    SAConsole
}

public abstract class SharedShipyardSystem : EntitySystem { }
