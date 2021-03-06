﻿using System;
using System.Diagnostics;
using System.IO;

namespace NTMiner {
    public static class ClientId {        
        public static string PublicKey { get; private set; }

        public static string PrivateKey { get; private set; }

        public static readonly string AppFileFullName = Process.GetCurrentProcess().MainModule.FileName;
        public static Guid Id { get; private set; }

        public static string GlobalDirFullName { get; private set; }

        public static string LangDbFileFullName { get; private set; }

        public static string LocalLangJsonFileFullName { get; private set; }

        public static string ServerLangJsonFileFullName { get; private set; }

        static ClientId() {
            GlobalDirFullName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NTMiner");
            if (!Directory.Exists(GlobalDirFullName)) {
                Directory.CreateDirectory(GlobalDirFullName);
            }
            LangDbFileFullName = Path.Combine(GlobalDirFullName, "lang.litedb");
            LocalLangJsonFileFullName = Path.Combine(GlobalDirFullName, "lang.json");
            ServerLangJsonFileFullName = Path.Combine(GlobalDirFullName, AssemblyInfo.ServerLangJsonFileName);

            string publicKey, privateKey;
            NTMinerRegistry.GetKeyPair(out publicKey, out privateKey);
            PublicKey = publicKey;
            PrivateKey = privateKey;
            Id = NTMinerRegistry.GetClientId();
        }
    }
}
