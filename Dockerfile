# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore dependencies
COPY ["Ade_Farming.csproj", "."]
RUN dotnet restore "./Ade_Farming.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "./Ade_Farming.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "./Ade_Farming.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expose Render-friendly port
EXPOSE 8080
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

# Copy published output
COPY --from=publish /app/publish .

# Start the app
ENTRYPOINT ["dotnet", "Ade_Farming.dll"]
