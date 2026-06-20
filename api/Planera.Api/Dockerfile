FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5065

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["api/Planera.Api/Planera.Api.csproj", "api/Planera.Api/"]
COPY ["apphost/Planera.ServiceDefaults/Planera.ServiceDefaults.csproj", "apphost/Planera.ServiceDefaults/"]
RUN dotnet restore "api/Planera.Api/Planera.Api.csproj"
COPY . .
WORKDIR api/Planera.Api
RUN dotnet build "Planera.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Planera.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app/data
COPY --from=publish /app/publish /app
COPY --from=publish /app/publish/appsettings.json /app/data
RUN echo "cp -n ../appsettings.json .; dotnet ../Planera.dll" >> ../start.sh
ENTRYPOINT ["sh", "../start.sh"]
