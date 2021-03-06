﻿using System;
using System.ServiceModel;

namespace NTMiner {
    [ServiceContract]
    public interface INTMinerDaemonService : IDisposable {
        [OperationContract]
        string GetDaemonVersion();

        [OperationContract]
        void ShutdownWindows();

        [OperationContract]
        void RestartWindows();

        [OperationContract]
        void OpenNTMiner(Guid workId);

        [OperationContract]
        void RestartNTMiner(Guid workId);

        [OperationContract]
        void CloseNTMiner();

        [OperationContract]
        bool IsNTMinerDaemonOnline();

        [OperationContract]
        bool IsNTMinerOnline();

        [OperationContract]
        void StartMine(
            int contextId,
            string minerName,
            string coin,
            string ourWallet,
            string testWallet,
            string kernelName);

        [OperationContract]
        void StopMine();
    }
}
