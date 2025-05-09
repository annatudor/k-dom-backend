using System.Data;
using MySql.Data.MySqlClient;

namespace KDomBackend.Data
{
    public class DatabaseContext
    {
        private readonly string _connectionString;
        public DatabaseContext(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
