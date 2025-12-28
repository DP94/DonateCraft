using System.Collections;
using Common.Exceptions;
using Common.Models;
using Core.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers;

public abstract class AbstractWithIdControllerTest<C, T, S> : AbstractControllerTest<C, T, S> where C : WithIdController<T> where T : WithId where S : WithIdService<T>
{
    [Test]
    public void Get_ById_Returns404_If_Null()
    {
        A.CallTo(() => this._service.GetById("1")).Throws<ResourceNotFoundException>();
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._controller.GetById("1"));
    }
    
    [Test]
    public async Task Delete_Deletes_Successfully()
    {
        var result = await this._controller.Delete("abc") as StatusCodeResult;
        Assert.That(204, Is.EqualTo(result.StatusCode));
    }
    
    [Test]
    public async Task GetById_Returns_Correct_Value()
    {
        var id = Guid.NewGuid().ToString();
        var value = this.CreateData();
        A.CallTo(() => this._service.GetById(id)).Returns(value);
        var result = await this._controller.GetById(id) as ObjectResult;
        A.CallTo(() => this._service.GetById(id)).MustHaveHappenedOnceExactly();
        Assert.That(value, Is.EqualTo(result.Value));
        Assert.That(200, Is.EqualTo(result.StatusCode));
    }
    
    [Test]
    public async Task Create_CreatesPlayer_Successfully()
    {
        var value = CreateData();
        A.CallTo(() => this._service.Create(value)).Returns(value);
        var result = await this._controller.Create(value) as ObjectResult;
        Assert.That(value, Is.EqualTo(result.Value));
        Assert.That(201, Is.EqualTo(result.StatusCode));
    }
    
    
    [Test]
    public async Task Update_Updates_Successfully()
    {
        var newValue = CreateData();
        var existingValue = CreateData();
        A.CallTo(() => this._service.Update(newValue)).Returns(newValue);
        A.CallTo(() => this._service.GetById(existingValue.Id)).Returns(existingValue);
        var result = await this._controller.Update(newValue) as ObjectResult;
        A.CallTo(() => this._service.Update(newValue)).MustHaveHappenedOnceExactly();
        Assert.That(newValue, Is.EqualTo(result.Value));
        Assert.That(200, Is.EqualTo(result.StatusCode));
    }
    
    [Test]
    public async Task GetAll_ReturnsAll()
    {
        var values = new List<T>() {CreateData(), CreateData()};
        A.CallTo(() => this._service.GetAll()).Returns(values);
        var result = await this._controller.GetAll() as ObjectResult;
        Assert.That(values, Is.EqualTo(result.Value).AsCollection);
        Assert.That(200, Is.EqualTo(result.StatusCode));
    }
}