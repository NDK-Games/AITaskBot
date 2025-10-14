using Newtonsoft.Json.Linq;

namespace TaskManager;

public static class TelegramAPI
{
    public static string Token => Environment.TelegramBotToken;

    public static async Task<string> GetUserFullName(long userId)
    {
        using HttpClient client = new HttpClient();
        string url = $"https://api.telegram.org/bot{Token}/getChat?chat_id={userId}";

        HttpResponseMessage response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return "Unknown";

        string json = await response.Content.ReadAsStringAsync();
        JObject data = JObject.Parse(json);

        if (data["ok"]?.Value<bool>() != true) return "Unknown";

        string firstName = data["result"]["first_name"]?.ToString() ?? "";
        string lastName = data["result"]["last_name"]?.ToString() ?? "";

        return string.IsNullOrEmpty(firstName) ? "Unknown" : $"{firstName} {lastName}".Trim();
    }
}