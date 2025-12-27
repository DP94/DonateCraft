using Core.Services.Lock;
using FakeItEasy;
using Web.Controllers;
using Lock = Common.Models.Lock;

namespace Web.Test.Controllers;

public class LockWithIdControllerTest : AbstractWithIdControllerTest<LockController, Lock, ILockService>
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