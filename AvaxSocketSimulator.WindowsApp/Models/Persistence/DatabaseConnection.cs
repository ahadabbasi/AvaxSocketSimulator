using System.Data.SqlClient;

namespace AvaxSocketSimulator.WindowsApp.Models.Persistence
{
    public class DatabaseConnection
    {

        private static DatabaseConnection _instance;

        public static DatabaseConnection Instance()
        {
            if (_instance == null)
            {
                _instance = new DatabaseConnection();
            }

            return _instance;
        }


        private readonly SqlConnectionStringBuilder _builder;

        private DatabaseConnection() 
        {
            _builder = new SqlConnectionStringBuilder()
            {
                DataSource = ".\\SQLEXPRESS",
                InitialCatalog = "AvaxPositionSocketSystem",
                UserID = "sa",
                Password = "123@123.com",
                TrustServerCertificate = true
            };
        }


        public string ConnectionString()
        {
            return _builder.ConnectionString;
        }
    }
}
