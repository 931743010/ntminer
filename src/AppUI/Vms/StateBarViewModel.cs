﻿using NTMiner.Views.Ucs;
using System;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class StateBarViewModel : ViewModelBase {
        public static readonly StateBarViewModel Current = new StateBarViewModel();

        public ICommand ConfigMinerServerHost { get; private set; }

        private StateBarViewModel() {
            this.ConfigMinerServerHost = new DelegateCommand(() => {
                MinerServerHostConfig.ShowWindow();
            });
        }

        public Version CurrentVersion {
            get {
                return NTMinerRoot.CurrentVersion;
            }
        }

        public string VersionTag {
            get {
                return NTMinerRoot.CurrentVersionTag;
            }
        }

        public string QQGroup {
            get {
                return NTMinerRoot.Current.QQGroup;
            }
        }

        public MinerProfileViewModel MinerProfile {
            get {
                return MinerProfileViewModel.Current;
            }
        }

        public GpuStatusBarViewModel GpuStatusBarVm {
            get {
                return GpuStatusBarViewModel.Current;
            }
        }
    }
}
