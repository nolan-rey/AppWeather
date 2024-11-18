using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using Newtonsoft.Json;
using ApiClasses;

namespace AppWeather
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadWeatherData();
        }

        // Affichage des données météo
        private async void LoadWeatherData()
        {
            
            var weatherData = await GetWeather();
            if (weatherData != null)
            {
                // Affichage des données météo actuelles
                TB_Temp.Text = $"{weatherData.current_condition.tmp} °C"; // Affichage de la température
                TB_Humidity.Text = $"Humidité: {weatherData.current_condition.humidity} %";// Affichage de l'humidité
                TB_Pression.Text = $"Préssion: {weatherData.current_condition.pressure} hPa";// Affichage de la pression
                TB_Localisation.Text = $"{weatherData.city_info.name}";// Affichage de la localisation

                // Affichage de l'icône du temps
                string iconUrl = weatherData.current_condition.icon;
                Console.WriteLine($"Icon URL: {iconUrl}"); // Log pour vérifier l'URL
                try
                {
                    WeatherIcon.Source = new BitmapImage(new Uri(iconUrl));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du chargement de l'image: {ex.Message}");
                }
            }

            //Affichage des données météo pour demain
            if (weatherData.fcst_day_0 != null)
            {
                TB_Temp_Day.Text = $"{weatherData.fcst_day_1.tmin} °C / {weatherData.fcst_day_1.tmax} °C"; // Affichage de la température
                //TB_Humidity_Day.Text = $"Humidité: {weatherData.fcst_day_0.rh2} %";// Affichage de l'humidité
                //TB_Pression_Day.Text = $"Préssion: {weatherData.fcst_day_0.msl} hPa";// Affichage de la pression
            }
            else
            {
                // Affichage d'un message d'erreur
                TB_Temp.Text = "Erreur de chargement des données";
                TB_Humidity.Text = "Erreur de chargement des données";
                TB_Pression.Text = "Erreur de chargement des données";
            }
        }

        // Récupération des données météo
        public async Task<WeatherResponse> GetWeather()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://www.prevision-meteo.ch/services/json/Sallanches");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WeatherResponse>(content);
            }
            else
            {
                return null;
            }
        }
    }
}


public class WeatherResponse
{
    public CurrentCondition current_condition { get; set; }
    public FcstDay0 fcst_day_0 { get; set; }
    public FcstDay0 fcst_day_1 { get; set; }
    public CityInfo city_info { get; set; }
}