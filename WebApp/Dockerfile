FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["webapp.csproj", "./"]
RUN dotnet restore "webapp.csproj"

COPY . .
RUN dotnet build "webapp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "webapp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

COPY --from=publish /app/publish/wwwroot .

COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
