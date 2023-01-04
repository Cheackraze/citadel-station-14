using Robust.Shared.Audio;

namespace Content.Server.Shipyard.Components
{
    /// <summary>
    /// The console that allows you to buy/sell ships
    /// </summary>
    [RegisterComponent]
    public sealed class ShipyardConsoleComponent : Component
    {
        //datafield for which catalog to point to
        [DataField("soundError")] public SoundSpecifier ErrorSound =
            new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_sigh.ogg");

        [DataField("soundConfirm")]
        public SoundSpecifier ConfirmSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");
    }
}
