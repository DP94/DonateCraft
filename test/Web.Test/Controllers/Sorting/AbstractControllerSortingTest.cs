using Common.Models;
using Core.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers.Sorting;

public abstract class AbstractControllerSortingTest<T, C, S> where C : WithIdController<T> where S : WithIdService<T> where T : WithId
{
    
    protected C _controller;
    protected S _service;

    [SetUp]
    public void AbstractSetUp()
    {
        this._service = CreateServiceFake();
        this._controller = CreateController(this._service);
        this._controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
    
    
    
    [Test]
    public async Task GetAll_Players_SortsByIdDesc_Successfully()
    {
        var data = CreateData();
        A.CallTo(() => this._service.GetAll()).Returns(data);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("id")},
            { "sortOrder", new StringValues("desc")}
        });
        var result = await this._controller.GetAll() as ObjectResult;
        var values = result.Value as List<T>;
        Assert.AreEqual("ghi", values[0].Id);
        Assert.AreEqual("abc", values[1].Id);
    }
    
    [Test]
    public async Task GetAll_Players_SortsByIdAsc_Successfully()
    {
        var data = CreateData();
        A.CallTo(() => this._service.GetAll()).Returns(data);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("id")},
            { "sortOrder", new StringValues("asc")}
        });
        var result = await this._controller.GetAll() as ObjectResult;
        var values = result.Value as List<T>;
        Assert.AreEqual("abc", values[0].Id);
        Assert.AreEqual("ghi", values[1].Id);
    }
    
    

    protected void SetQueryParams(Dictionary<string, StringValues> values)
    {
        this._controller.HttpContext.Request.Query = new QueryCollection(values);
    }

    protected abstract C CreateController(S service);
    protected abstract S CreateServiceFake();
    protected abstract List<T> CreateData();
}