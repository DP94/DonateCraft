using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace RevivalLambda;

[LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }
}