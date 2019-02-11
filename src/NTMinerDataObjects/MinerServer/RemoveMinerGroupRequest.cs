﻿using System;
using System.Text;

namespace NTMiner.MinerServer {
    public class RemoveMinerGroupRequest : RequestBase, ISignatureRequest {
        public RemoveMinerGroupRequest() { }
        public string LoginName { get; set; }
        public Guid MinerGroupId { get; set; }
        public string Sign { get; set; }

        public void SignIt(string password) {
            this.Sign = this.GetSign(password);
        }

        public string GetSign(string password) {
            StringBuilder sb = new StringBuilder();
            sb.Append(nameof(MessageId)).Append(MessageId)
                .Append(nameof(LoginName)).Append(LoginName)
                .Append(nameof(MinerGroupId)).Append(MinerGroupId)
                .Append(nameof(Timestamp)).Append(Timestamp.ToUlong())
                .Append(nameof(UserData.Password)).Append(password);
            return HashUtil.Sha1(sb.ToString());
        }
    }
}