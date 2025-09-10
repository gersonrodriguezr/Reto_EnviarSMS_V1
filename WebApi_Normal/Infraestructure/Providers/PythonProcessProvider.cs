using System.Diagnostics;
using System.Text.Json;
using WebApi_Normal.Config;
using WebApi_Normal.Dominio;
using WebApi_Normal.Interfaces;

namespace WebApi_Normal.Infraestructure.Providers
{
    public class PythonProcessProvider : IIncidenteProvider
    {
        private readonly AppSettings _app;

        public PythonProcessProvider(AppSettings app) => _app = app;

        public List<Incidente> GetIncidents()
        {
            var listaIncidentes = new List<Incidente>();

            foreach (var server in _app.Servers)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = server.PythonPath,
                        Arguments = server.ScriptPath,
                        WorkingDirectory = Path.GetDirectoryName(server.ScriptPath),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var proc = Process.Start(psi);
                    if (proc == null)
                    {
                        Console.WriteLine($"[ERROR] No se pudo iniciar Python en {server.Host}");
                        return listaIncidentes;
                    }


                    string output = proc.StandardOutput.ReadToEnd();
                    string error = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();

                    Console.WriteLine($"[DEBUG] Ejecutando: {psi.FileName} {psi.Arguments}");
                    Console.WriteLine($"[DEBUG] WorkingDirectory: {psi.WorkingDirectory}");
                    Console.WriteLine($"[DEBUG] Output: {output}");
                    Console.WriteLine($"[DEBUG] Error: {error}");

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        Console.WriteLine($"[Python error] {error}");
                    }


                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        var lista = JsonSerializer.Deserialize<List<Incidente>>(output);
                        if (lista != null)
                        {
                            listaIncidentes.AddRange(lista);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR en {server.Host}] {ex.Message}");
                    throw;
                }
                

            }

            return listaIncidentes;
        }
    }
}
