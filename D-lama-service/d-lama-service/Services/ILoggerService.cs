namespace d_lama_service.Services
{
    public interface ILoggerService
    {
        void LogInformation(int userId, string message);
        void LogException(Exception ex);
    }
}
