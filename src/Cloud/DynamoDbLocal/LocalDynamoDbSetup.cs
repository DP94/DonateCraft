using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Common.Util;

namespace Cloud.DynamoDbLocal;

public class LocalDynamoDbSetup : IDisposable
{
    private Process _process;
    private readonly string URL;

    public LocalDynamoDbSetup()
    {
        this.URL = "http://localhost:8000";
    }

    public async Task SetupDynamoDb()
    {
        this._process = this.StartDynamoProcess();
    }

    public async Task CreateTables(string playerTableName, string lockTableName, string charityTableName)
    {
        var client = GetClient();
        try
        {
            if (!string.IsNullOrWhiteSpace(playerTableName))
            {
                await CreatePlayerTable(client);
            }
            if (!string.IsNullOrWhiteSpace(lockTableName))
            {
                await CreateLockTable(client);
            }
            if (!string.IsNullOrWhiteSpace(charityTableName))
            {
                await CreateCharityTable(client);
            }
        } catch (ResourceInUseException)
        {
          //DDB is already setup   
        }

    }
    
    public async Task ClearTables(string playerTableName, string lockTableName, string charityTableName)
    {
        if (!string.IsNullOrWhiteSpace(playerTableName))
        {
            await this.GetClient().DeleteTableAsync(DynamoDbConstants.PlayerTableName);
        }
        if (!string.IsNullOrWhiteSpace(lockTableName))
        {
            await this.GetClient().DeleteTableAsync(DynamoDbConstants.LockTableName);
        }
        if (!string.IsNullOrWhiteSpace(charityTableName))
        {
            await this.GetClient().DeleteTableAsync(DynamoDbConstants.CharityTableName);
        }
        await this.CreateTables(playerTableName, lockTableName, charityTableName);
    }

    public void KillProcess()
    {
        this._process?.Kill();
    }

    public AmazonDynamoDBClient GetClient()
    {
        var config = new AmazonDynamoDBConfig
        {
            ServiceURL = this.URL
        };
        var credentials = new BasicAWSCredentials("x", "x");
        return new AmazonDynamoDBClient(credentials, config);
    }

    private async static Task CreatePlayerTable(IAmazonDynamoDB client)
    {
        await client.CreateTableAsync(new CreateTableRequest(
            DynamoDbConstants.PlayerTableName,
            new List<KeySchemaElement> { new(DynamoDbConstants.PlayerIdColName, KeyType.HASH) },
            new List<AttributeDefinition>
            {
                new(DynamoDbConstants.PlayerIdColName, ScalarAttributeType.S)
            },
            new ProvisionedThroughput(100, 100)));
    }

    private async static Task CreateLockTable(IAmazonDynamoDB client)
    {
        await client.CreateTableAsync(new CreateTableRequest
        {
            TableName = DynamoDbConstants.LockTableName,
            KeySchema = new List<KeySchemaElement> { new(DynamoDbConstants.LockIdColName, KeyType.HASH) },
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new(DynamoDbConstants.LockIdColName, ScalarAttributeType.S),
            },
            ProvisionedThroughput = new ProvisionedThroughput(100, 100),
        });
    }
    
    private async static Task CreateCharityTable(IAmazonDynamoDB client)
    {
        await client.CreateTableAsync(new CreateTableRequest
        {
            TableName = DynamoDbConstants.CharityTableName,
            KeySchema = new List<KeySchemaElement> { new(DynamoDbConstants.CharityIdColName, KeyType.HASH) },
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new(DynamoDbConstants.CharityIdColName, ScalarAttributeType.S)
            },
            ProvisionedThroughput = new ProvisionedThroughput(100, 100)
        });
    }

    private Process StartDynamoProcess()
    {
        var dir = Path.GetDirectoryName(
            Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
        var process = new Process
        {
            StartInfo = new ProcessStartInfo("java",
                "-Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar -inMemory -sharedDb -port 8000")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.Combine(dir, "DynamoDbLocal")
            }
        };
        process.ErrorDataReceived += (DataReceivedEventHandler)((sender, args) => { Console.WriteLine(args.Data); });
        process.OutputDataReceived += (DataReceivedEventHandler)((sender, args) => { Console.WriteLine(args.Data); });
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    public void Dispose()
    {
        _process.Dispose();
    }
}