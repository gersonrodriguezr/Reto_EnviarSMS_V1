using WebApi_Normal.Dominio;

namespace WebApi_Normal.Interfaces
{
    public interface IIncidenteProvider
    {
        List<Incidente> GetIncidents();
    }
}
