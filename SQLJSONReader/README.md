# SQLJSONReader
An extension to read JSON from SQL server off of a SQLCommand to get data. This package is in Nuget as [SQLJsonReader](https://www.nuget.org/packages/SQLJSONReader/)

Usage, include using `SQLJSON.Extensions;` for the use and addition of `ExecuteJsonReaderAsync` and `ExecuteJsonReader` to read JSON 
from SQL Server after a stored procedure call returns JSON using the `for JSON`.

```
string result;

using (SqlConnection conn = new SqlConnection(connection))
{
    conn.Open();

    using (var cmd = new SqlCommand(sproc, conn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 600 })
    {
        if (parameters != null)
            cmd.Parameters.AddRange(parameters);

        var reader = await cmd.ExecuteJsonReaderAsync();

        result = await reader.ReadAllAsync();
    }
}
```

---

See also [Json-Orm](https://www.nuget.org/packages/JSON-ORM/) Nuget Package which uses this package to process its SQL Server derived json into models.

Note with Json-Orm, the above code can be distilled to

```
var connectionStr = @"Data Source=...";

var jdb = new JsonOrmDatabase(connectionStr);

string raw = jdb.GetRawSQL("[dbo].[GetMyDataInJson]");
```