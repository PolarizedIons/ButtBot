FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY ./ButtBot/ButtBot.csproj .
RUN dotnet restore

COPY ./ButtBot .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
WORKDIR /app

COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "ButtBot.dll"]
