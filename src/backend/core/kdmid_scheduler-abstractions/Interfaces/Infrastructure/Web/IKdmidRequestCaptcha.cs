namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Web;

public interface IKdmidRequestCaptcha
{
    Task<string> SolveIntegerCaptcha(byte[] image, CancellationToken cToken);
}
