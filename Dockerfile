FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EnergyPlanner/EnergyPlanner.csproj EnergyPlanner/
RUN dotnet restore EnergyPlanner/EnergyPlanner.csproj
COPY EnergyPlanner/ EnergyPlanner/
RUN dotnet publish EnergyPlanner/EnergyPlanner.csproj -c Release -o /app/publish --no-restore

FROM build AS migrate
RUN dotnet tool install --global dotnet-ef --version 8.0.*
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /src/EnergyPlanner
ENTRYPOINT ["dotnet", "ef", "database", "update", "--configuration", "Release"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EnergyPlanner.dll"]
