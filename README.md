# DonateCraft API
[![Main build](https://github.com/DP94/DonateCraft/actions/workflows/main.yml/badge.svg)](https://github.com/DP94/DonateCraft/actions/workflows/main.yml)

# About
DonateCraft is the C# REST API responsible for managing the state of the [DonateCraft plugin](https://github.com/DP94/DonateCraftPlugin).
It is built on .NET 6 ASP.NET and currently targeted to run on AWS, using Lambda and DynamoDB.

# Meet the team
<img src="https://avatars.githubusercontent.com/u/14276637" width="100" height="100"/>	 **Dan** - Lead API engineer and team lead</br>
<img src="https://avatars.githubusercontent.com/u/14300505" width="100" height="100"/>	 **Veronica** - Lead UI engineer</br>
<img src="https://avatars.githubusercontent.com/u/575136" width="100" height="100"/> 		 **Sam** - Lead Minecraft plugin engineer and advisor of all things AWS</br>
# Building & running
DonateCraft was developed using JetBrains Rider and it is recommended to use that as the IDE of preference. Running the API is simple and just requires the following run configuration:
![image](https://user-images.githubusercontent.com/14276637/196933244-ec5e1743-fd64-4b71-a14c-97632f317da6.png)

**Important**:
The "ASPNETCORE_ENVIRONMENT" variable must be present and set to "LOCAL" for the local DynamoDB instance to run.


