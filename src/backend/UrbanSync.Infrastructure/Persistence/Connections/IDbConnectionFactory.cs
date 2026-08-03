using Microsoft.Data.SqlClient;

namespace UrbanSync.Infrastructure.Persistence.Connections;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}