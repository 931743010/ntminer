﻿using System.Text;

namespace NTMiner.MinerServer {
    public class AddOrUpdateNTMinerFileRequest : RequestBase, ISignatureRequest {
        public AddOrUpdateNTMinerFileRequest() { }
        public string LoginName { get; set; }
        public NTMinerFileData Data { get; set; }
        public string Sign { get; set; }

        public void SignIt(string password) {
            this.Sign = this.GetSign(password);
        }

        public string GetSign(string password) {
            StringBuilder sb = new StringBuilder();
            sb.Append(nameof(MessageId)).Append(MessageId)
                .Append(nameof(LoginName)).Append(LoginName)
                .Append(Data.GetSignData())
                .Append(nameof(Timestamp)).Append(Timestamp.ToUlong())
                .Append(nameof(UserData.Password)).Append(password);
            return HashUtil.Sha1(sb.ToString());
        }
    }
}