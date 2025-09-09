using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Business.Utils
{
    public static class DiagnosticsLogger
    {
        public static void Log(string tag, object? payload = null, Exception? ex = null)
        {
            try
            {
                var dir = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, "business-diagnostics.log");

                var sb = new StringBuilder();
                sb.AppendLine($"[{DateTime.UtcNow:o}] {tag}");
                if (payload != null)
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
                        sb.AppendLine("Payload:\n" + json);
                    }
                    catch (Exception serEx)
                    {
                        sb.AppendLine("Payload serialization failed: " + serEx.Message);
                    }
                }

                if (ex != null)
                {
                    sb.AppendLine("Exception:\n" + ex.ToString());
                }

                sb.AppendLine(new string('-', 120));

                File.AppendAllText(path, sb.ToString());
            }
            catch
            {
                // ignore logging errors
            }
        }
    }
}
