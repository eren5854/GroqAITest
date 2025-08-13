using DotNetEnv;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main()
    {// Proje kökünden .env dosyasını bul ve yükle
        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string envFilePath = Path.Combine(projectRoot, ".env");

        if (!File.Exists(envFilePath))
        {
            Console.WriteLine($".env dosyası bulunamadı: {envFilePath}");
            return;
        }

        Env.Load(envFilePath);

        // Ortam değişkeninden API key al
        string apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
        string model = "openai/gpt-oss-120b";

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("API key bulunamadı! .env dosyasına GROQ_API_KEY ekleyin.");
            return;
        }


        var messages = new List<object>(); // Sohbet geçmişi

        using var client = new HttpClient();
        var url = "https://api.groq.com/openai/v1/chat/completions";

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        Console.WriteLine("Groq Chat'e Hoş Geldin! Çıkmak için 'exit' yaz.");

        while (true)
        {
            Console.Write("\nSen: ");
            string userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput))
                continue;

            if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            // Kullanıcı mesajını listeye ekle
            messages.Add(new { role = "user", content = userInput });

            // API isteği
            var requestBody = new
            {
                model = model,
                messages = messages
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            // API cevabını al
            using var doc = JsonDocument.Parse(responseString);
            string assistantReply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // Asistan cevabını ekrana yaz
            Console.WriteLine($"\nAsistan: {assistantReply}");

            // Asistan mesajını da geçmişe ekle
            messages.Add(new { role = "assistant", content = assistantReply });
        }
    }
}
