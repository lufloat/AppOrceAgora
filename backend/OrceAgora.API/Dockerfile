FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .

RUN dotnet restore OrceAgora.API/OrceAgora.API.csproj
RUN dotnet publish OrceAgora.API/OrceAgora.API.csproj -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "OrceAgora.API.dll"]