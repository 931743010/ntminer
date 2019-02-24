﻿using NTMiner.MinerServer;
using System.Collections;
using System.Collections.Generic;

namespace NTMiner.Core.MinerServer.Impl {
    public class AppSettingSet : IAppSettingSet {
        private Dictionary<string, AppSettingData> _dicByKey = new Dictionary<string, AppSettingData>();
        private readonly INTMinerRoot _root;
        public AppSettingSet(INTMinerRoot root) {
            _root = root;
            VirtualRoot.Accept<ChangeAppSettingCommand>(
                "处理设置AppSetting命令",
                LogEnum.Console,
                action: message => {
                    if (message.AppSetting == null) {
                        return;
                    }
                    AppSettingData entity = AppSettingData.Create(message.AppSetting);
                    Server.AppSettingService.SetAppSettingAsync(entity, response => {
                        if (response.IsSuccess()) {
                            AppSettingData item;
                            if (_dicByKey.TryGetValue(message.AppSetting.Key, out item)) {
                                item.Value = message.AppSetting.Value;
                            }
                            else {
                                _dicByKey.Add(message.AppSetting.Key, entity);
                            }
                            VirtualRoot.Happened(new AppSettingChangedEvent(entity));
                        }
                        else if (response != null) {
                            Write.UserLine(response.Description, System.ConsoleColor.Red);
                        }
                    });
                });
        }

        private bool _isInited = false;
        private object _locker = new object();

        private void InitOnece() {
            if (_isInited) {
                return;
            }
            Init();
        }

        private void Init() {
            lock (_locker) {
                if (!_isInited) {
                    var list = Server.AppSettingService.GetAppSettings();
                    foreach (var item in list) {
                        _dicByKey.Add(item.Key, item);
                    }
                    _isInited = true;
                }
            }
        }

        public IAppSetting this[string key] {
            get {
                InitOnece();
                AppSettingData data;
                _dicByKey.TryGetValue(key, out data);
                return data;
            }
        }

        public IEnumerator<IAppSetting> GetEnumerator() {
            InitOnece();
            foreach (var item in _dicByKey.Values) {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            InitOnece();
            foreach (var item in _dicByKey.Values) {
                yield return item;
            }
        }
    }
}