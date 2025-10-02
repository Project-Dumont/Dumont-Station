// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <41803390+Kyoth25f@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
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
using Robust.Shared.Random;

namespace Content.Server._Gabystation.Economy
{
    public sealed class EconomyManagerSystem : EntitySystem
    {
        [Dependency] private readonly NameIdentifierSystem _name = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;
        [Dependency] private readonly IChatManager _chat = default!;
        [Dependency] private readonly IPrototypeManager _prototypes = default!;
        [Dependency] private readonly SharedIdCardSystem _id = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

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

            if (args.JobId is null || !_prototypes.TryIndex<JobPrototype>(args.JobId, out var proto) ||
                !proto.HasBank)
                return;

            int password = _random.Next(1000, 9999);
            if (!TryCreateAccount(out var number, (args.Station, comp), args.Mob, args.JobId, 100, password))
                return;
            Log.Debug($"Assigning bank id to {args.Profile.Name} ({number})!");

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
            EntityUid uid, string? jobId = null, int balance = 100, int password = 1234)
        {
            accountId = 0;
            var comp = station.Comp;

            // Assign a random bank account id
            _name.GenerateUniqueName(uid, _nameIdentifierGroup, out accountId);

            // Create the account interface
            var bankAccount = new BankAccount()
            {
                Balance = balance,
                JobId = jobId,
                InitialPassword = password,
                Password = password,
                Owner = uid,
                OwnerName = Name(uid) //TODO: owner change his account name
            };

            // Add the bank to the dict and ref dict
            comp.BankAccounts.Add(accountId, bankAccount);
            comp.UidBankRef.Add(uid, accountId);

            // Add the briefing in character menu
            if (TryComp<MindContainerComponent>(uid, out var mindc) && TryComp<MindComponent>(mindc.Mind, out var mind))
                mind.NanoBankAccount = accountId;

            if (_id.TryFindIdCard(uid, out var idCard))
            {
                // Add the account to the id card
                var bankCard = EnsureComp<NanoBankCardComponent>(idCard.Owner);
                bankCard.AccountId = accountId;
                bankCard.AccountPin = password;
                bankCard.LoggedIn = true;
                bankCard.Station = station.Owner;
                Dirty<NanoBankCardComponent>((idCard.Owner, bankCard));
            }

            return true;
        }

        public bool TryGetBalance(EconomyManagerComponent comp,
            int accountId,
            [NotNullWhen(true)] out int balance)
        {
            balance = 0;

            if (!comp.BankAccounts.ContainsKey(accountId) || !comp.BankAccounts.TryGetValue(accountId, out var bank))
                return false;

            balance = bank.Balance;
            return true;
        }

        public bool CanAfford(EconomyManagerComponent comp,
            int accountId, uint amount,
            [NotNullWhen(true)] out int balance)
        {
            if (!TryGetBalance(comp, accountId, out balance))
                return false;

            return balance >= amount;
        }

        public bool TryPurchase(EconomyManagerComponent comp,
            int accountId, uint price)
        {
            if (!TryGetBalance(comp, accountId, out var balance)
                || balance < price
                || !TryAddRemBalance(comp, accountId, -(int) price, raiseEvent: false))
                return false;

            var ev = new AccountTransferenceCompleted()
            {
                Type = TransferenceTypes.Purchase,
                AccountId = accountId,
                Amount = (int) price,
            };

            RaiseLocalEvent(comp.Owner, ev);

            return true;
        }

        public bool TrySetBalance(EconomyManagerComponent comp, int accountId, int balance, bool raiseEvent = true)
        {
            if (!comp.BankAccounts.ContainsKey(accountId)
                || !comp.BankAccounts.TryGetValue(accountId, out var bank))
                return false;

            if (bank.Balance == balance)
                return true; // Deveriamos levantar um evento, com Amount = 0, nesse caso?

            var previousBalance = bank.Balance;
            bank.Balance = balance;

            if (raiseEvent)
            {
                var ev = new AccountTransferenceCompleted()
                {
                    Type = TransferenceTypes.Update,
                    AccountId = accountId,
                    Account = bank,
                    Amount = balance - previousBalance
                };

                RaiseLocalEvent(comp.Owner, ev);
            }

            return true;
        }

        public bool TryAddRemBalance(EconomyManagerComponent comp, int accountId, int amount, bool raiseEvent = true)
        {
            if (!comp.BankAccounts.ContainsKey(accountId)
                || !comp.BankAccounts.TryGetValue(accountId, out var bank))
                return false;

            if (amount == 0)
                return true; // Deveriamos levantar um evento, com Amount = 0, nesse caso?

            bank.Balance += amount;

            if (raiseEvent)
            {
                var ev = new AccountTransferenceCompleted()
                {
                    Type = TransferenceTypes.Update,
                    AccountId = accountId,
                    Account = bank,
                    Amount = amount
                };

                RaiseLocalEvent(comp.Owner, ev);
            }

            return true;
        }

        public bool TryGetData(EconomyManagerComponent comp,
            int accountId,
            [NotNullWhen(true)] out IBankAccount? data)
        {
            data = null;

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

                comp.PaymentCooldownRemaining = comp.PaymentDelay;

                foreach (var (accountId, account) in comp.BankAccounts)
                {
                    if (account.JobId is null)
                        continue;
                    if (!_prototypes.TryIndex<JobPrototype>(account.JobId, out var proto))
                        continue;

                    account.Balance += proto.Salary;
                    Log.Debug("Event time");
                    var ev = new AccountTransferenceCompleted()
                    {
                        Type = TransferenceTypes.Payment,
                        AccountId = accountId,
                        Account = account,
                        Amount = proto.Salary
                    };
                    RaiseLocalEvent(uid, ev);
                    Log.Debug("Event done?");
                }

                RaiseLocalEvent(new AfterPaymentRotation() { Uid = uid });
            }
        }

        public void SetAccountData(EconomyManagerComponent comp, int account, IBankAccount data)
        {
            if (!comp.BankAccounts.ContainsKey(account))
                return;

            comp.BankAccounts[account] = data;
        }

        public bool GetAccountPassword(EconomyManagerComponent comp, int id,
            bool initial,
            [NotNullWhen(true)] out int password)
        {
            password = 0;

            if (!comp.BankAccounts.TryGetValue(id, out var bank))
                return false;

            password = initial ? bank.InitialPassword : bank.Password;
            return true;
        }

        public bool ValidateCard(EconomyManagerComponent comp, NanoBankCardComponent card)
        {
            return ValidateLogin(comp, card.AccountId, card.AccountPin);
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

        public bool TransferBalance(Entity<EconomyManagerComponent> economyManager, int targetId, int accountId, int amount)
        {
            var (ent, comp) = economyManager;

            // validate positive amount
            if (amount <= 0)
                return false;

            // validate accounts
            if (!TryGetData(comp, targetId, out var targetData)
                || !TryGetData(comp, accountId, out var data)
                || data.Balance < amount)
                return false;

            if (!TrySetBalance(comp, targetId, targetData.Balance + amount)
                || !TrySetBalance(comp, accountId, data.Balance - amount))
                return false;

            var ev = new AccountTransferenceCompleted()
            {
                Type = TransferenceTypes.Transference,
                AccountId = accountId,
                Account = data,
                Amount = amount,
                TargetAccount = targetId
            };

            RaiseLocalEvent(ent, ev);

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
