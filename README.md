# AquariumData2026

AquariumData2026 is a Clean Architecture .NET 9 service that ingests telemetry from an MQTT broker,
decodes binary payloads, and publishes normalized measurements to RabbitMQ.

## Solution Structure

- `src/AquariumData2026.Api` - ASP.NET Core host and background service.
- `src/AquariumData2026.Application` - Orchestration and abstraction layer.
- `src/AquariumData2026.Domain` - Core domain model.
- `src/AquariumData2026.Infrastructure` - MQTT, RabbitMQ, registry API, and decoding implementations.

## Configuration

All configuration lives in `src/AquariumData2026.Api/appsettings.json`.
By default, the service loads all aquariums from the central API (`RegistryApi`):

- `RegistryApi:BaseUrl` (default `http://localhost:5011/`)
- `RegistryApi:FreshWaterAquariumsPath`
- `RegistryApi:SeaWaterAquariumsPath`

MQTT topics are then generated from `Topics:TopicTemplate` using the loaded aquarium ids.

## Run

```bash
dotnet run --project src/AquariumData2026.Api
```

## Metrics

- The service exposes Prometheus metrics at `GET /metrics`.
- A ready-to-use Prometheus config is available at `prometheus.yml` (scrapes `localhost:37820`).

## Documentation

- `docs/Architecture.md`
- `docs/BinaryPayload.md`
