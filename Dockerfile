﻿

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ./LocalNugets /tmp/LocalNugets

COPY ./*.sln ./
COPY ./*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

RUN dotnet restore "AVStack.IdentityServer.sln" -s https://api.nuget.org/v3/index.json -s /tmp/LocalNugets
ENTRYPOINT ["dotnet run"]