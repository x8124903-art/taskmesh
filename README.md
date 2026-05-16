# TaskMesh

Plataforma de gestión de proyectos distribuida (microservicios .NET 8, MySQL, RabbitMQ, Redis, Blazor)

## Quick Deployment (sin clonar el repositorio)

Solo necesitas **un fichero** (`docker-compose.ghcr.yml`) y opcionalmente un `.env`:

```bash
# Descargar el compose
curl -O https://raw.githubusercontent.com/x8124903-art/taskmesh/main/docker-compose.ghcr.yml

# (Opcional) Crear .env para personalizar contraseñas - si no, usa valores por defecto
# Ver .env.example para referencia

# Levantar todo
docker compose -f docker-compose.ghcr.yml up -d
```

### Acceso

- **Web App**: http://localhost:3000
- **API Gateway**: http://localhost:5000
- **Grafana**: http://localhost:3001 (admin/admin)
- **Jaeger (tracing)**: http://localhost:16686
- **RabbitMQ Management**: http://localhost:15672 (taskmesh/password)
- **Prometheus**: http://localhost:9090

### Parar y limpiar

```bash
docker compose -f docker-compose.ghcr.yml down     # parar
docker compose -f docker-compose.ghcr.yml down -v   # parar y borrar datos
```

## Development (con el repositorio clonado)

```bash
git clone https://github.com/x8124903-art/taskmesh.git
cd taskmesh
docker compose up -d
```

## Architecture

- **Backend**: .NET 8 microservices (MsAuth, MsProjects, MsTasks, MsNotifications, MsGateway)
- **Frontend**: Blazor WebAssembly
- **Infrastructure**: MySQL, Redis, RabbitMQ, Jaeger, Prometheus, Grafana

