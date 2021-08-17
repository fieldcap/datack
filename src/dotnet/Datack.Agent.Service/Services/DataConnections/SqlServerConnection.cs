using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Datack.Common.Models.Internal;

namespace Datack.Agent.Services.DataConnections
{
    public class SqlServerConnection
    {
        private readonly Servers _servers;

        public SqlServerConnection(Servers servers)
        {
            _servers = servers;
        }

        private async Task<String> GetConnectionString()
        {
            var server = await _servers.GetServer();
            var dbSettings = server.DbSettings;

#if DEBUG
            dbSettings.ConnectionTimeout = 1000;
#endif

            return BuildConnectionString(dbSettings);
    }
        

        private static String BuildConnectionString(ServerDbSettings dbSettings)
        {
            return $"Server={dbSettings};User Id={dbSettings.UserName};Password={dbSettings.Password};Timeout={dbSettings.ConnectionTimeout}";
        }
        
        public async Task Test(ServerDbSettings serverDbSettings)
        {
            String connectionString;
            if (serverDbSettings == null)
            {
                connectionString = await GetConnectionString();
            }
            else
            {
                connectionString = BuildConnectionString(serverDbSettings);
            }

            await using var sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
        }

        public async Task<IList<Database>> GetDatabaseList()
        {
            await using var sqlConnection = new SqlConnection(await GetConnectionString());

            var result = await sqlConnection.QueryAsync<Database>(@"SELECT 
	name AS 'DatabaseName', 
	HAS_PERMS_BY_NAME(name, 'DATABASE', 'BACKUP DATABASE') AS 'HasAccess' 
FROM 
	sys.databases");

            return result.ToList();
        }

        public async Task<IList<File>> GetFileList()
        {
            await using var sqlConnection = new SqlConnection(await GetConnectionString());

            var result = await sqlConnection.QueryAsync<File>(@"SELECT 
	DB_NAME(database_id) AS 'DatabaseName',
	[type] as 'Type',
	[physical_name] as 'PhysicalName',
	size as 'Size'
FROM
	sys.master_files
ORDER BY 
	DB_NAME(database_id)");

            return result.ToList();
        }
    }
}
