using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using System.Text.Json;
using System.Text;
using WebApi_Normal.Config;
using WebApi_Normal.Interfaces;
using WebApi_Normal.Dominio;
using WebApi_Normal.Models.Responses;

namespace WebApi_Normal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SmsController : ControllerBase
    {


        private readonly ITecnicoRepository _tecnicos;
        private readonly ISmsService _sms;
        private readonly IIncidenteProvider _incidentes;

        public SmsController(ITecnicoRepository tecnicos, ISmsService sms, IIncidenteProvider incidentes)
        {
            _tecnicos = tecnicos;
            _sms = sms;
            _incidentes = incidentes;
        }

        [HttpGet("/sms_api")]
        public async Task<IActionResult> ProcesarIncidentes()
        {
            var result = new ProcesamientoIncidentesResponse();
            try
            {
                var tecnicos = _tecnicos.ListaTecnicos();

                List<Incidente> incidentes;
                try
                {
                    incidentes = _incidentes.GetIncidents();
                }
                catch (Exception ex)
                {
                    result.ErroresGenerales.Add($"Error obteniendo incidentes: {ex.Message}");
                    return StatusCode(500, result);
                }

                result.TotalIncidentes = incidentes.Count;

                foreach (var inc in incidentes)
                {
                    var detalle = new IncidenteProcesado
                    {
                        Torre = inc.Torre,
                        Mensaje = inc.Mensaje,
                        Timestamp = inc.Tiempo
                    };

                    var t = tecnicos.FirstOrDefault(x => x.Torre == inc.Torre);
                    if (t == null)
                    {
                        detalle.Estado = "sin_tecnico";
                        detalle.Error = $"No se encontró técnico para la torre {inc.Torre}.";
                        result.SinTecnico++;
                        result.Detalles.Add(detalle);
                        continue;
                    }

                    detalle.Tecnico = t.Nombre;
                    detalle.Telefono = t.Telefono;

                    try
                    {
                        var smsMsg = $"Alerta Torre {inc.Torre}: {inc.Mensaje}";
                        await _sms.EnviarSmsAsync(t.Telefono, smsMsg);
                        detalle.Estado = "enviado";
                        result.Enviados++;
                    }
                    catch (Exception ex)
                    {
                        detalle.Estado = "sms_error";
                        detalle.Error = $"Fallo envío SMS: {ex.Message}";
                        result.FallosSms++;
                    }

                    result.Detalles.Add(detalle);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Cualquier otro error no contemplado
                result.ErroresGenerales.Add($"Error inesperado: {ex.Message}");
                return StatusCode(500, result);
            }
        }

    }
}
