﻿using Microsoft.Win32;
using NTMiner.ServiceContracts.DataObjects;
using NTMiner.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class MainWindowViewModel : ViewModelBase {
        public static readonly MainWindowViewModel Current = new MainWindowViewModel();

        private double _downloadPercent;
        private bool _isDownloading = false;
        private NTMinerFileViewModel _selectedNTMinerFile;
        private NTMinerFileViewModel _serverLatestVm;
        private bool _isReady;
        private bool _localIsLatest;

        private List<NTMinerFileViewModel> _nTMinerFiles;
        private Visibility _isHistoryVisible = Visibility.Collapsed;

        private string _downloadMessage;
        private Visibility _btnCancelVisible = Visibility.Visible;

        private static readonly string _ntminerDirFullName;
        private static readonly string _downloadDirFullName;

        static MainWindowViewModel() {
            _ntminerDirFullName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NTMiner");
            if (!Directory.Exists(_ntminerDirFullName)) {
                Directory.CreateDirectory(_ntminerDirFullName);
            }
            string tempDirFullName = Path.Combine(_ntminerDirFullName, "Temp");
            if (!Directory.Exists(tempDirFullName)) {
                Directory.CreateDirectory(tempDirFullName);
            }
            _downloadDirFullName = Path.Combine(tempDirFullName, "Download");
            if (!Directory.Exists(_downloadDirFullName)) {
                Directory.CreateDirectory(_downloadDirFullName);
            }
        }

        private Action cancel;
        public ICommand Install { get; private set; }
        public ICommand CancelDownload { get; private set; }
        public ICommand ShowHistory { get; private set; }
        public ICommand AddNTMinerFile { get; private set; }

        private MainWindowViewModel() {
            if (App.IsInDesignMode) {
                return;
            }
            this.Refresh();
            this.CancelDownload = new DelegateCommand(() => {
                this.cancel?.Invoke();
                this.IsDownloading = false;
            });
            this.Install = new DelegateCommand(() => {
                if (this.IsDownloading) {
                    return;
                }
                this.IsDownloading = true;
                string ntMinerFile = string.Empty;
                string version = string.Empty;
                if (IsHistoryVisible == Visibility.Collapsed) {
                    if (ServerLatestVm != null) {
                        ntMinerFile = ServerLatestVm.FileName;
                        version = ServerLatestVm.Version;
                    }
                }
                else {
                    ntMinerFile = SelectedNTMinerFile.FileName;
                    version = SelectedNTMinerFile.Version;
                }
                Download(ntMinerFile, version, progressChanged: (percent) => {
                    this.DownloadMessage = percent + "%";
                    this.DownloadPercent = (double)percent / 100;
                }, downloadComplete: (isSuccess, message, saveFileFullName) => {
                    this.DownloadMessage = message;
                    this.DownloadPercent = 0;
                    this.BtnCancelVisible = Visibility.Collapsed;
                    if (isSuccess) {
                        this.DownloadMessage = "更新成功，正在重启";
                        CloseNTMinerMainWindow();
                        TimeSpan.FromSeconds(2).Delay().ContinueWith((t) => {
                            string location = NTMinerRegistry.GetLocation();
                            if (string.IsNullOrEmpty(location) || !File.Exists(location)) {
                                location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ntMinerFile);
                            }
                            File.Copy(saveFileFullName, location, overwrite: true);
                            File.Delete(saveFileFullName);
                            string arguments = NTMinerRegistry.GetArguments();
                            Process.Start(location, arguments);
                            this.IsDownloading = false;
                            Execute.OnUIThread(() => {
                                Application.Current.MainWindow.Close();
                            });
                        });
                    }
                    else {
                        TimeSpan.FromSeconds(2).Delay().ContinueWith((t) => {
                            this.IsDownloading = false;
                        });
                    }
                }, cancel: out cancel);
            });
            this.ShowHistory = new DelegateCommand(() => {
                if (IsHistoryVisible == Visibility.Visible) {
                    IsHistoryVisible = Visibility.Collapsed;
                }
                else {
                    IsHistoryVisible = Visibility.Visible;
                }
            });
            this.AddNTMinerFile = new DelegateCommand(() => {
                NTMinerFileEdit window = new NTMinerFileEdit("添加", "Icon_Add", new NTMinerFileViewModel());
                window.ShowDialogEx();
            });
        }

        public static void Download(
            string fileName,
            string version,
            Action<int> progressChanged,
            Action<bool, string, string> downloadComplete,
            out Action cancel) {
            Global.Logger.InfoDebugLine("下载：" + fileName);
            string saveFileFullName = Path.Combine(_downloadDirFullName, "NTMiner" + version);
            progressChanged?.Invoke(0);
            using (WebClient webClient = new WebClient()) {
                cancel = () => {
                    webClient.CancelAsync();
                };
                webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => {
                    progressChanged?.Invoke(e.ProgressPercentage);
                };
                webClient.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) => {
                    bool isSuccess = !e.Cancelled && e.Error == null;
                    if (isSuccess) {
                        Global.Logger.OkDebugLine("NTMiner" + version + "下载成功");
                    }
                    string message = "下载成功";
                    if (e.Error != null) {
                        message = "下载失败";
                        Global.Logger.ErrorDebugLine(e.Error.Message, e.Error);
                    }
                    if (e.Cancelled) {
                        message = "下载取消";
                    }
                    downloadComplete?.Invoke(isSuccess, message, saveFileFullName);
                };
                Server.FileUrlService.GetNTMinerUrlAsync(fileName, url => {
                    webClient.DownloadFileAsync(new Uri(url), saveFileFullName);
                });
            }
        }

        public void Refresh() {
            Server.FileUrlService.GetNTMinerFilesAsync(ntMinerFiles => {
                this.NTMinerFiles = (ntMinerFiles ?? new List<NTMinerFileData>()).Select(a => new NTMinerFileViewModel(a)).OrderByDescending(a => a.VersionData).ToList();
                if (this.NTMinerFiles == null || this.NTMinerFiles.Count == 0) {
                    LocalIsLatest = true;
                }
                else {
                    ServerLatestVm = this.NTMinerFiles.OrderByDescending(a => a.VersionData).FirstOrDefault();
                    if (ServerLatestVm.VersionData > LocalNTMinerVersion) {
                        Global.Logger.WarnDebugLine("发现新版本" + ServerLatestVm.Version);
                        this.SelectedNTMinerFile = ServerLatestVm;
                        LocalIsLatest = false;
                    }
                    else {
                        LocalIsLatest = true;
                    }
                }
                OnPropertyChanged(nameof(IsBtnInstallVisible));
                IsReady = true;
            });
        }

        private static void CloseNTMinerMainWindow() {
            try {
                string location = NTMinerRegistry.GetLocation();
                if (!string.IsNullOrEmpty(location) && File.Exists(location)) {
                    string processName = Path.GetFileNameWithoutExtension(location);
                    Process[] processes = Process.GetProcessesByName(processName);
                    if (processes.Length != 0) {
                        Windows.TaskKill.Kill(processName);
                    }
                }
            }
            catch (Exception ex) {
                Global.Logger.ErrorDebugLine(ex.Message, ex);
            }
        }

        public Visibility IsDevModeVisible {
            get {
                if (DevMode.IsDevMode) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public bool LocalIsLatest {
            get => _localIsLatest;
            set {
                _localIsLatest = value;
                OnPropertyChanged(nameof(LocalIsLatest));
            }
        }

        public Visibility IsHistoryVisible {
            get { return _isHistoryVisible; }
            set {
                _isHistoryVisible = value;
                OnPropertyChanged(nameof(IsHistoryVisible));
                OnPropertyChanged(nameof(BtnShowHistoryText));
                OnPropertyChanged(nameof(IsBtnInstallVisible));
            }
        }

        public string BtnShowHistoryText {
            get {
                if (this.IsHistoryVisible == Visibility.Visible) {
                    return "最新版本";
                }
                return "历史版本";
            }
        }

        public Visibility IsBtnInstallVisible {
            get {
                if (IsHistoryVisible == Visibility.Collapsed && LocalIsLatest) {
                    return Visibility.Collapsed;
                }
                if (SelectedNTMinerFile != null) {
                    return Visibility.Visible;
                }
                else {
                    return Visibility.Collapsed;
                }
            }
        }

        private Version _localNTMinerVersion;
        public Version LocalNTMinerVersion {
            get {
                if (_localNTMinerVersion == null) {
                    string currentVersion = NTMinerRegistry.GetCurrentVersion();
                    if (!Version.TryParse(currentVersion, out _localNTMinerVersion)) {
                        _localNTMinerVersion = new Version(1, 0);
                    }
                }
                return _localNTMinerVersion;
            }
        }

        public string LocalNTMinerVersionTag {
            get {
                return NTMinerRegistry.GetCurrentVersionTag();
            }
        }

        public Visibility BtnCancelVisible {
            get => _btnCancelVisible;
            set {
                _btnCancelVisible = value;
                OnPropertyChanged(nameof(BtnCancelVisible));
            }
        }

        public bool IsDownloading {
            get { return _isDownloading; }
            set {
                _isDownloading = value;
                OnPropertyChanged(nameof(IsDownloading));
            }
        }

        public double DownloadPercent {
            get {
                return _downloadPercent;
            }
            set {
                _downloadPercent = value;
                OnPropertyChanged(nameof(DownloadPercent));
            }
        }

        public string DownloadMessage {
            get {
                return _downloadMessage;
            }
            set {
                _downloadMessage = value;
                OnPropertyChanged(nameof(DownloadMessage));
            }
        }

        public List<NTMinerFileViewModel> NTMinerFiles {
            get => _nTMinerFiles;
            set {
                _nTMinerFiles = value;
                OnPropertyChanged(nameof(NTMinerFiles));
            }
        }

        public NTMinerFileViewModel SelectedNTMinerFile {
            get => _selectedNTMinerFile;
            set {
                _selectedNTMinerFile = value;
                OnPropertyChanged(nameof(SelectedNTMinerFile));
                OnPropertyChanged(nameof(IsBtnInstallVisible));
            }
        }

        public NTMinerFileViewModel ServerLatestVm {
            get {
                return _serverLatestVm;
            }
            set {
                _serverLatestVm = value;
                OnPropertyChanged(nameof(ServerLatestVm));
            }
        }

        public bool IsReady {
            get => _isReady;
            set {
                _isReady = value;
                OnPropertyChanged(nameof(IsReady));
            }
        }
    }
}
