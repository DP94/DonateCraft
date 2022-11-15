using Common.Models;
using Core.Services.Donation;
using FakeItEasy;
using Web.Controllers;

namespace Web.Test.Controllers;

public class DonationControllerTest : AbstractWithPlayerIdControllerTest<DonationController, Donation, IDonationService>
{
    protected override IDonationService CreateServiceFake()
    {
        return A.Fake<IDonationService>();
    }

    protected override DonationController CreateController(IDonationService service)
    {
        return new DonationController(service);
    }

    protected override Donation CreateData()
    {
        return new Donation(Guid.NewGuid().ToString(), 1, DateTime.Now, 1, "def", Guid.NewGuid().ToString(), false);
    }
}