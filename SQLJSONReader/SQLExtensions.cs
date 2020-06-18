
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace SQLJSON.Extensions
{
    public static class SQLExtensions
    {
        /// <summary>
        /// Microsoft doesn't handle JSON well, it wants to use XML methods which then retrieve
        /// the JSON. Sometimes the JSON has characters in it which 
        /// </summary>
        public static SqlJSONReader ExecuteJsonReader(this SqlCommand cmd)
        {
            var rdr = cmd.ExecuteReader();
            return new SqlJSONReader(rdr);
        }

        public static async Task<SqlJSONReader> ExecuteJsonReaderAsync(this SqlCommand cmd)
        {
            var rdr = await cmd.ExecuteReaderAsync();
            return new SqlJSONReader(rdr);
        }

        public class SqlJSONReader : System.IO.TextReader
        {
            private SqlDataReader SqlReader { get; set; }
            private string CurrentLine { get; set; }
            private int CurrentPostion { get; set; }

            public SqlJSONReader(SqlDataReader rdr)
            {
                CurrentLine = "";
                CurrentPostion = 0;
                this.SqlReader = rdr;
            }
            public override int Peek()
            {
                return GetChar(false);
            }

            public async Task<int> PeekAsync()
            {
                return await GetCharAsync(false);
            }

            public override int Read()
            {
                return GetChar(true);
            }

            public async Task<int> ReadAsync()
            {
                return await GetCharAsync(true);
            }

            public int GetChar(bool Advance)
            {
                while (CurrentLine.Length == CurrentPostion)
                {
                    if (!SqlReader.Read())
                    {
                        return -1;
                    }
                    CurrentLine = SqlReader.GetString(0);
                    CurrentPostion = 0;
                }
                var rv = CurrentLine[CurrentPostion];
                if (Advance)
                    CurrentPostion += 1;

                return rv;
            }

            public async Task<int> GetCharAsync(bool Advance)
            {
                while (CurrentLine.Length == CurrentPostion)
                {
                    if ((await SqlReader.ReadAsync()) == false)
                    {
                        return -1;
                    }
                    CurrentLine = SqlReader.GetString(0);
                    CurrentPostion = 0;
                }
                var rv = CurrentLine[CurrentPostion];
                if (Advance)
                    CurrentPostion += 1;

                return rv;
            }

            public string ReadAll()
            {
                var sbResult = new StringBuilder();

                if (SqlReader.HasRows)
                {
                    while (SqlReader.Read())
                        sbResult.Append(SqlReader.GetString(0));

                }
                else
                    return string.Empty;

                // Clean up any JSON escapes before returning
                return string.IsNullOrWhiteSpace(sbResult.ToString()) ? string.Empty : JsonConvert.DeserializeObject(sbResult.ToString()).ToString();
            }

            public async Task<string> ReadAllAsync()
            {
                var sbResult = new StringBuilder();

                if (SqlReader.HasRows)
                {
                    while (await SqlReader.ReadAsync())
                        sbResult.Append(SqlReader.GetString(0));

                }
                else
                    return string.Empty;

                // Clean up any JSON escapes before returning
                return string.IsNullOrWhiteSpace(sbResult.ToString()) ? string.Empty : JsonConvert.DeserializeObject(sbResult.ToString()).ToString();
            }

            public override void Close()
            {
                SqlReader.Close();
            }
        }

    }
}
