using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Text.Json;
using mssqlapi.Models;

[ApiController]
[Route("sql")]
public class SqlController : ControllerBase
{
    private readonly IConfiguration _config;

    public SqlController(IConfiguration config)
    {
        _config = config;
    }

    // [HttpPost("exec")]
    // public async Task<ActionResult<SqlResponse>> Execute(
    //     [FromBody] SqlRequest request,
    //     CancellationToken ct)
    // {
    //     // 1️⃣ Decode SQL
    //     string sql;
    //     try
    //     {
    //         var bytes = Convert.FromBase64String(request.SqlBase64);
    //         sql = Encoding.UTF8.GetString(bytes);
    //     }
    //     catch (FormatException)
    //     {
    //         return BadRequest(new
    //         {
    //             error = "InvalidBase64",
    //             message = "Sql not encoded in base64 correctly due to BadRequest"
    //         });
    //     }
    //     catch (Microsoft.Data.SqlClient.SqlException)
    //     {
    //         return BadRequest(new
    //         {
    //             error = "SQL error; ",
    //             message = "Sql not encoded in base64 correctly due to BadRequest"
    //         });

    //         return Ok(new ApiResponse<object> 
    //         {
    //           Success = false,
    //           Error = new ApiError
    //           {
    //               Message = ex.Message,
    //               Code = ex.Number.ToString(),
    //               Details = ex.Procedure // optional
    //           }
    //       });
    //     }

    //     var baseCs = _config.GetConnectionString("Default");
    //     if (string.IsNullOrWhiteSpace(baseCs))
    //         return Problem("Missing ConnectionStrings:Default");

    //     var builder = new SqlConnectionStringBuilder(baseCs);

    //     if (!string.IsNullOrWhiteSpace(request.Database))
    //     {
    //         builder.InitialCatalog = request.Database;
    //     }

    //     await using var conn = new SqlConnection(builder.ConnectionString);
    //     await conn.OpenAsync(ct);
    //     await using var sqlCmd = new SqlCommand(sql, conn)
    //     {
    //         CommandType = CommandType.Text,
    //         CommandTimeout = 30
    //     };

    //     await using var reader = await sqlCmd.ExecuteReaderAsync(ct);

    //     var table = new DataTable();
    //     table.Load(reader);

    //     string output;
    //     string contentType;

    //     if (string.Equals(request.Format, "html", StringComparison.OrdinalIgnoreCase))
    //     {
    //         output = ConvertToHtml(table);
    //         contentType = "text/html; charset=utf-8";
    //     }
    //     else
    //     {
    //         output = ConvertToJson(table);
    //         contentType = "application/json; charset=utf-8";
    //     }

    //     var outputBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(output));

    //     return new SqlResponse(outputBase64, contentType);
    // }

    [HttpPost("exec")]
    public async Task<ActionResult<SqlResponse>> Execute(
        [FromBody] SqlRequest request,
        CancellationToken ct)
    {
        // 1️⃣ Decode SQL
        string sql;
        try
        {
            var bytes = Convert.FromBase64String(request.SqlBase64);
            sql = Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException)
        {
            return BadRequest(new
            {
                error = "InvalidBase64",
                message = "SQL is not valid Base64."
            });
        }

        var baseCs = _config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(baseCs))
            return Problem("Missing ConnectionStrings:Default");

        var builder = new SqlConnectionStringBuilder(baseCs);

        if (!string.IsNullOrWhiteSpace(request.Database))
        {
            builder.InitialCatalog = request.Database;
        }

        try
        {
            await using var conn = new SqlConnection(builder.ConnectionString);
            await conn.OpenAsync(ct);

            await using var sqlCmd = new SqlCommand(sql, conn)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 30
            };

            await using var reader = await sqlCmd.ExecuteReaderAsync(ct);

            var table = new DataTable();
            table.Load(reader);

            string output;
            string contentType;

            if (string.Equals(request.Format, "html", StringComparison.OrdinalIgnoreCase))
            {
                output = ConvertToHtml(table);
                contentType = "text/html; charset=utf-8";
            }
            else
            {
                output = ConvertToJson(table);
                contentType = "application/json; charset=utf-8";
            }

            var outputBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(output));

            return new SqlResponse(outputBase64, contentType);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            // ✅ THIS is where SQL errors belong
            return UnprocessableEntity(new ApiResponse<object>
            {
                Success = false,
                Error = new ApiError
                {
                    Message = ex.Message,
                    Code = ex.Number.ToString(),
                    Details = ex.Procedure
                }
            });
        }
    }

    private static string ConvertToJson(DataTable table)
    {
        var rows = new List<Dictionary<string, object?>>();

        foreach (DataRow row in table.Rows)
        {
            var dict = new Dictionary<string, object?>();

            foreach (DataColumn col in table.Columns)
                dict[col.ColumnName] = row[col];

            rows.Add(dict);
        }

        return JsonSerializer.Serialize(rows);
    }

    private static string ConvertToHtml(DataTable table)
    {
        var sb = new StringBuilder();

        sb.Append("<table border='1'><thead><tr>");
        foreach (DataColumn col in table.Columns)
            sb.Append($"<th>{col.ColumnName}</th>");
        sb.Append("</tr></thead><tbody>");

        foreach (DataRow row in table.Rows)
        {
            sb.Append("<tr>");
            foreach (DataColumn col in table.Columns)
                sb.Append($"<td>{row[col]}</td>");
            sb.Append("</tr>");
        }

        sb.Append("</tbody></table>");

        return sb.ToString();
    }
}