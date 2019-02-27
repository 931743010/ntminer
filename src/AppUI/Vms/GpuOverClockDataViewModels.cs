﻿using NTMiner.Core;
using NTMiner.Core.Gpus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner.Vms {
    public class GpuOverClockDataViewModels : ViewModelBase {
        public static readonly GpuOverClockDataViewModels Current = new GpuOverClockDataViewModels();

        private readonly Dictionary<Guid, GpuOverClockDataViewModel> _dicById = new Dictionary<Guid, GpuOverClockDataViewModel>();
        private readonly Dictionary<Guid, GpuOverClockDataViewModel> _gpuAllVmDicByCoinId = new Dictionary<Guid, GpuOverClockDataViewModel>();

        private GpuOverClockDataViewModels() {
            VirtualRoot.On<GpuOverClockDataAddedOrUpdatedEvent>(
                "添加或更新了Gpu超频数据后刷新VM内存",
                LogEnum.Console,
                action: message => {
                    GpuOverClockDataViewModel vm;
                    if (_dicById.TryGetValue(message.Source.GetId(), out vm)) {
                        vm.Update(message.Source);
                    }
                    else {
                        _dicById.Add(message.Source.GetId(), new GpuOverClockDataViewModel(message.Source));
                    }
                });
            foreach (var coin in NTMinerRoot.Current.CoinSet) {
                Guid coinId = coin.GetId();
                foreach (var gpu in NTMinerRoot.Current.GpuSet) {
                    IGpuOverClockData data = NTMinerRoot.Current.GpuOverClockDataSet.GetGpuOverClockData(coinId, gpu.Index);
                    var vm = new GpuOverClockDataViewModel(data) {
                        Name = gpu.Name
                    };
                    _dicById.Add(data.GetId(), vm);
                    if (gpu.Index == NTMinerRoot.GpuAllId) {
                        _gpuAllVmDicByCoinId.Add(coinId, vm);
                    }
                }
            }
        }

        public GpuOverClockDataViewModel GpuAllVm(Guid coinId) {
            return _gpuAllVmDicByCoinId[coinId];
        }

        public List<GpuOverClockDataViewModel> List(Guid coinId) {
            return _dicById.Values.Where(a => a.CoinId == coinId).OrderBy(a => a.Index).ToList();
        }
    }
}