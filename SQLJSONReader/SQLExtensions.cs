
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text.Json;
using SQLJSONReader;

namespace SQLJSON.Extensions
{
    public static class SQLExtensions
    {
        /// <summary>
        /// Microsoft doesn't handle JSON well, it wants to use XML methods which then retrieve
        /// the JSON. Sometimes the JSON has characters in it which 
        /// </summary>
        public static SqlJSONReader ExecuteJsonReader(this SqlCommand cmd)
            => new SqlJSONReader(cmd.ExecuteReader());

        public static async Task<SqlJSONReader> ExecuteJsonReaderAsync(this SqlCommand cmd)
            => new SqlJSONReader( await cmd.ExecuteReaderAsync());

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
            
            public override int Peek() => GetChar(false);

            public async Task<int> PeekAsync() => await GetCharAsync(false);
            
            public override int Read() => GetChar(true);

            public async Task<int> ReadAsync() => await GetCharAsync(true);

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

                return ProcessResult(sbResult);
                
            }
            /// <summary>
            /// Handle the result whether in JSON or not. If JSON, the escapes will be removed.
            /// </summary>
            /// <param name="sbResult">The stringbuilder with the text.</param>
            /// <returns>Final Output string</returns>
            private static string ProcessResult(StringBuilder sbResult)
            {
             
                sbResult.Replace("\\\t", "\t");
                sbResult.Replace("\\\n", "\n");
                sbResult.Replace("\\\r", "\r");
             
                //var finalResult = sbResult.ToString();

                //if (!string.IsNullOrWhiteSpace(finalResult))
                //{
                //    try // Clean up any JSON escapes before returning
                //    {


                //        //finalResult = JsonConvert.DeserializeObject(finalResult).ToString();
                //        finalResult = JsonDocument.Parse(finalResult).ToJsonString();
                //    }
                //    catch (System.Exception) { } // Ignore standard text or invalid json returned.
                //}

                return sbResult.ToString();
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

               return ProcessResult(sbResult);
            }

            public override void Close() => SqlReader?.Close();
            
            
        }

    }
}
