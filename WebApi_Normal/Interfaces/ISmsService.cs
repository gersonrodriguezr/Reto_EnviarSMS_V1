namespace WebApi_Normal.Interfaces
{
    public interface ISmsService
    {
        Task EnviarSmsAsync(string numero, string mensaje);
    }
}
