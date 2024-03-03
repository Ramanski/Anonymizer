
using System.IO;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Anonymizer
{
    public class AnonymizerMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var originalBody = context.Response.Body;
            var stream = new MemoryStream();
            context.Response.Body = stream;               

            await next(context);

            stream.Seek(0, SeekOrigin.Begin);
            var originalReader = new StreamReader(stream);
            var originalContent = await originalReader.ReadToEndAsync(); // Reading first request

            Regex hasUserToAnon = new Regex("\"(xnuc)\":\"(X12345)\"", RegexOptions.IgnoreCase);

            if (hasUserToAnon.IsMatch(originalContent))
            {
                //My Custom Response Class
                var overringNewResponse = hasUserToAnon
                    .Replace(originalContent, "\"$1\":\"X00000\"");
                overringNewResponse = new Regex("\"(userName)\":\"[^\"]*\"").Replace(overringNewResponse, "\"$1\":\"ANONYM\"");

                var newVal = JsonSerializer.Deserialize<List<WeatherForecast>>(overringNewResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web));

                //converting my custom response class to jsontype
                var json = JsonSerializer.Serialize(overringNewResponse);

                //Modifying existing stream
                context.Response.Body = originalBody;
                await context.Response.WriteAsync(json);
            }
        }
    }
}
