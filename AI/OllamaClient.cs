using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AICardMod.AI;

public static class OllamaClient
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(20) };
    private const string ApiUrl = "http://ollama.yeci.lol/api/generate";
    private const string Model = "qwen2.5:3b";

    public static async Task<string> AskAsync(string prompt)
    {
        var body = JsonSerializer.Serialize(new { model = Model, prompt, stream = false });
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        try
        {
            var response = await _http.PostAsync(ApiUrl, content);
            var json = await response.Content.ReadAsStringAsync();
            global::Godot.GD.Print($"[AICard] Raw response: {json}");

            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Ollama success: { "response": "..." }
            if (root.TryGetProperty("response", out var resp))
                return resp.GetString() ?? "";

            // Ollama error: { "error": "..." }
            if (root.TryGetProperty("error", out var err))
                global::Godot.GD.PrintErr($"[AICard] Ollama API error: {err.GetString()}");
            else
                global::Godot.GD.PrintErr($"[AICard] Unexpected response: {json}");

            return "";
        }
        catch (Exception ex)
        {
            global::Godot.GD.PrintErr($"[AICard] Ollama error: {ex.Message}");
            return "";
        }
    }
}
