FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY EkaCare.SDK/EkaCare.SDK.csproj EkaCare.SDK/
COPY EkaCare.WebApi/EkaCare.WebApi.csproj EkaCare.WebApi/
RUN dotnet restore EkaCare.WebApi/EkaCare.WebApi.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/EkaCare.WebApi
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "EkaCare.WebApi.dll"]
