using Common.Models;
using Core.Services.Lock;
using FakeItEasy;
using Web.Controllers;

namespace Web.Test.Controllers;

public class LockControllerTest : AbstractControllerTest<LockController, Lock, ILockService>
{
    protected override ILockService CreateServiceFake()
    {
        return A.Fake<ILockService>();
    }

    protected override LockController CreateController(ILockService service)
    {
        return new LockController(service);
    }

    protected override Lock CreateData()
    {
        return new Lock("test", false);
    }

}