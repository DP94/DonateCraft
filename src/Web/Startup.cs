using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Cloud.DynamoDbLocal;
using Cloud.Services;
using Core.Services;

namespace Web;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        var awsOptions = new AWSOptions
        {
            Region = RegionEndpoint.EUWest2
        };
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "LOCAL")
        {
            new LocalDynamoDbSetup().SetupDynamoDb();
            awsOptions.Credentials = new BasicAWSCredentials("x", "x");
            awsOptions.DefaultClientConfig.ServiceURL = "http://localhost:8000";
        }

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonDynamoDB>(awsOptions);

        services.AddSingleton<IPlayerService, PlayerService>();
        services.AddSingleton<IDeathService, DeathService>();
        services.AddSingleton<IPlayerDynamoDbStorageService, PlayerDynamoDbStorageService>();
        services.AddSingleton<IDeathDynamoDbStorageService, DeathDynamoDbStorageService>();

        services.AddSwaggerGen(options => { options.EnableAnnotations(); });
        services.AddHttpContextAccessor();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
                        .WithExposedHeaders("Authorization", "x-amzn-remapped-authorization");
                });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors();
        app.Use((context, next) =>
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            return next.Invoke();
        });

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}