using System.Net.Http.Headers;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Cloud.Services;
using Cloud.Services.Aws;
using Common.Exceptions;
using Common.Models;
using Common.Util;
using Core.Services.Charity;
using Core.Services.Donation;
using Core.Services.Lock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Lock = Common.Models.Lock;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RevivalLambda;

public class Function
{
    private readonly IDonationService _donationService;
    private readonly ILockService _lockService;
    private readonly ICharityService _charityService;
    private readonly ILogger<Function> _logger;
    private readonly HttpClient _client;
    private readonly string _apiKey;


    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Function()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILockService, LockService>();
        services.AddSingleton<ICharityService, CharityService>();
        services.AddSingleton<IDonationService, DonationService>();
        services.AddSingleton<ILockCloudService, LockDynamoDbCloudService>();
        services.AddSingleton<ICharityCloudService, CharityDynamoDbCloudService>();
        services.AddSingleton<IDonationCloudService, DonationDynamoDbCloudService>();
        services.AddSingleton<IPlayerCloudService, PlayerDynamoDbCloudService>();
        services.AddAWSService<IAmazonDynamoDB>();
        services.AddMemoryCache();

        var options = new DonateCraftOptions();
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build().GetSection("DonateCraft");
        configuration.Bind(options);
        var wrapper = Options.Create(options);
        services.AddSingleton(wrapper);
        
        
        var serviceProvider = services.BuildServiceProvider();
        this._donationService = serviceProvider.GetService<IDonationService>();
        this._charityService = serviceProvider.GetService<ICharityService>();
        this._lockService = serviceProvider.GetService<ILockService>();
        this._logger =  serviceProvider.GetService<ILogger<Function>>();
        this._client = new HttpClient
        {
            BaseAddress = new Uri(Environment.GetEnvironmentVariable(Constants.JG_API_URL) ??
                                  "https://api.staging.justgiving.com/")
        };
        this._apiKey =  Environment.GetEnvironmentVariable(Constants.JG_API_KEY);
    }
    
    public async Task HandleRequest(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var record in evnt.Records)
        {
            var json = record.Body;
            var message = JsonSerializer.Deserialize<RevivalMessage>(json);
            var player = message.PlayerId;
            var donationId = message.DonationId;
            var paidForKey = message.PaidForById;

            Lock currentLock = null;
            try
            {
                currentLock = await this._lockService.GetById(player);
            }
            catch (ResourceNotFoundException)
            {
                this._logger.LogWarning("Lock with id {Player} not found, error code 4", player);
            }
            if (currentLock == null)
            {
                //In the event of someone donating when no lock is present
                return; //Redirect($"{this._donateCraftUi}?status=error&code=4");
            }
            if (currentLock.Unlocked)
            {
                //Send message here saying lock already unlocked
                return; //Redirect($"{this._donateCraftUi}?status=warning");
            }

            this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var justGivingDonation = await this.GetDonationData(donationId);
            if (justGivingDonation is not { Status: "Accepted" or "Pending" })
            {
                //Send error back here
                this._logger.LogInformation("Donation was not successful! Status is {Status}, error code 5",
                    justGivingDonation?.Status);
                return; //Redirect($"{this._donateCraftUi}?status=error&code=5");
            }

            var charityData = await GetCharityData(justGivingDonation.CharityId);
            var name = charityData.Name;
            var id = justGivingDonation.CharityId;


            await this._donationService.Create(player, new Donation
            {
                Amount = Convert.ToDouble(justGivingDonation.Amount),
                Id = justGivingDonation.Id.ToString(),
                CharityId = id,
                CharityName = name,
                CreatedDate = DateTime.Now,
                PaidForId = paidForKey ?? player,
                Private = string.IsNullOrWhiteSpace(justGivingDonation.Amount)
            });
            var charity = await this._charityService.GetById(id.ToString());
            charity.DonationCount++;
            await this._charityService.Update(charity);

            currentLock.DonationId = justGivingDonation.Id.ToString();
            currentLock.Unlocked = true;
            await this._lockService.Update(currentLock);
        }
    }
    
    private async Task<JustGivingDonation> GetDonationData(string donationId)
    {
        JustGivingDonation donation = null;
        for (var i = 0; i < 10; i++)
        {
            var donationData = await this._client.GetAsync($"{this._apiKey}/v1/donation/{donationId}");
            if (!donationData.IsSuccessStatusCode)
            {
                this._logger.LogWarning("Get donation {DonationId} data did not succeed {DonationDataStatusCode}", donationId, donationData.StatusCode);
                await Task.Delay(TimeSpan.FromMilliseconds(500) * i);
                continue;
            }
            donationData.EnsureSuccessStatusCode();
            var responseBody = await donationData.Content.ReadAsStringAsync();
            var justGivingDonation = JsonSerializer.Deserialize<JustGivingDonation>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (!donationData.IsSuccessStatusCode || justGivingDonation == null)
            {
                throw new InvalidOperationException($"Could not find a donation with id of {donationId}");
            }
            return justGivingDonation;
        }
        throw new InvalidOperationException("Donation not found");
    }

    
    private async Task<JustGivingCharity> GetCharityData(int charityId)
    {
        var charityResponse = await this._client.GetAsync($"{this._apiKey}/v1/charity/{charityId}");
        var charityData = JsonSerializer.Deserialize<JustGivingCharity>(await charityResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (!charityResponse.IsSuccessStatusCode || charityData == null)
        {
            throw new InvalidOperationException($"Could not find a charity with id of {charityId}");
        }
        return charityData;
    }
}