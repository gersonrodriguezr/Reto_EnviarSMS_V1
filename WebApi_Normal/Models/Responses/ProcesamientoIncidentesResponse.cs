namespace WebApi_Normal.Models.Responses
{
    public class ProcesamientoIncidentesResponse
    {
        public int TotalIncidentes { get; set; }
        public int Enviados { get; set; }
        public int SinTecnico { get; set; }
        public int FallosSms { get; set; }
        public List<string> ErroresGenerales { get; set; } = new();
        public List<IncidenteProcesado> Detalles { get; set; } = new();
    }

    public class IncidenteProcesado
    {
        public string Torre { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public string Timestamp { get; set; } = "";
        public string? Tecnico { get; set; }
        public string? Telefono { get; set; }
        public string Estado { get; set; } = ""; // "enviado" | "sin_tecnico" | "sms_error"
        public string? Error { get; set; }
    }
}
