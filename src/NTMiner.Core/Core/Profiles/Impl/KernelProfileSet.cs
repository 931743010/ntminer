﻿using NTMiner.Core.Kernels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NTMiner.Core.Profiles.Impl {
    public class KernelProfileSet : IKernelProfileSet {
        private readonly Dictionary<Guid, KernelProfile> _dicByKernelId = new Dictionary<Guid, KernelProfile>();

        private readonly INTMinerRoot _root;
        private readonly object _locker = new object();
        public KernelProfileSet(INTMinerRoot root) {
            _root = root;
            Global.Logger.InfoDebugLine(this.GetType().FullName + "接入总线");
        }

        public IKernelProfile EmptyKernelProfile {
            get {
                return KernelProfile.Empty;
            }
        }

        public IKernelProfile GetKernelProfile(Guid kernelId) {
            if (_dicByKernelId.ContainsKey(kernelId)) {
                return _dicByKernelId[kernelId];
            }
            lock (_locker) {
                if (_dicByKernelId.ContainsKey(kernelId)) {
                    return _dicByKernelId[kernelId];
                }
                var kernelProfile = new KernelProfile() {
                    KernelId = kernelId
                };
                _dicByKernelId.Add(kernelId, kernelProfile);
                return kernelProfile;
            }
        }

        private class KernelProfile : IKernelProfile {
            public static readonly KernelProfile Empty = new KernelProfile();

            public KernelProfile() { }

            public Guid KernelId { get; set; }

            private IKernel _kernel;
            public IKernel Kernel {
                get {
                    if (_kernel == null) {
                        NTMinerRoot.Current.KernelSet.TryGetKernel(this.KernelId, out _kernel);
                    }
                    return _kernel;
                }
            }

            public InstallStatus InstallStatus {
                get {
                    if (this.KernelId == Guid.Empty || this.Kernel == null) {
                        return InstallStatus.Installed;
                    }
                    string packageFullName = this.Kernel.GetPackageFileFullName();
                    if (File.Exists(packageFullName)) {
                        return InstallStatus.Installed;
                    }
                    List<string> packageHistoryFileFullNames = this.Kernel.GetPackageHistoryFileFullNames();
                    if (packageHistoryFileFullNames.Any(a => File.Exists(a))) {
                        return InstallStatus.CanUpdate;
                    }
                    return InstallStatus.Uninstalled;
                }
            }
        }
    }
}
