using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.NameIdentifier;
using Content.Shared._Gabystation.Economy;
using Content.Shared._Gabystation.NanoBank;
using Content.Shared.Access.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.NameIdentifier;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Gabystation.Economy
{
    public sealed class EconomyManagerSystem : EntitySystem
    {
        [Dependency] private readonly NameIdentifierSystem _name = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;
        [Dependency] private readonly IChatManager _chat = default!;
        [Dependency] private readonly IPrototypeManager _prototypes = default!;
        [Dependency] private readonly SharedIdCardSystem _id = default!;
        private readonly ProtoId<NameIdentifierGroupPrototype> _nameIdentifierGroup = "NanoBank";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        }

        private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
        {
            if (!EntityManager.TryGetComponent<EconomyManagerComponent>(args.Station, out var comp))
                return;


            TryCreateAccount(out var number, (args.Station, comp), args.Mob, args.JobId);
            Log.Debug($"Assigning bank id to {args.Profile.Name} ({number})!");
            GetAccountPassword(number, true, out var password);

            _chat.ChatMessageToOne(
                Shared.Chat.ChatChannel.Server,
                Loc.GetString("economy-manager-chat-new-account"),
                Loc.GetString("economy-manager-chat-new-account-wrapped", ("number", number), ("password", password)),
                default,
                false,
                args.Player.Channel
                );
        }

        public bool TryCreateAccount(out int accountId, Entity<EconomyManagerComponent> station,
            EntityUid uid, string? jobId = null, float balance = 500, int password = 1234)
        {
            accountId = 0;
            var comp = station.Comp;

            // Assign a random bank account id
            _name.GenerateUniqueName(uid, _nameIdentifierGroup, out accountId);

            // Create the account interface
            var bankAccount = new BankAccount()
            { Balance = balance, JobId = jobId, InitialPassword = password, Password = password, Owner = uid };

            // Add the bank to the dict and ref dict
            comp.BankAccounts.Add(accountId, bankAccount);
            comp.UidBankRef.Add(uid, accountId);

            // Add the breafing in character menu
            if (TryComp<MindContainerComponent>(uid, out var mindc) && TryComp<MindComponent>(mindc.Mind, out var mind))
                mind.NanoBankAccount = accountId;

            if (_id.TryFindIdCard(uid, out var idCard))
            {
                // Add the account to the id card
                var bankCard = EnsureComp<NanoBankCardComponent>(idCard);
                bankCard.AccountId = accountId;
                bankCard.AccountPin = password;
                bankCard.LoggedIn = true;
            }

            return true;
        }

        public bool TryGetBalance(EntityUid station, int accountId, out float balance)
        {
            balance = 0;
            if (!EntityManager.TryGetComponent<EconomyManagerComponent>(station, out var comp))
                return false;

            if (!comp.BankAccounts.ContainsKey(accountId) || !comp.BankAccounts.TryGetValue(accountId, out var bank))
                return false;

            balance = bank.Balance;
            return true;
        }

        public bool TryGetData(EntityUid station, int accountId, out IBankAccount? data)
        {
            data = null;

            if (!EntityManager.TryGetComponent<EconomyManagerComponent>(station, out var comp))
                return false;

            if (!comp.BankAccounts.ContainsKey(accountId) || !comp.BankAccounts.TryGetValue(accountId, out data))
                return false;

            return true;
        }

        // This handles payments
        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var ents = AllEntityQuery<EconomyManagerComponent>();
            while (ents.MoveNext(out var uid, out var comp))
            {
                if (comp.PaymentCooldownRemaining >= 0f)
                {
                    comp.PaymentCooldownRemaining -= frameTime;
                    continue;
                }

                foreach (var (accountId, account) in comp.BankAccounts)
                {
                    if (account.JobId is null)
                        continue;
                    if (!_prototypes.TryIndex<JobPrototype>(account.JobId, out var proto))
                        continue;

                    account.Balance += proto.Salary;
                    Log.Debug("Event time");
                    var ev = new AccountPaymentCompleted() { AccountId = accountId, Account = account, Uid = uid, Payment = proto.Salary };
                    RaiseLocalEvent(ev);
                    Log.Debug("Event done?");
                }

                RaiseLocalEvent(new AfterPaymentRotation() { Uid = uid });

                Log.Debug("Payment time!");

                comp.PaymentCooldownRemaining = comp.PaymentDelay;
            }
        }

        public void SetAccountData(EntityUid station, int account, IBankAccount data)
        {
            if (!TryComp<EconomyManagerComponent>(station, out var comp))
                return;

            if (!comp.BankAccounts.ContainsKey(account))
                return;

            comp.BankAccounts[account] = data;
        }

        public bool GetAccountPassword(int id, bool initial, out int password)
        {
            password = 0;
            var stations = _gameTicker.GetSpawnableStations(); // this sucks
            if (!EntityManager.TryGetComponent<EconomyManagerComponent>(stations[0], out var comp))
                return false;

            if (!comp.BankAccounts.TryGetValue(id, out var bank))
                return false;

            password = initial ? bank.InitialPassword : bank.Password;
            return true;
        }

        public bool ValidateLogin(EconomyManagerComponent comp, int id, int pin)
        {
            if (!comp.BankAccounts.TryGetValue(id, out var account))
                return false;

            if (account.Password != pin)
            {
                account = null;
                return false;
            }

            return true;
        }

        public List<(int AccountId, EntityUid Uid, IBankAccount Account)> GetAllLinkedAccounts()
        {
            var result = new List<(int, EntityUid, IBankAccount)>();
            //TODO: return the station name

            var stations = _gameTicker.GetSpawnableStations();

            foreach (var station in stations)
            {
                if (!EntityManager.TryGetComponent<EconomyManagerComponent>(station, out var comp))
                    continue;

                foreach (var (uid, accountId) in comp.UidBankRef)
                {
                    if (!comp.BankAccounts.TryGetValue(accountId, out var bankAccount))
                        continue;

                    result.Add((accountId, uid, bankAccount));
                }
            }

            return result;

        }
    }
}
