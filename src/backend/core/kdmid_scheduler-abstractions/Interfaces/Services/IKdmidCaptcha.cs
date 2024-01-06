namespace KdmidScheduler.Abstractions.Interfaces.Services;

public interface IKdmidCaptcha
{
    Task<string> SolveIntegerCaptcha(byte[] image, CancellationToken cToken);
}
