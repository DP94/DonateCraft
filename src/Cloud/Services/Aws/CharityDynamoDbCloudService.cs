using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.Util;
using Common.Exceptions;
using Common.Models;
using Common.Util;
using ResourceNotFoundException = Common.Exceptions.ResourceNotFoundException;

namespace Cloud.Services.Aws;

public class CharityDynamoDbCloudService : ICharityCloudService
{
    private readonly IAmazonDynamoDB _dynamoDb;

    public CharityDynamoDbCloudService(IAmazonDynamoDB dynamoDb)
    {
        this._dynamoDb = dynamoDb;
    }

    public async Task<Charity> GetCharityById(string id)
    {
        var response = await this._dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbConstants.CharityTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.CharityIdColName, new AttributeValue(id)
                }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
        {
            throw new ResourceNotFoundException($"Death {id} not found");
        }

        return DynamoDbUtility.GetCharityFromAttributes(response.Item);
    }

    public async Task<List<Charity>> GetCharities()
    {
        var result = await this._dynamoDb.ScanAsync(new ScanRequest(DynamoDbConstants.CharityTableName));
        return result.Items.Select(DynamoDbUtility.GetCharityFromAttributes).ToList();
    }

    public async Task DeleteCharity(string id)
    {
        await this._dynamoDb.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = DynamoDbConstants.CharityTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.CharityIdColName, new AttributeValue(id)
                }
            }
        });
    }

    public async Task<Charity> CreateCharity(Charity charity)
    {
        try
        {
            await this._dynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = DynamoDbConstants.CharityTableName,
                Item = DynamoDbUtility.GetAttributesFromCharity(charity),
                ConditionExpression = $"attribute_not_exists({DynamoDbConstants.CharityIdColName})"
            });
        }
        catch (ConditionalCheckFailedException e)
        {
            throw new ResourceExistsException($"Charity with id {charity.Id} already exists!");
        }

        return charity;
    }

    public async Task<Charity> UpdateCharity(Charity charity)
    {
        await this.GetCharityById(charity.Id);
        await this._dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = DynamoDbConstants.CharityTableName,
            Item = DynamoDbUtility.GetAttributesFromCharity(charity)
        });
        return charity;
    }
}