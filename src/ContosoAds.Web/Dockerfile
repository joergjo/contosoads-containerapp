FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8090
ENV ASPNETCORE_URLS=http://*:8080;http://*:8090

RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["./ContosoAds.Web.csproj", "."]
RUN dotnet restore "ContosoAds.Web.csproj"
COPY [".", "."]
RUN dotnet build "ContosoAds.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContosoAds.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContosoAds.Web.dll"]
