namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Services;

public interface IKdmidCaptcha
{
    Task<string> SolveIntegerCaptcha(byte[] image, CancellationToken cToken);
}
