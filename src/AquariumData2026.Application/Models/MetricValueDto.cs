using AquariumData2026.Domain.Model;

namespace AquariumData2026.Application.Models;

/// <summary>
/// Serialized metric value ready for transport.
/// </summary>
public sealed record MetricValueDto(MetricType Type, decimal Value, string Unit);
