using Core.Services.Lock;
using FakeItEasy;
using Web.Controllers;
using Lock = Common.Models.Lock;

namespace Web.Test.Controllers.Sorting;

public class LockControllerSortingTest : AbstractControllerSortingTest<Lock, LockController, ILockService>
{

    protected override LockController CreateController(ILockService service)
    {
        return new LockController(service);
    }

    protected override ILockService CreateServiceFake()
    {
        this._service = A.Fake<ILockService>();
        return this._service;
    }

    protected override List<Lock> CreateData()
    {
        return new List<Lock>
        {
            new("abc", false),
            new("ghi", false)
        };
    }
}