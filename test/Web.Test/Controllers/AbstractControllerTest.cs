using Common.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers;

public abstract class AbstractControllerTest<C, T, S> where C : DonateCraftController<T> where T : WithId where S : BaseService
{
    protected C _controller;
    protected S _service;
    
    [SetUp]
    public void SetUp()
    {
        this._service = this.CreateServiceFake();
        this._controller = this.CreateController(this._service);
        this._controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
    
    protected abstract S CreateServiceFake();
    
    protected abstract C CreateController(S service);

    protected abstract T CreateData();

}