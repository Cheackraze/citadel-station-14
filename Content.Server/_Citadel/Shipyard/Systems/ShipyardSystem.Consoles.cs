using Content.Server.Access.Systems;
using Content.Server.Popups;
using Content.Server.Shipyard.Components;
using Content.Server.Shuttles.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.GameTicking;
using Content.Shared.Shipyard.Events;
using Content.Shared.Shipyard.BUI;
using Content.Shared.Shipyard.Prototypes;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Players;
using Content.Shared.Coordinates;
using Robust.Shared.Prototypes;
using Content.Server.Station.Components;

namespace Content.Server.Shipyard.Systems
{
    public sealed partial class ShipyardSystem
    {

        [Dependency] private readonly IdCardSystem _idCardSystem = default!;
        [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private void InitializeConsole()
        {
            SubscribeLocalEvent<ShipyardConsoleComponent, ShipyardConsolePurchaseMessage>(OnPurchaseMessage);
            SubscribeLocalEvent<ShipyardConsoleComponent, BoundUIOpenedEvent>(OnConsoleUIOpened);
            SubscribeLocalEvent<ShipyardConsoleComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<RoundRestartCleanupEvent>(Reset);
        }

        private void OnInit(EntityUid uid, ShipyardConsoleComponent orderConsole, ComponentInit args)
        {
            var station = _station.GetOwningStation(orderConsole.Owner);
        }

        private void Reset(RoundRestartCleanupEvent ev)
        {
            Reset();
        }

        private void Reset()
        {
            //round cleanup event;
        }

        private void UpdateConsole(ShipyardBankAccountComponent bank, EntityUid playerUid)
        {
            string idName;
            if (!_idCardSystem.TryGetIdCard(playerUid, out var idCard) || idCard == null || idCard.FullName == null)
            {
                idName = "Unknown User";
            }
            else
            {
                idName = idCard.FullName;
            }
            new ShipyardConsoleInterfaceState(idName, bank.Balance);

        }

        #region Interface


        private void OnPurchaseMessage(EntityUid uid, ShipyardConsoleComponent component, ShipyardConsolePurchaseMessage args)
        {
            if (args.Session.AttachedEntity is not {Valid: true} player)
                return;

            if (args.Price <= 0)
                return;

            var bank = GetBankAccount(player);

            if (bank == null)
                return;

            if (bank.Balance <= args.Price)
            {
                ConsolePopup(args.Session, Loc.GetString("shipyard-console-insufficient-funds", ("cost", args.Price)));
                PlayDenySound(uid, component);
                return;
            }

            if (!_prototypeManager.TryIndex<VesselPrototype>(args.Vessel, out var vessel) || vessel == null)
            {
                ConsolePopup(args.Session, Loc.GetString("shipyard-console-invalid-vessel", ("vessel", args.Vessel)));
                PlayDenySound(uid, component);
                return;
            }

            if (!TryPurchaseVessel(bank, vessel, out var shuttle) || shuttle == null)
            {
                PlayDenySound(uid, component);
                return;
            }

            DeductFunds(bank, vessel.Price);
            var newDeed = EnsureComp<ShuttleDeedComponent>(Spawn("CaptainIDCard", component.Owner.ToCoordinates()));
            RegisterDeed(newDeed, shuttle);
        }

        private void OnConsoleUIOpened(EntityUid uid, ShipyardConsoleComponent component, BoundUIOpenedEvent args)
        {
            if (args.Session.AttachedEntity is not { Valid: true } player)
                return;

            var bank = EnsureComp<ShipyardBankAccountComponent>(args.Session.AttachedEntity.Value);
            UpdateConsole(bank, args.Session.AttachedEntity.Value);
        }

        #endregion
        private void ConsolePopup(ICommonSession session, string text) => _popup.PopupCursor(text, session);

        private void PlayDenySound(EntityUid uid, ShipyardConsoleComponent component)
        {
            SoundSystem.Play(component.ErrorSound.GetSound(), Filter.Pvs(uid, entityManager: EntityManager), uid);
        }


        public bool TryPurchaseVessel(ShipyardBankAccountComponent component, VesselPrototype vessel, out ShuttleComponent? deed)
        {
            var stationUid = _station.GetOwningStation(component.Owner);

            if (component == null || vessel == null || vessel.ShuttlePath == null || stationUid == null)
            {
                deed = null;
                return false;
            };

            PurchaseShuttle(stationUid, vessel.ShuttlePath.ToString(), out deed);

            if (deed == null)
            {
                return false;
            };

            return true;
        }

        private void RegisterDeed(ShuttleDeedComponent deed, ShuttleComponent shuttle)
        {
            deed.ShuttleUid = shuttle.Owner;
            Dirty(deed); //done dirt cheap
        }

        public void DeductFunds(ShipyardBankAccountComponent component, int amount)
        {
            component.Balance = Math.Max(0, component.Balance - amount);
            Dirty(component);
        }

        public ShipyardBankAccountComponent? GetBankAccount(EntityUid playerUid)
        {
            TryComp<ShipyardBankAccountComponent>(playerUid, out var bankComponent);
            return bankComponent;
        }
    }
}
