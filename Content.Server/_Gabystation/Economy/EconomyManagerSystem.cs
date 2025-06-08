using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.NameIdentifier;
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
        private readonly ProtoId<NameIdentifierGroupPrototype> _nameIdentifierGroup = "NanoBank";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        }

        private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
        {
            var stations = _gameTicker.GetSpawnableStations(); // this sucks
            if (!EntityManager.TryGetComponent<EconomyManagerComponent>(stations[0], out var comp))
                return;

            TryCreateAccount(out var number, args.Mob, args.JobId);
            Log.Debug($"Assigning bank id to {args.Profile.Name} ({number})!");
            GetAccountPassword(number, true, out var password);

            _chat.ChatMessageToOne(
                Shared.Chat.ChatChannel.Server,
                "You got an new [bold]NanoBank[/bold] account.",
                $"Your account id is [bold]{number}[/bold] and your initial password is '[bold]{password}[/bold]'.",
                default,
                false,
                args.Player.Channel
                );
        }

        public bool TryCreateAccount(out int accountId, EntityUid uid, string? jobId = null, float balance = 500, int password = 1234)
        {
            accountId = 0;
            var stations = _gameTicker.GetSpawnableStations(); // this sucks
            if (!EntityManager.TryGetComponent<EconomyManagerComponent>(stations[0], out var comp))
                return false;

            // Assign a random bank account id
            _name.GenerateUniqueName(uid, _nameIdentifierGroup, out accountId);

            var bankAccount = new BankAccount() { Balance = balance, JobId = jobId, InitialPassword = password, Password = password };
            comp.BankAccounts.Add(accountId, bankAccount);
            comp.UidBankRef.Add(uid, accountId);

            if (TryComp<MindContainerComponent>(uid, out var mindc) && TryComp<MindComponent>(mindc.Mind, out var mind))
                mind.NanoBankAccount = accountId;

            return true;
        }

        public bool TryGetBalance(int accountId, out float balance)
        {
            balance = 0;
            var stations = _gameTicker.GetSpawnableStations(); // this sucks
            if (!EntityManager.TryGetComponent<EconomyManagerComponent>(stations[0], out var comp))
                return false;

            if (!comp.BankAccounts.ContainsKey(accountId) || !comp.BankAccounts.TryGetValue(accountId, out var bank))
                return false;

            balance = bank.Balance;
            return true;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);


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

        public List<(int AccountId, EntityUid Uid, IBankAccount Account)> GetLinkedAccounts()
        {
            var result = new List<(int, EntityUid, IBankAccount)>();

            var stations = _gameTicker.GetSpawnableStations();
            if (!EntityManager.TryGetComponent<EconomyManagerComponent>(stations[0], out var comp))
                return result;

            foreach (var (uid, accountId) in comp.UidBankRef)
            {
                if (!comp.BankAccounts.TryGetValue(accountId, out var bankAccount))
                    continue;

                result.Add((accountId, uid, bankAccount));
            }
            return result;
        }
    }

    public sealed class BankAccount : IBankAccount
    {
        public required int Password { get; set; }
        public required int InitialPassword { get; set; }
        public float Balance { get; set; }
        public required string? JobId { get; set; }
    }

    public interface IBankAccount
    {
        int Password { get; set; }
        int InitialPassword { get; set; }
        float Balance { get; set; }
        string? JobId { get; set; }
    }
}
