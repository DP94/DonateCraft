using Common.Models;
using Core.Services.Death;
using FakeItEasy;
using Web.Controllers;

namespace Web.Test.Controllers;

public class DeathControllerTest : AbstractWithPlayerIdControllerTest<DeathController, Death, IDeathService>
{

    protected override IDeathService CreateServiceFake()
    {
        return A.Fake<IDeathService>();
    }

    protected override DeathController CreateController(IDeathService service)
    {
        return new DeathController(service);
    }

    protected override Death CreateData()
    {
        return new Death(Guid.NewGuid().ToString(), "unit tests");
    }

}