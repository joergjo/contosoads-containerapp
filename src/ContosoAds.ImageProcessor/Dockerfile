FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8081
ENV ASPNETCORE_URLS http://*:8081

RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["./ContosoAds.ImageProcessor.csproj", "."]
RUN dotnet restore "ContosoAds.ImageProcessor.csproj"
COPY [".", "."]
RUN dotnet build "ContosoAds.ImageProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContosoAds.ImageProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContosoAds.ImageProcessor.dll"]
