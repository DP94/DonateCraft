# AGENTS.md

## Project Overview

**DonateCraft** is the C# REST API responsible for managing the state of the [DonateCraft plugin](https://github.com/DP94/DonateCraftPlugin), a Minecraft plugin that lets players raise money for charity in-game. It runs on ASP.NET Core, deployed to AWS Lambda, backed by DynamoDB, and integrates with the JustGiving API for charity/donation data.

### Key technologies
- **.NET 10.0** for the framework and language
- **ASP.NET Core** for the REST API, hosted via **Amazon.Lambda.AspNetCoreServer** (API Gateway + Lambda)
- **AWS SDK** (DynamoDB, SQS) for cloud interactions
- **Terraform** for provisioning AWS infrastructure
- **DynamoDB Local** for local development, spun up automatically when `ASPNETCORE_ENVIRONMENT=LOCAL`
- **JustGiving API** to handle donations via callbacks - the API is here: https://api.justgiving.com/docs

### Project structure

DonateCraft is a layered solution (`DonateCraft.sln`) split into `src/` (production projects) and `test/` (a matching xUnit test project per source project). Dependencies flow one direction: `Web`/`RevivalLambda` → `Core` → `Cloud` → `Common`.

- **`src/Web`** — The ASP.NET Core Web API and the primary Lambda entry point.
  - `Controllers/` — REST endpoints (`CharityController`, `DonationController`, `PlayerController`, `DeathController`, `LockController`, `CallbackController`), plus shared base controllers (`WithIdController`, `WithPlayerIdController`).
  - `Filters/` — `ExceptionFilter` for translating exceptions into HTTP responses.
  - `Startup.cs` — DI/service registration, AWS options, DynamoDB Local bootstrap for the `LOCAL` environment, JustGiving API configuration.
  - `LambdaEntryPoint.cs` / `LocalEntryPoint.cs` — entry points for running as a Lambda (via API Gateway) vs. locally (Kestrel).

- **`src/RevivalLambda`** — A standalone Lambda function, triggered by SQS, that handles "revival" messages (`Function.cs`, `Services/RevivalService.cs`). Deployed independently of `Web`.

- **`src/Core`** — Business logic layer. Each domain (`Charity`, `Death`, `Donation`, `Lock`, `Player`) has a `Service`/`IService` pair under `Services/<Domain>/`, plus shared generics (`WithIdService`, `WithPlayerIdService`, `BaseService`).

- **`src/Cloud`** — AWS integration layer: DynamoDB and SQS access behind interfaces (`ICharityCloudService`, `IDonationCloudService`, `IRevivalQueueService`, etc.), DynamoDB attribute-mapping utilities, and `DynamoDbLocal/` (bundled DynamoDB Local jar + setup code for offline dev).
  - `Terraform/` — infrastructure-as-code for API Gateway, Lambda, DynamoDB, SQS, and IAM.

- **`src/Common`** — Shared, dependency-free building blocks used by every layer: domain `Models/` (`Charity`, `Donation`, `Player`, `Death`, `Lock`, JustGiving DTOs, `Sort/` criteria), `Exceptions/`, and `Util/` (constants).

- **`test/`** — Mirrors `src/` one-to-one: `Web.Test`, `Cloud.Test`, `RevivalLambda.Test`, plus `Integration.Test` for end-to-end coverage across layers.

Solution-wide package versions are centralized in `Directory.Packages.props` (central package management).