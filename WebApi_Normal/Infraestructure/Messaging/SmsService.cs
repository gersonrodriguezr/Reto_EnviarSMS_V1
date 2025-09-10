using System.Text.Json;
using System.Text;
using WebApi_Normal.Interfaces;
using static System.Net.WebRequestMethods;
using WebApi_Normal.Config;
using WebApi_Normal.Models.Dtos;

namespace WebApi_Normal.Infraestructure.Messaging
{
    public class SmsService : ISmsService
    {
        private readonly AppSettings _app;
        private readonly HttpClient _http;

        public SmsService(AppSettings app, HttpClient http)
        {
            _app = app;
            _http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(Math.Max(5,_app.Sms.TimeoutSeconds))
            };
        }

        public async Task EnviarSmsAsync(string numero, string mensaje)
        {
            var dto = new SmsRequestDto { Mensaje = mensaje, Numero = numero }; 
            var payload = JsonSerializer.Serialize(dto);

            using var req = new HttpRequestMessage(HttpMethod.Post, _app.Sms.Endpoint);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_app.Sms.User}:{_app.Sms.Pass}")));
            req.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(req, CancellationToken.None);
            var body = await resp.Content.ReadAsStringAsync();

            Console.WriteLine($"[SMS] numero={numero} status={(int)resp.StatusCode} body={body}");
            resp.EnsureSuccessStatusCode();
        }
    }
}
