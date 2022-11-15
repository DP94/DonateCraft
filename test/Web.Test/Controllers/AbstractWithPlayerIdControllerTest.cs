using System.Collections;
using Common.Models;
using Core.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers;

public abstract class AbstractWithPlayerIdControllerTest<C, T, S> : AbstractControllerTest<C, T, S> where C : WithPlayerIdController<T> where T : WithPlayerId where S : WithPlayerIdService<T>
{
    [Test]
    public async Task Delete_Deletes_Successfully()
    {
        var result = await this._controller.Delete("test", "abc") as StatusCodeResult;
        Assert.AreEqual(204, result.StatusCode);
    }
    
    [Test]
    public async Task Get_ReturnsPlayers()
    {
        var values = new List<T>() { CreateData(), CreateData() };
        A.CallTo(() => this._service.GetAllForPlayerId("test")).Returns(values);
        var result = await this._controller.GetAllForPlayer("test") as ObjectResult;
        CollectionAssert.AreEqual(values, (IEnumerable)result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task Get_ById_ReturnsCorrectValue()
    {
        var value = CreateData();
        A.CallTo(() => this._service.GetByPlayerId("test", "abc")).Returns(value);
        var result = await this._controller.GetByIdForPlayer("test", "abc") as ObjectResult;
        Assert.AreEqual(value, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
    
    [Test]
    public async Task Post_Creates_Successfully()
    {
        var value = CreateData();
        A.CallTo(() => this._service.Create("test", value)).Returns(value);
        var result = await this._controller.Create("test", value) as ObjectResult;
        Assert.AreEqual(value, result.Value);
        Assert.AreEqual(201, result.StatusCode);
    }

    [Test]
    public async Task Put_Updates_Successfully()
    {
        var value = CreateData();
        A.CallTo(() => this._service.Update("test", value)).Returns(value);
        var result = await this._controller.Update("test", value) as ObjectResult;
        Assert.AreEqual(value, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
}