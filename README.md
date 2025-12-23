
# DonateCraft API
[![Main build](https://github.com/DP94/DonateCraft/actions/workflows/main.yml/badge.svg)](https://github.com/DP94/DonateCraft/actions/workflows/main.yml)

# About
DonateCraft is the C# REST API responsible for managing the state of the [DonateCraft plugin](https://github.com/DP94/DonateCraftPlugin).
It is built on .NET 8 ASP.NET and currently targeted to run on AWS, using Lambda and DynamoDB.

# Building & running
DonateCraft was developed using JetBrains Rider and it is recommended to use that as the IDE of preference. Running the API is simple and just requires the following run configuration:
![image](https://user-images.githubusercontent.com/14276637/198834693-f62266a3-32df-4286-8f5d-ebd2d112dc5c.png)

**Important**:
The following envrionment variables must be set:

 - **ASPNETCORE_ENVIRONMENT** 
	 - Set this to "LOCAL" to enable DynamoDB local spin up
 - **DonateCraft__DonateCraftUiUrl**
	 - Set this to your local instance of the DonateCraftUI, default is https://donatecraftui.s3.eu-west-2.amazonaws.com/index.html
 - **DonateCraft__JustGivingApiKey** 
	 - Set this to whatever your JustGiving API key is. Used to make API requests for donation and charity information.
 - **DonateCraft__JustGivingApiUrl**
	 - Set this to https://api.staging.justgiving.com/

The "ASPNETCORE_ENVIRONMENT" variable must be present and set to "LOCAL" for the local DynamoDB instance to run.


