using Content.Server.Access.Systems;
using Content.Server.Popups;
using Content.Server.Shipyard.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Shipyard.Events;
using Content.Shared.Shipyard.BUI;
using Content.Shared.Shipyard.Prototypes;
using Content.Shared.Shipyard.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Players;
using Content.Shared.Coordinates;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;
using Content.Server.StationRecords;
using Content.Shared.Access.Components;
using Content.Shared.StationRecords;
using Content.Shared.Shipyard;
using System.Linq;
using Content.Shared.Database;
using Content.Shared.Roles;
using System.Xml.Linq;

namespace Content.Server.Shipyard.Systems
{
    public sealed class ShipyardConsoleSystem : SharedShipyardSystem
    {

        [Dependency] private readonly IdCardSystem _idCardSystem = default!;
        [Dependency] private readonly AccessSystem _accessSystem = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly StationRecordsSystem _recordSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ShipyardSystem _shipyard = default!;
        [Dependency] private readonly StationSystem _station = default!;

        public void InitializeConsole()
        {
            SubscribeLocalEvent<ShipyardConsoleComponent, EntInsertedIntoContainerMessage>(UpdateConsole);
            SubscribeLocalEvent<ShipyardConsoleComponent, EntRemovedFromContainerMessage>(EmptyConsole);
            SubscribeLocalEvent<ShipyardConsoleComponent, ShipyardConsolePurchaseMessage>(OnPurchaseMessage);
            SubscribeLocalEvent<ShipyardConsoleComponent, BoundUIOpenedEvent>(OnConsoleUIOpened);
            SubscribeLocalEvent<ShipyardConsoleComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<ShipyardConsoleComponent, WriteToTargetIdMessage>(UpdateNames);
            SubscribeLocalEvent<RoundRestartCleanupEvent>(Reset);
        }

        private void OnInit(EntityUid uid, SharedShipyardConsoleComponent orderConsole, ComponentInit args)
        {
            //_shipyard.SetupShipyard(); ///if we have to start up the shipyard from here later
        }

        private void Reset(RoundRestartCleanupEvent ev)
        {
            Reset();
        }

        private void Reset()
        {
            //round cleanup event;
        }
        private void EmptyConsole(EntityUid uid, SharedShipyardConsoleComponent orderConsole, EntRemovedFromContainerMessage args)
        {
            var newState = new ShipyardConsoleInterfaceState(
                    0,
                    false,
                    null,
                    null,
                    string.Empty);
            _uiSystem.TrySetUiState(args.Container.Owner, ShipyardConsoleUiKey.Shipyard, newState);

        }
        private void UpdateConsole(EntityUid uid, SharedShipyardConsoleComponent orderConsole, EntInsertedIntoContainerMessage args)
        {
            ShipyardConsoleInterfaceState newState;

            // this could be prettier
            if (args.Entity is not { Valid: true } targetIdEntity)
            {
                newState = new ShipyardConsoleInterfaceState(
                    0,
                    false,
                    null,
                    null,
                    string.Empty);
            }
            else
            {
                if (!TryComp<IdCardComponent>(args.Entity, out var targetIdComponent))
                    return;
                if (!HasComp<AccessComponent>(args.Entity))
                    return;

                var name = string.Empty;

                var bank = EnsureComp<ShipyardBankAccountComponent>(args.Entity);
                newState = new ShipyardConsoleInterfaceState(
                    bank.Balance,
                    true,
                    targetIdComponent.FullName,
                    targetIdComponent.JobTitle,
                    name);
            }

            _uiSystem.TrySetUiState(args.Container.Owner, ShipyardConsoleUiKey.Shipyard, newState);

        }

        private void UpdateNames(EntityUid uid, ShipyardConsoleComponent component, WriteToTargetIdMessage args)
        {
            ShipyardConsoleInterfaceState newState;

            if (component.TargetIdSlot.Item is not { Valid: true } targetIdEntity)
            {
                newState = new ShipyardConsoleInterfaceState(
                    0,
                    false,
                    null,
                    null,
                    string.Empty);
            }
            else
            {
                if (!TryComp<IdCardComponent>(targetIdEntity, out var targetIdComponent))
                    return;
                if (!TryComp<AccessComponent>(targetIdEntity, out var targetAccessComponent))
                    return;

                _idCardSystem.TryChangeFullName(targetIdComponent.Owner, args.FullName, player: args.Session.AttachedEntity);
                _idCardSystem.TryChangeJobTitle(targetIdComponent.Owner, args.JobTitle, player: args.Session.AttachedEntity);

                var oldTags = _accessSystem.TryGetTags(targetIdComponent.Owner, targetAccessComponent) ?? new List<string>();
                oldTags = oldTags.ToList();

                if (oldTags.SequenceEqual(args.AccessList))
                    return;

                var addedTags = args.AccessList.Except(oldTags).Select(tag => "+" + tag).ToList();
                var removedTags = oldTags.Except(args.AccessList).Select(tag => "-" + tag).ToList();
                _accessSystem.TrySetTags(targetIdEntity, args.AccessList);

                UpdateStationRecord(targetIdEntity, args.FullName, args.JobTitle, string.Empty);
                var bank = EnsureComp<ShipyardBankAccountComponent>(targetIdComponent.Owner);
                newState = new ShipyardConsoleInterfaceState(
                bank.Balance,
                true,
                targetIdComponent.FullName,
                targetIdComponent.JobTitle,
                args.FullName);
            }
            _uiSystem.TrySetUiState(component.Owner, ShipyardConsoleUiKey.Shipyard, newState);
        }

        private void UpdateStationRecord(EntityUid idCard, string newFullName, string newJobTitle, string newJobProto)
        {
            var station = _station?.GetOwningStation(idCard);
            if (station == null
                || _recordSystem == null
                || !TryComp<StationRecordKeyStorageComponent>(idCard, out var keyStorage)
                || keyStorage.Key == null
                || !_recordSystem.TryGetRecord(station.Value, keyStorage.Key.Value, out GeneralStationRecord? record))
            {
                return;
            }

            record.Name = newFullName;
            record.JobTitle = newJobTitle;

            if (_prototypeManager.TryIndex(newJobProto, out JobPrototype? job))
            {
                record.JobPrototype = newJobProto;
                record.JobIcon = job.Icon;
            }

            _recordSystem.Synchronize(station.Value);
        }


        public void OnPurchaseMessage(EntityUid uid, SharedShipyardConsoleComponent component, ShipyardConsolePurchaseMessage args)
        {
            if (args.Session.AttachedEntity is not { Valid: true } player)
            {
                return;
            }
            if (args.Price <= 0)
                return;

            if (component.TargetIdSlot.ContainerSlot == null || component.TargetIdSlot.ContainerSlot.ContainedEntity == null)
                return;

            var bank = GetBankAccount((EntityUid) component.TargetIdSlot.ContainerSlot.ContainedEntity);

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
            PlayConfirmSound(uid, component);
            var newDeed = EnsureComp<ShuttleDeedComponent>(bank.Owner);
            _idCardSystem.TryGetIdCard(newDeed.Owner, out var idCard);
            if (idCard != null && TryComp<AccessComponent>(idCard.Owner, out var newCap))
            {
                //later we will make a custom pilot job, for now they get the captain treatment
                var newAccess = newCap.Tags.ToList();
                newAccess.Add($"Captain");
                _accessSystem.TrySetTags(newCap.Owner, newAccess, newCap);
            }

            RegisterDeed(newDeed, shuttle);
        }

        private void OnConsoleUIOpened(EntityUid uid, SharedShipyardConsoleComponent component, BoundUIOpenedEvent args)
        {
            if (args.Session.AttachedEntity is not { Valid: true } player)
                return;
        }

        private void ConsolePopup(ICommonSession session, string text)
        {
            _popup.PopupCursor(text, session);
        }

        private void PlayDenySound(EntityUid uid, SharedShipyardConsoleComponent component)
        {
            SoundSystem.Play(component.ErrorSound.GetSound(), Filter.Pvs(uid, entityManager: EntityManager), uid);
        }
        private void PlayConfirmSound(EntityUid uid, SharedShipyardConsoleComponent component)
        {
            SoundSystem.Play(component.ConfirmSound.GetSound(), Filter.Pvs(uid, entityManager: EntityManager), uid);
        }

        public bool TryPurchaseVessel(ShipyardBankAccountComponent component, VesselPrototype vessel, out ShuttleComponent? deed)
        {
            var stationUid = _station.GetOwningStation(component.Owner);

            if (component == null || vessel == null || vessel.ShuttlePath == null || stationUid == null)
            {
                deed = null;
                return false;
            };

            _shipyard.PurchaseShuttle(stationUid, vessel.ShuttlePath.ToString(), out deed);

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

        public ShipyardBankAccountComponent? GetBankAccount(EntityUid uid)
        {
            if (!TryComp<ShipyardBankAccountComponent>(uid, out var bankAccount))
            { 
                bankAccount = EnsureComp<ShipyardBankAccountComponent>(uid);
            }
            return bankAccount;
        }
    }
}
