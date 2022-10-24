using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Cloud.DynamoDbLocal;
using Cloud.Services;
using Cloud.Services.Aws;
using Common.Models;
using Common.Util;
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
        var env = Environment.GetEnvironmentVariable(Constants.ASPNETCORE_ENVIRONMENT);
        if (env == "LOCAL")
        {
            var localDynamo = new LocalDynamoDbSetup();
            localDynamo.SetupDynamoDb().Wait();
            localDynamo.CreateTables(DynamoDbConstants.PlayerTableName,
                DynamoDbConstants.LockTableName, DynamoDbConstants.CharityTableName).Wait();
            awsOptions.Credentials = new BasicAWSCredentials("x", "x");
            awsOptions.DefaultClientConfig.ServiceURL = "http://localhost:8000";
        }

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonDynamoDB>(awsOptions);
        RegisterServices(services);

        
        var justGivingHost = Environment.GetEnvironmentVariable(Constants.JG_API_URL) ?? "https://api.staging.justgiving.com/";
        var justGivingAPIKey = Environment.GetEnvironmentVariable(Constants.JG_API_KEY);
        if (justGivingAPIKey == null)
        {
            throw new InvalidOperationException($"{Constants.JG_API_KEY} could not be found as an environment variable!");
        }
        
        services.Configure<DonateCraftOptions>(Configuration.GetSection(DonateCraftOptions.DonateCraft));

        var client = new HttpClient { BaseAddress = new Uri(justGivingHost) };
        services.AddSingleton(client);
        
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

        //app.UseHttpsRedirection();

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
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IPlayerService, PlayerService>();
        services.AddSingleton<IDeathService, DeathService>();
        services.AddSingleton<ILockService, LockService>();
        services.AddSingleton<ICharityService, CharityService>();
        services.AddSingleton<IDonationService, DonationService>();
        services.AddSingleton<IPlayerCloudService, PlayerDynamoDbCloudService>();
        services.AddSingleton<IDeathCloudService, DeathDynamoDbStorageService>();
        services.AddSingleton<ILockCloudService, LockDynamoDbCloudService>();
        services.AddSingleton<ICharityCloudService, CharityDynamoDbCloudService>();
        services.AddSingleton<IDonationCloudService, DonationDynamoDbCloudService>();
    }
}