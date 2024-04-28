using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

public class Weather
{
    public DateTime Date { get; set; }
    public double UserTemperature { get; set; }
    public int UserHumidity { get; set; }
    public double YrnoTemperature { get; set; }
    public int YrnoHumidity { get; set; }
}

public class WeatherLog
{
    public List<Weather> WeatherData { get; }

    public WeatherLog()
    {
        WeatherData = new List<Weather>();
    }

    public void AddWeatherData(Weather weather)
    {
        WeatherData.Add(weather);
    }

    public void SaveWeatherLogToJson(string filePath)
    {
        string json = JsonConvert.SerializeObject(WeatherData, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
}

public static class WeatherApi
{
    public static async Task<Weather> GetWeatherDataFromApi(string apiUrl)
    {
        Weather weather = new Weather();

        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    dynamic apiResponse = JsonConvert.DeserializeObject(json);
                    weather.YrnoTemperature = apiResponse.temperature;
                    weather.YrnoHumidity = apiResponse.humidity;
                }
                else
                {
                    Console.WriteLine("Failed to fetch data from the API. Status Code: " + response.StatusCode);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("An error occurred while fetching data from the API: " + ex.Message);
        }

        return weather;
    }
}

public class ReportGenerator
{
    public static double CalculateDifference(double userValue, double yrnoValue)
    {
        return userValue - yrnoValue;
    }

    public static void GenerateDayReport(Weather weather)
    {
        double tempDiff = CalculateDifference(weather.UserTemperature, weather.YrnoTemperature);
        int humidityDiff = weather.UserHumidity - weather.YrnoHumidity;

        Console.WriteLine("Day Report:");
        Console.WriteLine($"User Temperature: {weather.UserTemperature}°C, Yr.no Temperature: {weather.YrnoTemperature}°C, Difference: {tempDiff}°C");
        Console.WriteLine($"User Humidity: {weather.UserHumidity}%, Yr.no Humidity: {weather.YrnoHumidity}%, Difference: {humidityDiff}%");
    }

    public static void GenerateWeekReport(List<Weather> weatherData)
    {
        
        double avgUserTemp = weatherData.Average(w => w.UserTemperature);
        double avgYrnoTemp = weatherData.Average(w => w.YrnoTemperature);
        int avgUserHumidity = (int)weatherData.Average(w => w.UserHumidity);
        int avgYrnoHumidity = (int)weatherData.Average(w => w.YrnoHumidity);

        Console.WriteLine("Week Report:");
        Console.WriteLine($"Average User Temperature: {avgUserTemp}°C, Average Yr.no Temperature: {avgYrnoTemp}°C");
        Console.WriteLine($"Average User Humidity: {avgUserHumidity}%, Average Yr.no Humidity: {avgYrnoHumidity}%");
    }

    public static void GenerateMonthReport(List<Weather> weatherData)
    {
        
        double avgUserTemp = weatherData.Average(w => w.UserTemperature);
        double avgYrnoTemp = weatherData.Average(w => w.YrnoTemperature);
        int avgUserHumidity = (int)weatherData.Average(w => w.UserHumidity);
        int avgYrnoHumidity = (int)weatherData.Average(w => w.YrnoHumidity);

        Console.WriteLine("Month Report:");
        Console.WriteLine($"Average User Temperature: {avgUserTemp}°C, Average Yr.no Temperature: {avgYrnoTemp}°C");
        Console.WriteLine($"Average User Humidity: {avgUserHumidity}%, Average Yr.no Humidity: {avgYrnoHumidity}%");
    }
}

class Program
{
    static async Task Main()
{
    WeatherLog weatherLog = new WeatherLog();

    Console.WriteLine("Type in your weather measurements for today:");
    Console.Write("Temperature (°C): ");
    double userTemp = Convert.ToDouble(Console.ReadLine());
    Console.Write("Humidity (%): ");
    int userHumidity = Convert.ToInt32(Console.ReadLine());


    string apiUrl = "https://api.met.no/weatherapi/locationforecast/2.0/compact?lat=60.10&lon=9.58";
    Weather yrnoWeather = await WeatherApi.GetWeatherDataFromApi(apiUrl);

    Weather todayWeather = new Weather
    {
        Date = DateTime.Today,
        UserTemperature = userTemp,
        UserHumidity = userHumidity,
        YrnoTemperature = yrnoWeather.YrnoTemperature,
        YrnoHumidity = yrnoWeather.YrnoHumidity
    };
    weatherLog.AddWeatherData(todayWeather);
    weatherLog.SaveWeatherLogToJson("weather_log.json");

    
    ReportGenerator.GenerateDayReport(todayWeather);

    
    List<Weather> weekWeatherData = new List<Weather>();
    for (int i = 0; i < 7; i++)
    {
        Weather weekYrnoWeather = await WeatherApi.GetWeatherDataFromApi(apiUrl);
        weekWeatherData.Add(weekYrnoWeather);
    }

    foreach (var weather in weekWeatherData)
    {
        weatherLog.AddWeatherData(weather);
    }

    ReportGenerator.GenerateWeekReport(weekWeatherData);

    List<Weather> monthWeatherData = new List<Weather>();
    for (int i = 0; i < 30; i++)
    {
        Weather monthYrnoWeather = await WeatherApi.GetWeatherDataFromApi(apiUrl);
        monthWeatherData.Add(monthYrnoWeather);
    }

    foreach (var weather in monthWeatherData)
    {
        weatherLog.AddWeatherData(weather);
    }

    ReportGenerator.GenerateMonthReport(monthWeatherData);
}
}