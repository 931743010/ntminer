﻿namespace NTMiner.User {
    public interface IUserSet {
        bool Contains(string pubKey);
        bool TryGetKey(string pubKey, out IUser key);
    }
}