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
For demonstration purposes, MQTT topics are hard-coded via the `Topics` section.

## Run

```bash
dotnet run --project src/AquariumData2026.Api
```

## Documentation

- `docs/Architecture.md`
- `docs/BinaryPayload.md`
