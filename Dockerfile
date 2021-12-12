

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ./LocalNugets /tmp/LocalNugets
COPY ./*.sln ./
COPY ./*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

COPY . .
WORKDIR "/src"
RUN dotnet build "AVStack.IdentityServer.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AVStack.IdentityServer.sln" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AVStack.IdentityServer.WebApi.dll"]
