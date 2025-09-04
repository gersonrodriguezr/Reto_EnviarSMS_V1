namespace WebApi_Normal.Config
{
    public class AppSettings
    {
        public string CsvPath { get; set; } = "";
        public string PythonPath { get; set; } = "";
        public string ScriptPath { get; set; } = "";
        public SmsSettings Sms { get; set; } = new();
        public List<ServerConfig> Servers { get; set; } = new();
    }

    public class SmsSettings
    {
        public string Endpoint { get; set; } = "";
        public string User { get; set; } = "";
        public string Pass { get; set; } = "";
    }

    public class ServerConfig
    {
        public string Host { get; set; } = "";
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
    }

}
