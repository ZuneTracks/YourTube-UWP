using System;
using System.Diagnostics;
using Windows.Storage;

namespace YouTube.Uwp.Services
{
    internal static class DiagnosticLog
    {
        private const string LogKey = "DiagnosticLog";
        private const int MaximumCharacters = 6000;

        public static void Write(string source, string message)
        {
            try
            {
                string entry = DateTimeOffset.UtcNow.ToString("u")
                    + " | "
                    + source
                    + " | "
                    + Redact(message);
                object storedValue;
                string existing = ApplicationData.Current.LocalSettings.Values.TryGetValue(LogKey, out storedValue)
                    ? storedValue as string
                    : string.Empty;
                string updated = string.IsNullOrEmpty(existing) ? entry : existing + Environment.NewLine + entry;
                if (updated.Length > MaximumCharacters)
                {
                    updated = updated.Substring(updated.Length - MaximumCharacters);
                }

                ApplicationData.Current.LocalSettings.Values[LogKey] = updated;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("YourTube diagnostic write failed: " + exception.HResult.ToString("X8"));
            }
        }

        public static void WriteException(string source, Exception exception)
        {
            if (exception == null)
            {
                Write(source, "Exception information was unavailable.");
                return;
            }

            Write(
                source,
                "Exception "
                + exception.GetType().FullName
                + " (0x"
                + exception.HResult.ToString("X8")
                + ").");
        }

        public static string Read()
        {
            object storedValue;
            return ApplicationData.Current.LocalSettings.Values.TryGetValue(LogKey, out storedValue)
                ? storedValue as string ?? string.Empty
                : string.Empty;
        }

        public static void Clear()
        {
            ApplicationData.Current.LocalSettings.Values.Remove(LogKey);
        }

        private static string Redact(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("access_token", "[redacted]")
                .Replace("refresh_token", "[redacted]")
                .Replace("client_secret", "[redacted]")
                .Replace("device_code", "[redacted]")
                .Replace("user_code", "[redacted]");
        }
    }
}
