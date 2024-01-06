FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app

COPY . ./

WORKDIR /app/src/hosts/KdmidScheduler.Web.Api

RUN dotnet restore

RUN dotnet publish -c Release -o publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app/src/hosts/KdmidScheduler.Web.Api/publish

COPY --from=build-env /app/src/hosts/KdmidScheduler.Web.Api/publish .

ENTRYPOINT ["dotnet", "KdmidScheduler.Web.Api.dll"]
