FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MsProjects.csproj", "./"]
RUN dotnet restore "MsProjects.csproj"

COPY . .
RUN dotnet build "MsProjects.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MsProjects.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MsProjects.dll"]
