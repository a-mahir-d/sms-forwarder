using System.Text.Json;
using sms_forwarder.Models;

namespace sms_forwarder.Services;

public static class SettingsService
{
    private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "settings.json");
    private static AppSettings Current { get; set; }
    private static readonly Lock FileLock = new ();
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static void Initialize()
    {
        lock (FileLock)
        {
            if (!File.Exists(FilePath))
            {
                Current = new AppSettings();
                WriteToFile();
            }
            else
            {
                try
                {
                    var json = File.ReadAllText(FilePath);
                    Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    Current = new AppSettings();
                }
            }
        }
    }

    public static AppSettings Get() => Current;
    
    public static void Update(AppSettings newSettings)
    {
        Current = newSettings;
        WriteToFile();
    }
    
    private static void WriteToFile()
    {
        lock (FileLock)
        {
            try
            {
                var json = JsonSerializer.Serialize(Current, JsonSerializerOptions);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hata: {ex.Message}");
            }
        }
    }
}