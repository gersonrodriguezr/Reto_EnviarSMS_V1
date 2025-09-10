using Renci.SshNet;
using System.Text.Json;
using WebApi_Normal.Config;
using WebApi_Normal.Dominio;
using WebApi_Normal.Interfaces;

namespace WebApi_Normal.Infraestructure.Providers
{
    public class PythonSshProvider : IIncidenteProvider
    {
        private readonly AppSettings _app;

        public PythonSshProvider(AppSettings app) => _app = app;

        public List<Incidente> GetIncidents()
        {
            var listaIncidentes = new List<Incidente>();
            foreach (var server in _app.Servers)
            {
                using var client = new SshClient(server.Host, server.User, server.Password);
                try
                {
                    client.Connect();

                    var cmd = client.RunCommand($"{server.PythonPath} {server.ScriptPath}");// ya que se puede usar el ssh

                    if (!string.IsNullOrWhiteSpace(cmd.Result))
                    {
                        var lista = JsonSerializer.Deserialize<List<Incidente>>(cmd.Result);

                        if (lista != null)
                        {
                            listaIncidentes.AddRange(lista);
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
            return listaIncidentes;
        }
    }
}
