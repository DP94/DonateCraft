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
}