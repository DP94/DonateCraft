using Common.Exceptions;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using NUnit.Framework;
using Web.Filters;

namespace Web.Test.Filters;

public class ExceptionFilterTest
{

    private ExceptionFilter _filter;
    private ActionContext _actionContext;
    
    [SetUp]
    public void SetUp()
    {
        this._filter = new ExceptionFilter();
        this._actionContext = new ActionContext(new DefaultHttpContext(), A.Fake<RouteData>(), A.Fake<ActionDescriptor>());
    }

    [Test]
    public async Task Filter_Returns404_When_ResourceNotFoundException_Thrown()
    {
        await RunFilterTest(new ResourceNotFoundException("test"), 404, "test");
    }
    
    [Test]
    public async Task Filter_Returns409_When_ResourceNotFoundException_Thrown()
    {
        await RunFilterTest(new ResourceExistsException("test"), 409, "test");
    }

    [Test]
    public async Task Filter_Returns500_When_ExceptionNotMapped_Thrown()
    {
        await RunFilterTest(new OutOfMemoryException("test"), 500, "test");
    }

    
    private async Task RunFilterTest(Exception exception, int expectedCode, string expectedMessage)
    {
        var context = new ExceptionContext(this._actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
        await this._filter.OnExceptionAsync(context);
        var result = context.Result as JsonResult;
        var value = result.Value as ExceptionModel;
        Assert.AreEqual(expectedCode, result.StatusCode);
        Assert.AreEqual(expectedMessage, value.Error);
    }
}