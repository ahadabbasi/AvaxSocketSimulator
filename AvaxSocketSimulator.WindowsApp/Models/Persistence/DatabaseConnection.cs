using System.Data.SqlClient;

namespace AvaxSocketSimulator.WindowsApp.Models.Persistence
{
    public class DatabaseConnection
    {
        private static DatabaseConnection _instance;

        public static DatabaseConnection Instance() => 
            _instance ?? (_instance = new DatabaseConnection());


        private readonly SqlConnectionStringBuilder _builder;

        private DatabaseConnection() 
        {
            _builder = new SqlConnectionStringBuilder
            {
                DataSource = ".\\SQLEXPRESS",
                InitialCatalog = "AvaxPositionSocketSystem",
                UserID = "sa",
                Password = "123@123.com",
                TrustServerCertificate = true
            };
        }


        public string ConnectionString() => 
            _builder.ConnectionString;
    }
}
