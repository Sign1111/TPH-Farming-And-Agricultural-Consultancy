FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Ade_Farming.csproj", "."]
RUN dotnet restore "./Ade_Farming.csproj"
COPY . .
RUN dotnet build "./Ade_Farming.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Ade_Farming.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ade_Farming.dll"]
