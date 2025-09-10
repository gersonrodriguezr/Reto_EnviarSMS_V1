namespace WebApi_Normal.Dominio
{
    public class Tecnico
    {
        public string Nombre { get; set; }
        public string Torre { get; set; }// porque puede ser que luego alguna torre se llame con alguna letra y número.
        public string Telefono { get; set; } // porque puede tener el + adelante del número 
    }
}
