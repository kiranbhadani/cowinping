using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoWinPing
{
    class Program
    {
        const string apiUrl = "https://cdn-api.co-vin.in/api/v2/appointment/sessions/calendarByPin";
        
        static async Task Main(string[] args)
        {
            Console.Write("Enter pincode to watch:");
            var pinConde = Console.ReadLine();

            var url = apiUrl + $"?pincode={pinConde}&date={DateTime.Today.ToString("dd-MM-yyyy")}";
            var httpClient = new HttpClient();
            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(content);

                        var availabeSlot = json.SelectTokens("$.centers[*].sessions[?(@.available_capacity_dose1 > 0)]");
                        if (availabeSlot != null && availabeSlot.Count() > 0)
                        {
                            Console.WriteLine("Hurry! slots availabe!");
                            Console.Beep();
                            //break;
                        }
                    }

                    await Task.Delay(5000);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
