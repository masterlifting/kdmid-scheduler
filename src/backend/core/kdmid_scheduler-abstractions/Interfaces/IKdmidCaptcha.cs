namespace KdmidScheduler.Abstractions.Interfaces;

public interface IKdmidCaptcha
{
    Task<string> SolveIntegerCaptcha(byte[] image, CancellationToken cToken);
}
