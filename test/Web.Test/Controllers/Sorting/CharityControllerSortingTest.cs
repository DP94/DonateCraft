using Common.Models;
using Core.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers.Sorting;

public class CharityControllerSortingTest : AbstractControllerSortingTest<Charity, CharityController>
{
    private ICharityService _charityService;
    
    [Test]
    public async Task GetAll_Charities_SortsByIdAsc_Successfully()
    {
        var charities = CreateCharities();
        A.CallTo(() => this._charityService.GetCharities()).Returns(charities);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("id")},
            { "sortOrder", new StringValues("asc")}
        });
        var result = await this._controller.GetCharities() as ObjectResult;
        var newCharities = result.Value as List<Charity>;
        Assert.AreEqual("abc", newCharities[0].Id);
        Assert.AreEqual("ghi", newCharities[1].Id);
    }
    
    [Test]
    public async Task GetAll_Charities_SortsByIdDesc_Successfully()
    {
        var charities = CreateCharities();
        A.CallTo(() => this._charityService.GetCharities()).Returns(charities);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("id")},
            { "sortOrder", new StringValues("desc")}
        });
        var result = await this._controller.GetCharities() as ObjectResult;
        var newCharities = result.Value as List<Charity>;
        Assert.AreEqual("ghi", newCharities[1].Id);
        Assert.AreEqual("abc", newCharities[0].Id);
    }
    
    [Test]
    public async Task GetAll_Charities_SortsByDonationCountAsc_Successfully()
    {
        var charities = CreateCharities();
        A.CallTo(() => this._charityService.GetCharities()).Returns(charities);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("donationCount")},
            { "sortOrder", new StringValues("asc")}
        });
        var result = await this._controller.GetCharities() as ObjectResult;
        var newCharities = result.Value as List<Charity>;
        Assert.AreEqual("abc", newCharities[0].Id);
        Assert.AreEqual("ghi", newCharities[1].Id);
    }
    
    [Test]
    public async Task GetAll_Charities_SortsByDonationCountDesc_Successfully()
    {
        var charities = CreateCharities();
        A.CallTo(() => this._charityService.GetCharities()).Returns(charities);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("donationCount")},
            { "sortOrder", new StringValues("desc")}
        });
        var result = await this._controller.GetCharities() as ObjectResult;
        var newCharities = result.Value as List<Charity>;
        Assert.AreEqual("ghi", newCharities[1].Id);
        Assert.AreEqual("abc", newCharities[0].Id);
    }
    
    private static List<Charity> CreateCharities()
    {
        return new List<Charity>
        {
            new("abc", 1),
            new("ghi", 2)
        };
    }
    
    protected override CharityController CreateController()
    {
        this._charityService = A.Fake<ICharityService>();
        return new CharityController(this._charityService);
    }
}