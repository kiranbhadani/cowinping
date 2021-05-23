using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoWinPing
{
    class Program
    {
        const string apiUrl = "https://cdn-api.co-vin.in/api/v2/appointment/sessions/calendarByPin";
        const string pinCodeRegEx = "^[1-9][0-9]{5}$";

        static async Task Main(string[] args)
        {
            Console.WriteLine("CoWin Ping");
            Console.WriteLine("------------------------------");
            Console.Write("Enter pincode to watch:");
            var pinConde = Console.ReadLine();
            while (!Regex.IsMatch(pinConde, pinCodeRegEx))
            {
                Console.WriteLine("Please enter valid pincode, 6 digit number");
                pinConde = Console.ReadLine();
            }

            int ageNum;
            Console.Write("Enter age:");
            var age = Console.ReadLine();
            while (!int.TryParse(age, out ageNum) || !(ageNum > 1 && ageNum <= 150))
            {
                Console.WriteLine("Please enter valid age, numbers of years only");
                age = Console.ReadLine();
            }

            Console.Write("Dose 1/2:");
            var dose = Console.ReadKey();
            while (dose.Key != ConsoleKey.D1 && dose.Key != ConsoleKey.D2)
            {
                Console.WriteLine();
                Console.Write("Press number 1 or 2:");
                dose = Console.ReadKey();
            }
            Console.WriteLine();

            var url = apiUrl + $"?pincode={pinConde}&date={DateTime.Today.ToString("dd-MM-yyyy")}";
            var httpClient = new HttpClient();
            Console.WriteLine($"Started... looking slots for {pinConde}. Keep your speakers on to get alert.");
            Console.WriteLine($"Press ESC to stop. It will auto stop after 1 hour.");

            var doesStr = $"available_capacity_dose{dose.KeyChar}";
            var totalSec = 0;
            var runForSec = 60 * 60;

            do
            {
                while (!Console.KeyAvailable && totalSec <= runForSec)
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(content);

                        var centers = json.SelectToken("$.centers");
                        foreach (var center in centers.Children())
                        {
                            var availabeSlot = center.SelectTokens($"$.sessions[?(@.{doesStr}>0 && @.min_age_limit<={ageNum})]");
                            if (availabeSlot != null && availabeSlot.Count() > 0)
                            {
                                Console.WriteLine($"Hurry! {availabeSlot.First().Value<string>(doesStr)} slots available at {center.Value<string>("name")}");
                                Console.Beep();
                                //break;
                            }
                        }
                    }

                    await Task.Delay(5000);
                    totalSec += 5;
                }

                if (totalSec > runForSec)
                {
                    break;
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            Console.WriteLine($"Stopped...");
            Environment.Exit(0);
        }
    }
}
