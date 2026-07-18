using Microsoft.Data.SqlClient;

namespace UrbanSync.Infrastructure
{

    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection CreateConnection() => new(_connectionString);
    }
}
