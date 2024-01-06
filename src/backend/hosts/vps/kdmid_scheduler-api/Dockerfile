FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app

COPY . .

WORKDIR /app/hosts/vps/kdmid_scheduler-api

RUN dotnet restore

RUN dotnet publish -c Release -o publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app/hosts/vps/kdmid_scheduler-api/publish

COPY --from=build-env /app/hosts/vps/kdmid_scheduler-api/publish .

ENTRYPOINT ["dotnet", "KdmidScheduler.Api.dll"]
