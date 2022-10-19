using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.TestUtilities;
using Common.Models;
using Core.Services;
using FakeItEasy;
using NUnit.Framework;

namespace DeathLambda.Test;

public class DeathLambdaTest
{
    private DeathLambda _lambda;
    private ILockService _lockService;

    [SetUp]
    public void SetUp()
    {
        this._lockService = A.Fake<ILockService>();
        this._lambda = new DeathLambda(this._lockService);
    }

    [Test]
    public async Task Lambda_CreatesLock_Successfully_ForDeathInsert_Event()
    {
        await this._lambda.HandleRequest(CreateDDBEvent(OperationType.INSERT, new StreamRecord
        {
            NewImage = new Dictionary<string, AttributeValue>
            {
                {
                    "playerId", new AttributeValue("test")
                }
            }
        }), new TestLambdaContext());
        A.CallTo(() => this._lockService.Create(A<Lock>.That.Matches(l => l.Unlocked == false && l.Key == "test")))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Lambda_DoesNot_CreateLock_IfPlayerIdMissing()
    {
        await this._lambda.HandleRequest(CreateDDBEvent(OperationType.INSERT, new StreamRecord
        {
            NewImage = new Dictionary<string, AttributeValue>()
        }), new TestLambdaContext());
        A.CallTo(() => this._lockService.Create(A<Lock>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public async Task Lambda_DoesNot_Process_ModifyEvent()
    {
        await this._lambda.HandleRequest(CreateDDBEvent(OperationType.MODIFY, null), new TestLambdaContext());
        A.CallTo(() => this._lockService.Create(A<Lock>.Ignored)).MustNotHaveHappened();
    }
    
    [Test]
    public async Task Lambda_DoesNot_Process_RemoveEvent()
    {
        await this._lambda.HandleRequest(CreateDDBEvent(OperationType.REMOVE, null), new TestLambdaContext());
        A.CallTo(() => this._lockService.Create(A<Lock>.Ignored)).MustNotHaveHappened();
    }

    private static DynamoDBEvent CreateDDBEvent(OperationType eventType, StreamRecord record)
    {
        return new DynamoDBEvent
        {
            Records = new List<DynamoDBEvent.DynamodbStreamRecord>
            {
                new()
                {
                    EventName = eventType,
                    Dynamodb = record
                }
            }
        };
    }
}