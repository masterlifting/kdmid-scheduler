namespace KdmidScheduler.Abstractions.Models.Core.v1.BotApiDto;

public sealed record CommandGetDto(string Id, string Name, string City, string KdmidId, string KdmidCd, string? KdmidEms, byte Attempts);
public sealed record CommandSetDto(string Name, string CityCode, string KdmidId, string KdmidCd, string? KdmidEms);

