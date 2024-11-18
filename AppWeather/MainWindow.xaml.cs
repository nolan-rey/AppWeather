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
            _ = LoadWeatherData("Annecy"); // Ville par défaut
        }

        // Méthode pour formater et afficher la date et le jour en majuscules
        private string FormatDayAndDate(string dayLong, string date)
        {
            DateTime parsedDate;
            if (DateTime.TryParse(date, out parsedDate))
            {
                // Exemple pour "MARDI 19" avec date formatée
                return $"{dayLong.ToUpper()} {parsedDate.Day}";
            }
            return $"{dayLong.ToUpper()} {date}"; // Fallback si la date ne peut pas être parsée
        }

        private async void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            string cityName = CityInput.Text.Trim();
            if (!string.IsNullOrEmpty(cityName))
            {
                await LoadWeatherData(cityName);
            }
            else
            {
                MessageBox.Show("Veuillez entrer un nom de ville.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        // Affichage des données météo
        private async Task LoadWeatherData(string cityName)
        {
            
            var weatherData = await GetWeather(cityName); ;
            if (weatherData == null)
            {
                MessageBox.Show("Erreur : Aucune donnée météo trouvée pour cette ville.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Arrête l'exécution si aucune donnée n'est disponible
            }
            if (weatherData != null && weatherData.current_condition != null)
            {
                TB_Temp.Text = $"{weatherData.current_condition.tmp} °C";
                TB_Humidity.Text = $"Humidité: {weatherData.current_condition.humidity} %";
                TB_Pression.Text = $"Pression: {weatherData.current_condition.pressure} hPa";
                TB_Localisation.Text = $"{weatherData.city_info.name}";

                string iconUrl = weatherData.current_condition.icon_big;
                try
                {
                    WeatherIcon.Source = new BitmapImage(new Uri(iconUrl));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du chargement de l'image: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Erreur : Les données météo actuelles sont manquantes.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            //Affichage des données météo pour demain (fcst_day_1)
            TB_Day_Tomorrow.Text = FormatDayAndDate(weatherData.fcst_day_1.day_long, weatherData.fcst_day_1.date);
            TB_Temp_Day.Text = $"{weatherData.fcst_day_1.tmin} °C / {weatherData.fcst_day_1.tmax} °C";
            string iconWeatherNextDay = weatherData.fcst_day_1.icon;
            Next_Day_Icon.Source = new BitmapImage(new Uri(iconWeatherNextDay));
            TB_Weather_NextDay.Text = weatherData.fcst_day_1.condition;

            //Affichage des données météo pour après-demain (fcst_day_2)
            TB_Day_AfterTomorrow.Text = FormatDayAndDate(weatherData.fcst_day_2.day_long, weatherData.fcst_day_2.date);
            TB_Temp_AfterDay.Text = $"{weatherData.fcst_day_2.tmin} °C / {weatherData.fcst_day_2.tmax} °C";
            string iconWeatherAfterNextDay = weatherData.fcst_day_2.icon;
            AfterNext_Day_Icon.Source = new BitmapImage(new Uri(iconWeatherAfterNextDay));
            TB_Weather_AfterNextDay.Text = weatherData.fcst_day_2.condition;

            //Affichage des données météo pour dans 3 jours (fcst_day_3)
            TB_Day_3Days.Text = FormatDayAndDate(weatherData.fcst_day_3.day_long, weatherData.fcst_day_3.date);
            TB_Temp_3Days.Text = $"{weatherData.fcst_day_3.tmin} °C / {weatherData.fcst_day_3.tmax} °C";
            string iconWeather3Days = weatherData.fcst_day_3.icon;
            Day_3_Icon.Source = new BitmapImage(new Uri(iconWeather3Days));
            TB_Weather_3Days.Text = weatherData.fcst_day_3.condition;

            //Affichage des données météo pour dans 4 jours (fcst_day_4)
            TB_Day_4Days.Text = FormatDayAndDate(weatherData.fcst_day_4.day_long, weatherData.fcst_day_4.date);
            TB_Temp_4Days.Text = $"{weatherData.fcst_day_4.tmin} °C / {weatherData.fcst_day_4.tmax} °C";
            string iconWeather4Days = weatherData.fcst_day_4.icon;
            Day_4_Icon.Source = new BitmapImage(new Uri(iconWeather4Days));
            TB_Weather_4Days.Text = weatherData.fcst_day_4.condition;
        }

        // Récupération des données météo
        public async Task<WeatherResponse> GetWeather(string cityName)
        {
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync($"https://www.prevision-meteo.ch/services/json/{cityName}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    try
                    {
                        // Désérialisation protégée
                        return JsonConvert.DeserializeObject<WeatherResponse>(content);
                    }
                    catch (JsonSerializationException ex)
                    {
                        // Affichage d'un message d'erreur si la désérialisation échoue
                        MessageBox.Show($"Erreur lors de la récupération des données pour la ville '{cityName}'. Veuillez vérifier votre saisie ou réessayer plus tard.",
                            "Erreur de données", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Console.WriteLine($"Détails de l'erreur : {ex.Message}");
                        return null;
                    }
                }
                else
                {
                    // Gestion des erreurs de réponse API
                    MessageBox.Show("Erreur de récupération des données météo. Vérifiez le nom de la ville ou réessayez plus tard.",
                        "Erreur de connexion", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Gestion des erreurs générales (par exemple, problèmes de connexion)
                MessageBox.Show($"Une erreur est survenue lors de la connexion à l'API : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}


public class WeatherResponse
{
    public CurrentCondition current_condition { get; set; }
    public FcstDay0 fcst_day_0 { get; set; }

    public FcstDay1 fcst_day_1 { get; set; }
    public FcstDay2 fcst_day_2 { get; set; }
    public FcstDay3 fcst_day_3 { get; set; }
    public FcstDay4 fcst_day_4 { get; set; }
    public CityInfo city_info { get; set; }
}