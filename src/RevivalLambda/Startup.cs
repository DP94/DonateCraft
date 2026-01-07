using Amazon.Lambda.Annotations;
using Cloud.Services;
using Cloud.Services.Aws;
using Core.Services.Charity;
using Core.Services.Donation;
using Core.Services.Lock;
using Microsoft.Extensions.DependencyInjection;

namespace RevivalLambda;

[LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {

    }
}