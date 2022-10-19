FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY DonateCraft.sln .
COPY src/Web/Web.csproj ./src/Web/
COPY src/Core/Core.csproj ./src/Core/
COPY src/Common/Common.csproj ./src/Common/
COPY src/Cloud/Cloud.csproj ./src/Cloud/

COPY test/Web.Test/Web.Test.csproj ./test/Web.Test/
COPY test/Cloud.Test/Cloud.Test.csproj ./test/Cloud.Test/

RUN dotnet restore

COPY src/. ./src/
COPY test/. ./test/

RUN dotnet build DonateCraft.sln -c Release

RUN dotnet test test/Web.Test/Web.Test.csproj --no-build -c Release; exit 0

RUN dotnet publish src/Web/Web.csproj --no-build -c Release -o /app/publish/Web

FROM mcr.microsoft.com/dotnet/aspnet:6.0

WORKDIR /etc
RUN mkdir ./docker

env ASPNETCORE_URLS = "http://*:80" \
WORKDIR /app
COPY --from=build-env /app/publish/Web .
EXPOSE 80
ENTRYPOINT ["dotnet", "Web.dll"]