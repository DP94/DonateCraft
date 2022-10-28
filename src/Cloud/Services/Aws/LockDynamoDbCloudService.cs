using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.Util;
using Common.Exceptions;
using Common.Models;
using Common.Util;
using Microsoft.Extensions.Caching.Memory;
using ResourceNotFoundException = Common.Exceptions.ResourceNotFoundException;

namespace Cloud.Services.Aws;

public class LockDynamoDbCloudService : ILockCloudService
{

    private readonly IAmazonDynamoDB _amazonDynamoDb;
    private readonly IDonationCloudService _donationCloudService;
    private readonly IPlayerCloudService _playerCloudService;
    private readonly IMemoryCache _cache;

    public LockDynamoDbCloudService(IAmazonDynamoDB amazonDynamoDb, IDonationCloudService donationCloudService, IPlayerCloudService playerCloudService, IMemoryCache cache)
    {
        this._amazonDynamoDb = amazonDynamoDb;
        this._donationCloudService = donationCloudService;
        this._playerCloudService = playerCloudService;
        this._cache = cache;
    }

    public async Task<Lock> Create(Lock newLock)
    {
        try
        {
            await this._amazonDynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = DynamoDbConstants.LockTableName,
                Item = DynamoDbUtility.GetAttributesFromLock(newLock),
                ConditionExpression = $"attribute_not_exists({DynamoDbConstants.LockIdColName})"
            });
        }
        catch (ConditionalCheckFailedException e)
        {
            throw new ResourceExistsException($"Lock with id {newLock.Id} already exists!");    
        }

        return newLock;
    }

    public async Task<List<Lock>> GetLocks()
    {
        var result = await this._amazonDynamoDb.ScanAsync(new ScanRequest(DynamoDbConstants.LockTableName));
        return result.Items.Select(DynamoDbUtility.GetLockFromAttributes).ToList();
    }

    public async Task<List<Lock>> GetLocksForPlayers(List<string> playerIds)
    {
        if (playerIds is { Count: 0 })
        {
            return new List<Lock>();
        }
        
        var request = new BatchGetItemRequest
        {
            RequestItems = new Dictionary<string, KeysAndAttributes>
            {
                {
                    DynamoDbConstants.LockTableName,
                    new KeysAndAttributes
                    {
                        Keys = new List<Dictionary<string, AttributeValue>>()
                    }
                }
            }
        };
        playerIds.ForEach(id =>
        {
            request.RequestItems[DynamoDbConstants.LockTableName].Keys.Add(new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.LockIdColName, new AttributeValue(id)
                }
            });
        });
        var locks = new List<Lock>();
        var response = await this._amazonDynamoDb.BatchGetItemAsync(request);
        if (response != null && response.Responses.TryGetValue(DynamoDbConstants.LockTableName, out var ddbLocks))
        {
            foreach (var aLock in ddbLocks)
            {
                var newLock = DynamoDbUtility.GetLockFromAttributes(aLock);
                var donationId = newLock.DonationId;
                if (!string.IsNullOrWhiteSpace(donationId))
                {
                    var donation = await this._donationCloudService.GetDonation(newLock.Id, donationId);
                    var paidBy = await this._cache.GetOrCreateAsync(donation.PaidForId, _ => this._playerCloudService.GetPlayerById(donation.PaidForId, new DynamoAttributeMappingCriteria(false, false)));
                    donation.PaidForBy = paidBy;
                    newLock.Donation = donation;
                }
                locks.Add(newLock);
            }
        }
        return locks;
    }

    public async Task<Lock> GetLock(string id)
    {
        var response = await this._amazonDynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbConstants.LockTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.LockIdColName, new AttributeValue(id)
                }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
        {
            throw new ResourceNotFoundException($"Lock {id} not found");
        }

        return DynamoDbUtility.GetLockFromAttributes(response.Item);
    }

    public async Task DeleteLock(string id)
    {
        await this._amazonDynamoDb.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = DynamoDbConstants.LockTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.LockIdColName, new AttributeValue(id)
                }
            }
        });
    }

    public async Task<Lock> UpdateLock(Lock theLock)
    {
        await this.GetLock(theLock.Id);
        await this._amazonDynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = DynamoDbConstants.LockTableName,
            Item = DynamoDbUtility.GetAttributesFromLock(theLock)
        });
        return theLock;
    }
}