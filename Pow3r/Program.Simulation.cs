// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Threading;
using Content.Server.Power.Pow3r;
using Robust.Shared.Threading;
using static Content.Server.Power.Pow3r.PowerState;


namespace Pow3r
{
    internal sealed partial class Program
    {
        private const int MaxTickData = 180;

        private PowerState _state = new();
        private Network _linking;
        private int _tickDataIdx;
        private bool _paused;

        private readonly string[] _solverNames =
        {
            nameof(BatteryRampPegSolver),
            nameof(NoOpSolver)
        };

        private readonly IPowerSolver[] _solvers = {
            new BatteryRampPegSolver(),
            new NoOpSolver()
        };

        private int _currentSolver;

        private readonly float[] _simTickTimes = new float[MaxTickData];
        private readonly Queue<object> _remQueue = new();
        private readonly Stopwatch _simStopwatch = new Stopwatch();

        private readonly IParallelManager _parallel = new SerialParallelManager();

        private void Tick(float frameTime)
        {
            if (_paused)
                return;

            RunSingleStep(frameTime);
        }

        private void RunSingleStep(float frameTime)
        {
            _simStopwatch.Restart();
            _tickDataIdx = (_tickDataIdx + 1) % MaxTickData;

            _solvers[_currentSolver].Tick(frameTime, _state, _parallel);

            // Update tick history.
            foreach (var load in _state.Loads.Values)
            {
                var displayLoad = _displayLoads[load.Id];
                displayLoad.ReceivedPowerData[_tickDataIdx] = load.ReceivingPower;
            }

            foreach (var supply in _state.Supplies.Values)
            {
                var displaySupply = _displaySupplies[supply.Id];
                displaySupply.SuppliedPowerData[_tickDataIdx] = supply.CurrentSupply;
            }

            foreach (var battery in _state.Batteries.Values)
            {
                var displayBattery = _displayBatteries[battery.Id];
                displayBattery.StoredPowerData[_tickDataIdx] = battery.CurrentStorage;
                displayBattery.ReceivingPowerData[_tickDataIdx] = battery.CurrentReceiving;
                displayBattery.SuppliedPowerData[_tickDataIdx] = battery.CurrentSupply;
            }

            _simTickTimes[_tickDataIdx] = (float) _simStopwatch.Elapsed.TotalMilliseconds;
        }

        private void RunSingleStep()
        {
            RunSingleStep(1f/_tps);
        }

        // Link data is stored authoritatively on networks,
        // but for easy access it is replicated into the linked components.
        // This is updated here.
        private void RefreshLinks()
        {
            foreach (var battery in _state.Batteries.Values)
            {
                battery.LinkedNetworkCharging = default;
                battery.LinkedNetworkDischarging = default;
            }

            foreach (var load in _state.Loads.Values)
            {
                load.LinkedNetwork = default;
            }

            foreach (var supply in _state.Supplies.Values)
            {
                supply.LinkedNetwork = default;
            }

            foreach (var network in _state.Networks.Values)
            {
                foreach (var loadId in network.Loads)
                {
                    var load = _state.Loads[loadId];
                    load.LinkedNetwork = network.Id;
                }

                foreach (var supplyId in network.Supplies)
                {
                    var supply = _state.Supplies[supplyId];
                    supply.LinkedNetwork = network.Id;
                }

                foreach (var batteryId in network.BatteryLoads)
                {
                    var battery = _state.Batteries[batteryId];
                    battery.LinkedNetworkCharging = network.Id;
                }

                foreach (var batteryId in network.BatterySupplies)
                {
                    var battery = _state.Batteries[batteryId];
                    battery.LinkedNetworkDischarging = network.Id;
                }
            }
        }

        /// <summary>
        /// Pow3r only needs deterministic single-threaded execution for solver previews.
        /// </summary>
        private sealed class SerialParallelManager : IParallelManager
        {
            public event Action ParallelCountChanged { add { } remove { } }
            public int ParallelProcessCount => 1;

            public void AddAndInvokeParallelCountChanged(Action changed)
            {
                return;
            }

            public WaitHandle Process(IRobustJob job)
            {
                job.Execute();
                var ev = new ManualResetEventSlim();
                ev.Set();
                return ev.WaitHandle;
            }

            public void ProcessNow(IRobustJob job)
            {
                job.Execute();
            }

            public void ProcessNow(IParallelRobustJob jobs, int amount)
            {
                for (var i = 0; i < amount; i++)
                {
                    jobs.Execute(i);
                }
            }

            public void ProcessSerialNow(IParallelRobustJob jobs, int amount)
            {
                for (var i = 0; i < amount; i++)
                {
                    jobs.Execute(i);
                }
            }

            public WaitHandle Process(IParallelRobustJob jobs, int amount)
            {
                ProcessSerialNow(jobs, amount);
                var ev = new ManualResetEventSlim();
                ev.Set();
                return ev.WaitHandle;
            }

            public void ProcessNow(IParallelBulkRobustJob jobs, int amount)
            {
                jobs.ExecuteRange(0, amount);
            }

            public void ProcessSerialNow(IParallelBulkRobustJob jobs, int amount)
            {
                jobs.ExecuteRange(0, amount);
            }

            public WaitHandle Process(IParallelBulkRobustJob jobs, int amount)
            {
                ProcessSerialNow(jobs, amount);
                var ev = new ManualResetEventSlim();
                ev.Set();
                return ev.WaitHandle;
            }
        }

    }
}
