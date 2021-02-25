FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

ARG PROJECT

RUN test -n "$PROJECT" || (echo "PROJECT argument not set" && false)

COPY ./ButtBot.Library ../ButtBot.Library

COPY ./ButtBot.$PROJECT/ButtBot.$PROJECT.csproj .
RUN dotnet restore

COPY ./ButtBot.$PROJECT .
RUN rm ./appsettings*.json

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
WORKDIR /app

ARG PROJECT
ENV PROJECT_DLL="ButtBot.$PROJECT.dll"

COPY --from=build-env /app/out .

ENTRYPOINT dotnet $PROJECT_DLL
