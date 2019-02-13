﻿using NTMiner.Views.Ucs;
using System;
using System.Windows;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class ConsoleViewModel : ViewModelBase {
        public static readonly ConsoleViewModel Current = new ConsoleViewModel();

        public ICommand CustomTheme { get; private set; }
        private ConsoleViewModel() {
            this.CustomTheme = new DelegateCommand(() => {
                LogColor.ShowWindow();
            });
            VirtualRoot.Access<MineStartedEvent>(
                Guid.Parse("47BA4DAB-5EDD-4BDF-A40B-B5B339DB0B78"),
                "挖矿开始后因此日志窗口的水印",
                LogEnum.Console,
                action: message => {
                    this.IsWatermarkVisible = Visibility.Collapsed;
                });
        }

        private Visibility _isWatermarkVisible = Visibility.Visible;
        public Visibility IsWatermarkVisible {
            get {
                return _isWatermarkVisible;
            }
            set {
                if (_isWatermarkVisible != value) {
                    _isWatermarkVisible = value;
                    OnPropertyChanged(nameof(IsWatermarkVisible));
                }
            }
        }

        public MinerProfileViewModel MinerProfile {
            get {
                return MinerProfileViewModel.Current;
            }
        }
    }
}