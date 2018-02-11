namespace TestWebApiAzure
{
    public class AppSettings
    {
        public string AppName { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }

    public class ConnectionStrings
    {
        public string TestWebApiDb { get; set; }
    }
}
