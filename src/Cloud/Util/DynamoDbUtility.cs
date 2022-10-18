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
}