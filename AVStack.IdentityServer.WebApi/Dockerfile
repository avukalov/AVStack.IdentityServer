FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["AVStack.IdentityServer.WebApi/AVStack.IdentityServer.WebApi.csproj", "AVStack.IdentityServer.WebApi/"]
RUN dotnet restore "AVStack.IdentityServer.WebApi/AVStack.IdentityServer.WebApi.csproj"
COPY . .
WORKDIR "/src/AVStack.IdentityServer.WebApi"
RUN dotnet build "AVStack.IdentityServer.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AVStack.IdentityServer.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AVStack.IdentityServer.WebApi.dll"]
