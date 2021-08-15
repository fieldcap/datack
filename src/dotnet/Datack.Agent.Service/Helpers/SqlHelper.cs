using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Datack.Common.Models.Internal;
using Dapper;

namespace Datack.Agent.Service.Helpers
{
    public static class SqlHelper
    {
        private static String GetConnectionString(ServerDbSettings serverDbSettings, Int32 timeout = 60000)
        {
            return $"Server={serverDbSettings.Server};User Id={serverDbSettings.UserName};Password={serverDbSettings.Password};Timeout={timeout}";
        }

        /// <summary>
        /// Test a database connection.
        /// </summary>
        /// <param name="serverDbSettings">The server database settings.</param>
        /// <returns>"Success" if successful, otherwise an error message.</returns>
        public static async Task<String> TestDatabaseConnection(ServerDbSettings serverDbSettings)
        {
            await using var sqlConnection = new SqlConnection(GetConnectionString(serverDbSettings, 1000));

            try
            {
                await sqlConnection.OpenAsync();

                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Get a list of all databases from the connectionstring.
        /// </summary>
        /// <param name="serverDbSettings"></param>
        /// <returns></returns>
        public static async Task<IList<DatabaseList>> GetDatabaseList(ServerDbSettings serverDbSettings)
        {
            var connection = new SqlConnection(GetConnectionString(serverDbSettings, 1000));

            var result = await connection.QueryAsync<DatabaseList>("SELECT name as 'DatabaseName', 1 as 'HasAccess' FROM sys.databases");

            return result.ToList();
        }
    }
}
