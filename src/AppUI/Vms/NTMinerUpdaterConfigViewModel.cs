﻿using NTMiner.AppSetting;
using NTMiner.ServiceContracts.DataObjects;
using System;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class NTMinerUpdaterConfigViewModel : ViewModelBase {
        public ICommand Save { get; private set; }

        public Action CloseWindow { get; set; }

        public NTMinerUpdaterConfigViewModel() {
            this.Save = new DelegateCommand(() => {
                try {
                    if (string.IsNullOrEmpty(this.FileName)) {
                        this.FileName = "NTMinerUpdater.exe";
                    }
                    Global.Execute(new SetAppSettingCommand(new AppSettingData {
                        Key = "ntminerUpdaterFileName",
                        Value = this.FileName
                    }));
                    CloseWindow?.Invoke();
                }
                catch (Exception e) {
                    Global.Logger.ErrorDebugLine(e.Message, e);
                }
            });
            _fileName = "NTMinerUpdater.exe";
        }

        private string _fileName;
        public string FileName {
            get {
                return _fileName;
            }
            set {
                if (_fileName != value) {
                    _fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }
    }
}