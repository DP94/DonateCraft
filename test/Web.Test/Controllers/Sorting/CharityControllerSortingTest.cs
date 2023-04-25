using Common.Models;
using Core.Services;
using Core.Services.Charity;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers.Sorting;

public class CharityControllerSortingTest : AbstractControllerSortingTest<Charity, CharityController, ICharityService>
{
    private ICharityService _charityService;

    [Test]
    public async Task GetAll_Charities_SortsByDonationCountAsc_Successfully()
    {
        var charities = CreateData();
        A.CallTo(() => this._charityService.GetAll()).Returns(charities);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("donationCount") },
            { "sortOrder", new StringValues("asc") }
        });
        var result = await this._controller.GetAll() as ObjectResult;
        var newCharities = result.Value as List<Charity>;
        Assert.AreEqual("abc", newCharities[0].Id);
        Assert.AreEqual("ghi", newCharities[1].Id);
    }

    [Test]
    public async Task GetAll_Charities_SortsByDonationCountDesc_Successfully()
    {
        var charities = CreateData();
        A.CallTo(() => this._charityService.GetAll()).Returns(charities);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("donationCount") },
            { "sortOrder", new StringValues("desc") }
        });
        var result = await this._controller.GetAll() as ObjectResult;
        var newCharities = result.Value as List<Charity>;
        Assert.AreEqual("ghi", newCharities[0].Id);
        Assert.AreEqual("abc", newCharities[1].Id);
    }

    protected override List<Charity> CreateData()
    {
        return new List<Charity>
        {
            new("abc", 1, false),
            new("ghi", 2, false)
        };
    }

    protected override CharityController CreateController(ICharityService charityService)
    {
        return new CharityController(charityService);
    }

    protected override ICharityService CreateServiceFake()
    {
        this._charityService = A.Fake<ICharityService>();
        return this._charityService;
    }
}