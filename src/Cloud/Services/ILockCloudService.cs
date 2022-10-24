﻿using Common.Models;

namespace Cloud.Services;

public interface ILockCloudService
{
    Task<Lock> Create(Lock newLock);
    Task<List<Lock>> GetLocks();
    Task<Lock> GetLock(string id);
    Task DeleteLock(string id);
    Task<Lock> UpdateLock(Lock theLock);
    Task<Lock> GetLockByKey(string key);
}