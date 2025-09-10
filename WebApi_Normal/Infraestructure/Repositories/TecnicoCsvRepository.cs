using System.Collections.Generic;
using WebApi_Normal.Config;
using WebApi_Normal.Dominio;
using WebApi_Normal.Interfaces;

namespace WebApi_Normal.Infraestructure.Repositories
{
    public class TecnicoCsvRepository : ITecnicoRepository
    {
        private readonly AppSettings _app;
        public TecnicoCsvRepository(AppSettings app) => _app = app;

        public List<Tecnico> ListaTecnicos()
        {
            var listaTecnicos = new List<Tecnico>();

            if (!System.IO.File.Exists(_app.CsvPath))
            {
                return listaTecnicos;
            }

            var lines = File.ReadAllLines(_app.CsvPath);

            foreach (var line in lines.Skip(1))
            {
                var cols = line.Split(',');

                if (cols.Length < 5) {
                    continue;
                } 

                listaTecnicos.Add(new Tecnico
                {
                    Nombre = cols[1].Trim(),
                    Telefono = cols[2].Trim(),
                    Torre = cols[4].Trim()
                });
            }
            return listaTecnicos;

        }
    }
}
