using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers.Sorting;

public abstract class AbstractControllerSortingTest<T, C> where C : DonateCraftBaseController<T>
{
    
    protected C _controller;

    [SetUp]
    public void AbstractSetUp()
    {
        this._controller = CreateController();
        this._controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    protected void SetQueryParams(Dictionary<string, StringValues> values)
    {
        this._controller.HttpContext.Request.Query = new QueryCollection(values);
    }

    protected abstract C CreateController();
}