# Use the .NET 8 SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY Ade_Farming.csproj ./
RUN dotnet restore "Ade_Farming.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "Ade_Farming.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "Ade_Farming.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use a lightweight ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Environment variables
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=10000

# Expose the port your app will run on
EXPOSE 10000

# Start the app
ENTRYPOINT ["dotnet", "Ade_Farming.dll"]
