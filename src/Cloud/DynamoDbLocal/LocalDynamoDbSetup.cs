using System.Diagnostics;
using System.Reflection;
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
        var client = this.GetClient();
        await CreatePlayerTable(client);
        await CreateDeathTable(client);
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

    private static async Task CreatePlayerTable(IAmazonDynamoDB client)
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
    
    private static async Task CreateDeathTable(IAmazonDynamoDB client)
    {
        await client.CreateTableAsync(new CreateTableRequest
        {
            TableName = DynamoDbConstants.DeathTableName,
            KeySchema = new List<KeySchemaElement> { new(DynamoDbConstants.DeathIdColName, KeyType.HASH) },
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new(DynamoDbConstants.DeathIdColName, ScalarAttributeType.S),
                new(DynamoDbConstants.DeathPlayerIdColName, ScalarAttributeType.S)
            },
            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new()
                {
                    IndexName = DynamoDbConstants.DeathPlayerIdColName,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new()
                        {
                            AttributeName = DynamoDbConstants.DeathPlayerIdColName,
                            KeyType = KeyType.HASH
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput(100, 100),
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                }
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