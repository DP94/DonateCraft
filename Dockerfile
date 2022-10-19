FROM public.ecr.aws/lambda/dotnet:6 AS base
FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim as build

WORKDIR /app

COPY DonateCraft.sln .
COPY src/Web/Web.csproj ./src/Web/
COPY src/Core/Core.csproj ./src/Core/
COPY src/Common/Common.csproj ./src/Common/
COPY src/Cloud/Cloud.csproj ./src/Cloud/
COPY src/DeathLambda/DeathLambda.csproj ./src/DeathLambda/

COPY test/Web.Test/Web.Test.csproj ./test/Web.Test/
COPY test/Cloud.Test/Cloud.Test.csproj ./test/Cloud.Test/
COPY test/DeathLambda.Test/DeathLambda.Test.csproj ./test/DeathLambda.Test/

RUN dotnet restore

COPY src/. ./src/
COPY test/. ./test/

RUN dotnet build DonateCraft.sln -c Release

#We aren't running tests here as Github actions run them for us earlier in the process

FROM build AS publish
RUN dotnet publish src/Web/Web.csproj --configuration Release --runtime linux-x64 --self-contained false --output /app/publish/Web -p:PublishReadyToRun=true
RUN dotnet publish src/DeathLambda/DeathLambda.csproj --configuration Release --runtime linux-x64 --self-contained false --output /app/publish/DeathLambda -p:PublishReadyToRun=true

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish/Web .