using Robust.Shared.Serialization;

namespace Content.Shared.Shipyard.Events;

/// <summary>
///     Save ID info from the console
/// </summary>
[Serializable, NetSerializable]
public sealed class WriteToTargetIdMessage : BoundUserInterfaceMessage
{
    public readonly string FullName;
    public readonly string JobTitle;
    public readonly List<string> AccessList;
    public readonly string JobPrototype;

    public WriteToTargetIdMessage(string fullName, string jobTitle, List<string> accessList, string jobPrototype)
    {
        FullName = fullName;
        JobTitle = jobTitle;
        AccessList = accessList;
        JobPrototype = jobPrototype;
    }
}
