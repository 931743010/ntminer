﻿using LiteDB;
using NTMiner.ServiceContracts.DataObjects;

namespace NTMiner.Data.Impl {
    public class UserData : IUser {
        public UserData() { }

        [BsonId(autoId: true)]
        public ObjectId Id { get; set; }

        public string LoginName { get; set; }

        public string Password { get; set; }

        public string Description { get; set; }
    }
}
