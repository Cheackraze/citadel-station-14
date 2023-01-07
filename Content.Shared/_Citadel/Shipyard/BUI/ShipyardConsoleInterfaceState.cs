using Robust.Shared.Serialization;

namespace Content.Shared.Shipyard.BUI;

[NetSerializable, Serializable]
public sealed class ShipyardConsoleInterfaceState : BoundUserInterfaceState
{
    public int Balance;
    public readonly bool IsTargetIdPresent;
    public readonly string TargetIdName;
    public readonly string? TargetIdFullName;
    public readonly string? TargetIdJobTitle;
    public ShipyardConsoleInterfaceState(int balance,
        bool isTargetIdPresent,
        string? targetIdFullName,
        string? targetIdJobTitle,
        string targetIdName)
    {
        Balance = balance;
        IsTargetIdPresent = isTargetIdPresent;
        TargetIdFullName = targetIdFullName;
        TargetIdJobTitle = targetIdJobTitle;
        TargetIdName = targetIdName;

    }
}
