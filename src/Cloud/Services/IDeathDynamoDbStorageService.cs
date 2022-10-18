﻿using Common.Models;

namespace Cloud.Services;

public interface IDeathDynamoDbStorageService
{
    Task<List<Death>> GetDeaths();
    Task<Death?> GetDeathById(string id);

    Task<Death> CreateDeath(Death death);

    Task DeleteDeath(string id);

    Task<Death> UpdateDeath(Death death);
}