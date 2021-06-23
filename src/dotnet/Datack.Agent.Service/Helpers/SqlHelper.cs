using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Datack.Common.Models.Internal;

namespace Datack.Agent.Service.Helpers
{
    public static class SqlHelper
    {
        private static String GetConnectionString(ServerDbSettings serverDbSettings, Int32 timeout = 60000)
        {
            return $"Server={serverDbSettings.Server};User Id={serverDbSettings.UserName};Password={serverDbSettings.Password};Timeout={timeout}";
        }

        /// <summary>
        /// Test a databas connection.
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

        public static async Task<IList<String>> GetDatabaseList(ServerDbSettings serverDbSettings)
        {
            return await GetSingleResultList<String>("SELECT name from sys.databases", serverDbSettings);
        }

        public static async Task<IList<T>> GetSingleResultList<T>(String query, ServerDbSettings serverDbSettings)
        {
            await using var sqlConnection = new SqlConnection(GetConnectionString(serverDbSettings, 1000));

            await sqlConnection.OpenAsync();

            await using var cmd = new SqlCommand("SELECT name from sys.databases", sqlConnection);

            await using var dr = await cmd.ExecuteReaderAsync();

            var resultList = new List<T>();
            while (dr.Read())
            {
                resultList.Add((T) dr[0]);
            }

            return resultList;
        }
    }
}
