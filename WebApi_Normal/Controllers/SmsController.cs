using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using System.Text.Json;
using System.Text;
using WebApi_Normal.Config;

namespace WebApi_Normal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SmsController : ControllerBase
    {
        private readonly AppSettings _app;

        public SmsController(AppSettings app)
        {
            _app = app;
        }

        [HttpGet("/sms_api")]
        public async Task<IActionResult> ProcesarEventos()
        {
            Console.WriteLine("Inicio ProcesarEventos");

            var tecnicos = LoadTecnicos();

            List<Dictionary<string, string>> eventos;

            if (OperatingSystem.IsWindows())
            {
                eventos = ObtenerEventosWindows();
            }
            else
            {
                eventos = ObtenerEventosLinux();
            }

            var errores = new List<string>();
            var enviados = new List<string>();

            foreach (var ev in eventos)
            {
                if (!ev.ContainsKey("torre") || !ev.ContainsKey("msg"))
                    continue;

                var torre = ev["torre"];
                if (!tecnicos.TryGetValue(torre, out var phone))
                {
                    Console.WriteLine($"No se encontró técnico para torre {torre}");
                    continue;
                }

                var mensaje = $"Alerta Torre {torre}: {ev["msg"]}";

                try
                {
                    await EnviarSms(phone, mensaje);
                    enviados.Add($"SMS enviado a {phone}");
                }
                catch (Exception ex)
                {
                    errores.Add($"Error enviando SMS a {phone}: {ex.Message}");
                }
            }

            if (errores.Any())
            {
                return StatusCode(500, new
                {
                    Mensaje = "Se procesaron eventos, pero ocurrieron errores al enviar SMS.",
                    Enviados = enviados,
                    Errores = errores
                });
            }

            return Ok(new
            {
                Mensaje = "Eventos procesados y SMS enviados correctamente.",
                Enviados = enviados
            });
        }

        private async Task EnviarSms(string numero, string mensaje)
        {
            var payload = JsonSerializer.Serialize(new { numero, mensaje });

            var req = new HttpRequestMessage(HttpMethod.Post, _app.Sms.Endpoint);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_app.Sms.User}:{_app.Sms.Pass}")));
            req.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var resp = await client.SendAsync(req);
            var respBody = await resp.Content.ReadAsStringAsync();

            Console.WriteLine($"SMS enviado a {numero}, status={resp.StatusCode}, respuesta={respBody}");
        }

        // ---------------- FUNCIONES PRIVADAS ----------------
        private Dictionary<string, string> LoadTecnicos()
        {
            var map = new Dictionary<string, string>();
            if (!System.IO.File.Exists(_app.CsvPath)) return map;
            var lines = System.IO.File.ReadAllLines(_app.CsvPath);
            foreach (var line in lines.Skip(1))
            {
                var cols = line.Split(',');
                if (cols.Length < 5) continue;
                var torre = cols[4].Trim();
                var phone = cols[2].Trim();
                if (!string.IsNullOrEmpty(torre) && !string.IsNullOrEmpty(phone))
                    map[torre] = phone;
            }
            return map;
        }

        private List<Dictionary<string, string>> ObtenerEventosLinux()
        {
            var eventos = new List<Dictionary<string, string>>();
            foreach (var server in _app.Servers)
            {
                using var client = new SshClient(server.Host, server.User, server.Password);
                try
                {
                    client.Connect();

                    var cmd = client.RunCommand($"{_app.PythonPath} {_app.ScriptPath}");// ya que se puede usar el ssh

                    if (!string.IsNullOrWhiteSpace(cmd.Result))
                    {
                        var lista = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(cmd.Result);
                        
                        if (lista != null)
                        {
                            eventos.AddRange(lista);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error conectando a {server.Host}: {ex.Message}");
                }
                finally
                {
                    if (client.IsConnected) client.Disconnect();
                }
            }
            return eventos;
        }

        private List<Dictionary<string, string>> ObtenerEventosWindows()
        {
            var eventos = new List<Dictionary<string, string>>();

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _app.PythonPath,
                    Arguments = _app.ScriptPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine($"[ERROR Python] {error}");
                }

                if (!string.IsNullOrWhiteSpace(output))
                {
                    var lista = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(output);
                    if (lista != null)
                    {
                        eventos.AddRange(lista);
                    }      
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ejecutando Python local: {ex.Message}");
            }

            return eventos;
        }

    }
}
