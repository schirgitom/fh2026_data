# Binary Payload Format

All MQTT messages are expected to carry a binary payload in the following format:

- `byte` Version (currently `1`)
- `byte[16]` Aquarium Id (GUID bytes, little-endian)
- `int64` UTC ticks (`DateTimeOffset.Ticks`)
- `byte` Metric count
- Repeated Metric entries:
  - `byte` Metric type id
  - `double` Metric value (IEEE 754)
  - `byte` Unit code

Metric type ids and unit codes are mapped in code under:

- `src/AquariumData2026.Infrastructure/Decoding/BinaryMetricMappings.cs`

Unsupported versions, metric types, or truncated payloads are rejected with errors logged by the ingestion pipeline.
