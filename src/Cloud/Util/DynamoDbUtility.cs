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
        if (player.Deaths.Count <= 0)
        {
            return attributeValues;
        }
        
        var deathList = new AttributeValue
        {
            L = new List<AttributeValue>()
        };
        foreach (var death in player.Deaths)
        {
            death.PlayerId = player.Id;
            deathList.L.Add(new AttributeValue
            {
                M = GetAttributesFromDeath(death)
            });
        }
        attributeValues.TryAdd(DynamoDbConstants.PlayerDeathsColName, deathList);
        return attributeValues;
    }
    
    public static Dictionary<string, AttributeValue> GetAttributesFromDeath(Death death)
    {
        var attributeValues = new Dictionary<string, AttributeValue>();
        attributeValues.TryAdd(DynamoDbConstants.DeathIdColName, new AttributeValue(death.Id));
        attributeValues.TryAdd(DynamoDbConstants.DeathPlayerIdColName, new AttributeValue(death.PlayerId));
        attributeValues.TryAdd(DynamoDbConstants.DeathReasonColName, new AttributeValue(death.Reason));
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
    
    public static Dictionary<string, AttributeValue> GetAttributesFromCharity(Charity charity)
    {
        var attributeValues = new Dictionary<string, AttributeValue>();
        attributeValues.TryAdd(DynamoDbConstants.CharityIdColName, new AttributeValue(charity.Id));
        attributeValues.TryAdd(DynamoDbConstants.CharityNameColName, new AttributeValue(charity.Name));
        attributeValues.TryAdd(DynamoDbConstants.CharityDescriptionColName, new AttributeValue(charity.Description));
        attributeValues.TryAdd(DynamoDbConstants.CharityURLColName, new AttributeValue(charity.Url));
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

        if (!attributeValues.TryGetValue(DynamoDbConstants.PlayerDeathsColName, out var deaths))
        {
            return player;
        }
        
        foreach (var death in deaths.L.Select(deathAttributes => GetDeathFromAttributes(deathAttributes.M)))
        {
            death.PlayerId = player.Id;
            player.Deaths.Add(death);
        }

        return player;
    }
    
    public static Death GetDeathFromAttributes(Dictionary<string, AttributeValue> attributeValues)
    {
        var death = new Death();

        if (attributeValues.TryGetValue(DynamoDbConstants.DeathIdColName, out var id) &&
            attributeValues.TryGetValue(DynamoDbConstants.DeathReasonColName, out var reason) &&
            attributeValues.TryGetValue(DynamoDbConstants.DeathCreatedDateColName, out var createdDate))
        {
            death.Id = id.S;
            death.Reason = reason.S;
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
    
    public static Charity GetCharityFromAttributes(Dictionary<string, AttributeValue> attributeValues)
    {
        var charity = new Charity();

        if (attributeValues.TryGetValue(DynamoDbConstants.CharityIdColName, out var id) &&
            attributeValues.TryGetValue(DynamoDbConstants.CharityNameColName, out var name) &&
            attributeValues.TryGetValue(DynamoDbConstants.CharityDescriptionColName, out var description) &&
            attributeValues.TryGetValue(DynamoDbConstants.CharityURLColName, out var url))
        {
            charity.Id = id.S;
            charity.Description = description.S;
            charity.Name = name.S;
            charity.Url = url.S;
        }
            
        return charity;
    }
}