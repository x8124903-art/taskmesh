FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MsAuth.csproj", "./"]
RUN dotnet restore "MsAuth.csproj"

COPY . .
RUN dotnet build "MsAuth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MsAuth.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MsAuth.dll"]
