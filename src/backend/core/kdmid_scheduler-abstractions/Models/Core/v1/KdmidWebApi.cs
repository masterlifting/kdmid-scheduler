namespace KdmidScheduler.Abstractions.Models.Core.v1.KdmidWebApi;

public sealed record CityGetDto(string Code, string Name);
public sealed record CommandGetDto(string Id, string City, string KdmidId, string KdmidCd, string? KdmidEms, byte Attempts);
public sealed record CommandSetDto(string CityCode, string KdmidId, string KdmidCd, string? KdmidEms);

