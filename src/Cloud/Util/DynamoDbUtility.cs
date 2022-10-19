using System.Globalization;
using System.Security.Cryptography;
using Amazon.DynamoDBv2.Model;
using Common.Models;
using Common.Util;

namespace Cloud.Util;

public static class DynamoDbUtility
{
    public static Dictionary<string, AttributeValue> GetAttributesFromPlayer(Player player)
    {
        var attributeValues = new Dictionary<string, AttributeValue>();
        attributeValues.TryAdd(DynamoDbConstants.PlayerIdColName, new AttributeValue(player.Id));
        attributeValues.TryAdd(DynamoDbConstants.PlayerNameColName, new AttributeValue(player.Name));
        return attributeValues;
    }
    
    public static Dictionary<string, AttributeValue> GetAttributesFromDeath(Death death)
    {
        var attributeValues = new Dictionary<string, AttributeValue>();
        attributeValues.TryAdd(DynamoDbConstants.DeathIdColName, new AttributeValue(death.Id));
        attributeValues.TryAdd(DynamoDbConstants.DeathPlayerIdColName, new AttributeValue(death.PlayerId));
        attributeValues.TryAdd(DynamoDbConstants.DeathReasonColName, new AttributeValue(death.Reason));
        attributeValues.TryAdd(DynamoDbConstants.DeathPlayerNameColName, new AttributeValue(death.PlayerName));
        attributeValues.TryAdd(DynamoDbConstants.DeathCreatedDateColName, new AttributeValue(death.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")));
        return attributeValues;
    }
    
    public static Dictionary<string, AttributeValue> GetAttributesFromLock(Lock theLock)
    {
        var attributeValues = new Dictionary<string, AttributeValue>();
        attributeValues.TryAdd(DynamoDbConstants.LockIdColName, new AttributeValue(theLock.Id));
        attributeValues.TryAdd(DynamoDbConstants.LockKeyColName, new AttributeValue(theLock.Key));
        attributeValues.TryAdd(DynamoDbConstants.LockUnlockedColName, new AttributeValue {BOOL = theLock.Unlocked});
        return attributeValues;
    }

    public static Player GetPlayerFromAttributes(Dictionary<string, AttributeValue> attributeValues)
    {
        var player = new Player();

        if (attributeValues.TryGetValue(DynamoDbConstants.PlayerIdColName, out var id) &&
            attributeValues.TryGetValue(DynamoDbConstants.PlayerNameColName, out var name))
        {
            player.Id = id.S;
            player.Name = name.S;
        }
            
        return player;
    }
    
    public static Death GetDeathFromAttributes(Dictionary<string, AttributeValue> attributeValues)
    {
        var death = new Death();

        if (attributeValues.TryGetValue(DynamoDbConstants.DeathIdColName, out var id) &&
            attributeValues.TryGetValue(DynamoDbConstants.DeathPlayerIdColName, out var playerId)
            && attributeValues.TryGetValue(DynamoDbConstants.DeathReasonColName, out var reason)
            && attributeValues.TryGetValue(DynamoDbConstants.DeathPlayerNameColName, out var playerName)
            && attributeValues.TryGetValue(DynamoDbConstants.DeathCreatedDateColName, out var createdDate))
        {
            death.Id = id.S;
            death.PlayerId = playerId.S;
            death.Reason = reason.S;
            death.PlayerName = playerName.S;
            death.CreatedDate = DateTime.ParseExact(createdDate.S, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
            
        return death;
    }
    
    public static Lock GetLockFromAttributes(Dictionary<string, AttributeValue> attributeValues)
    {
        var newLock = new Lock();

        if (attributeValues.TryGetValue(DynamoDbConstants.LockIdColName, out var id) &&
            attributeValues.TryGetValue(DynamoDbConstants.LockKeyColName, out var key) &&
            attributeValues.TryGetValue(DynamoDbConstants.LockUnlockedColName, out var unlocked))
        {
            newLock.Id = id.S;
            newLock.Key = key.S;
            newLock.Unlocked = unlocked.BOOL;
        }
            
        return newLock;
    }
}