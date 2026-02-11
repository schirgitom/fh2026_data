# Architecture Overview

The service follows Clean Architecture with explicit boundaries:

- Domain: Core model definitions (`Aquarium`, `Measurement`, `MetricValue`).
- Application: Use-case orchestration and abstractions (`IMqttSubscriber`, `IMessagePublisher`).
- Infrastructure: External systems integration (MQTT, RabbitMQ, registry API, decoding).
- API: ASP.NET Core host and background worker.

The ingestion pipeline is:

1. Startup queries the registry API for aquariums.
2. Topic resolution is performed via `ITopicProvider`.
3. MQTT subscription streams binary payloads.
4. Payloads are decoded into measurement DTOs.
5. Measurements are published to RabbitMQ for persistence.

Logging is included at all key stages to support observability and operational diagnostics.
