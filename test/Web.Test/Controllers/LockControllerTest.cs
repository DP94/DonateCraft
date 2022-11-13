using System.Collections;
using Common.Models;
using Core.Services;
using Core.Services.Lock;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers;

public class LockControllerTest
{
    private LockController _controller;
    private ILockService _lockService;
    
    [SetUp]
    public void SetUp()
    {
        this._lockService = A.Fake<ILockService>();
        this._controller = new LockController(this._lockService, A.Fake<IHttpContextAccessor>());
    }
    
    [Test]
    public async Task GetLocks_ReturnsCorrectLocks()
    {
        var locks = new List<Lock>
        {
            new(Guid.NewGuid().ToString(), false),
            new(Guid.NewGuid().ToString(), false)
        };
        A.CallTo(() => this._lockService.GetAll()).Returns(locks);
        var result = await this._controller.GetLocks(null) as ObjectResult;
        
        A.CallTo(() => this._lockService.GetAll()).MustHaveHappenedOnceExactly();
        CollectionAssert.AreEqual(locks, (IEnumerable)result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
    
    [Test]
    public async Task GetById_Returns_Correct_Value()
    {
        var id = Guid.NewGuid().ToString();
        var newLock = new Lock(id, false);
        A.CallTo(() => this._lockService.GetById(id)).Returns(newLock);
        var result = await this._controller.GetLock(id) as ObjectResult;
        A.CallTo(() => this._lockService.GetById(id)).MustHaveHappenedOnceExactly();
        Assert.AreEqual(newLock, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
    
    [Test]
    public async Task CreateLock_Creates_Successfully()
    {
        var newLock = new Lock(Guid.NewGuid().ToString(), false);
        A.CallTo(() => this._lockService.Create(newLock)).Returns(newLock);
        var result = await this._controller.CreateLock(newLock) as ObjectResult;
        A.CallTo(() => this._lockService.Create(newLock)).MustHaveHappenedOnceExactly();
        Assert.AreEqual(newLock, result.Value);
        Assert.AreEqual(201, result.StatusCode);
    }
    
    [Test]
    public async Task UpdateLock_Updates_Successfully()
    {
        var newLock = new Lock(Guid.NewGuid().ToString(), false);
        var existingLock = new Lock(Guid.NewGuid().ToString(), false);
        A.CallTo(() => this._lockService.Update(newLock)).Returns(newLock);
        A.CallTo(() => this._lockService.GetById(existingLock.Id)).Returns(existingLock);
        var result = await this._controller.UpdateLock(newLock) as ObjectResult;
        A.CallTo(() => this._lockService.Update(newLock)).MustHaveHappenedOnceExactly();
        Assert.AreEqual(newLock, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
    
    [Test]
    public async Task DeleteLock_Deletes_Successfully()
    {
        var result = await this._controller.DeleteLock("abc") as StatusCodeResult;
        Assert.AreEqual(204, result.StatusCode);
    }
}