﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using Newtonsoft.Json;
using ApiClasses;
using System.Windows.Media.Imaging;
using System.IO;

namespace AppWeather
{
    public partial class MainWindow : Window
    {
        // Liste pour stocker les villes déjà utilisées
        private List<string> cityHistory = new List<string>();

        // Chemin du fichier pour stocker les villes
        private string cityHistoryFilePath = "cityHistory.txt";

        public MainWindow()
        {
            InitializeComponent();
            LoadCityHistory(); // Charge les villes enregistrées au démarrage de l'application
            _ = LoadWeatherData("Sallanches"); // Charge les données météo pour une ville par défaut
        }

        // Formate le jour et la date en une chaîne lisible
        private string FormatDayAndDate(string dayLong, string date)
        {
            DateTime parsedDate;
            if (DateTime.TryParse(date, out parsedDate))
            {
                return $"{dayLong.ToUpper()} {parsedDate.Day}";
            }
            return $"{dayLong.ToUpper()} {date}";
        }

        // Gestionnaire de clic pour le bouton de recherche de ville
        private async void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            string cityName = CityInput.Text.Trim();
            if (!string.IsNullOrEmpty(cityName))
            {
                // Charge les données météo pour la ville saisie par l'utilisateur
                await LoadWeatherData(cityName);

                // Sauvegarde la ville dans l'historique si elle n'existe pas déjà
                SaveCityIfNew(cityName);

                // Ajoute la ville à l'historique si elle n'y est pas déjà
                if (!cityHistory.Contains(cityName))
                {
                    cityHistory.Add(cityName);
                    CityHistoryComboBox.Items.Add(cityName); // Ajoute la ville dans la ComboBox
                }
            }
            else
            {
                // Affiche un message si le champ de texte est vide
                MessageBox.Show("Veuillez entrer un nom de ville.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Charge l'historique des villes enregistrées à partir du fichier
        private void LoadCityHistory()
        {
            if (File.Exists(cityHistoryFilePath))
            {
                var savedCities = File.ReadAllLines(cityHistoryFilePath);
                cityHistory.AddRange(savedCities.Distinct()); // Évite les doublons
                foreach (var city in cityHistory)
                {
                    CityHistoryComboBox.Items.Add(city);
                }
            }
        }

        // Sauvegarde la ville dans le fichier si elle est nouvelle
        private void SaveCityIfNew(string cityName)
        {
            if (!cityHistory.Contains(cityName))
            {
                cityHistory.Add(cityName);
                CityHistoryComboBox.Items.Add(cityName);
                File.AppendAllText(cityHistoryFilePath, cityName + Environment.NewLine);
            }
        }

        // Gestionnaire de sélection de ville dans la ComboBox d'historique
        private void OnCityHistorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CityHistoryComboBox.SelectedItem != null)
            {
                // Récupère la valeur de la ville sélectionnée
                string selectedCity = CityHistoryComboBox.SelectedItem.ToString();
                MessageBox.Show($"Ville sélectionnée : {selectedCity}");

                // Charge les données météo pour la ville sélectionnée
                _ = LoadWeatherData(selectedCity);
            }
        }

        // Vérifie si une ville existe à l'aide de l'API
        private async Task<bool> CheckCityExists(string cityName)
        {
            var weatherData = await GetWeather(cityName);
            if (weatherData != null && weatherData.city_info != null)
            {
                return true;
            }
            else
            {
                MessageBox.Show("La ville saisie n'existe pas ou les données ne sont pas disponibles.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        // Charge les données météo pour une ville donnée
        private async Task LoadWeatherData(string cityName)
        {
            bool cityExists = await CheckCityExists(cityName);
            if (!cityExists) return;

            var weatherData = await GetWeather(cityName);
            if (weatherData == null || weatherData.current_condition == null)
            {
                MessageBox.Show("Erreur : Aucune donnée météo trouvée pour cette ville.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Mise à jour de l'interface utilisateur avec les données météo
            TB_Temp.Text = $"{weatherData.current_condition.tmp} °C";
            TB_Humidity.Text = $"Humidité: {weatherData.current_condition.humidity} %";
            TB_Pression.Text = $"Pression: {weatherData.current_condition.pressure} hPa";
            TB_Localisation.Text = $"{weatherData.city_info.name}";

            // Mise à jour de l'icône de la météo actuelle
            string iconUrl = weatherData.current_condition.icon_big;
            try
            {
                WeatherIcon.Source = new BitmapImage(new Uri(iconUrl));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'image: {ex.Message}");
            }

            // Mise à jour des prévisions pour les jours suivants
            TB_Day_Tomorrow.Text = FormatDayAndDate(weatherData.fcst_day_1.day_long, weatherData.fcst_day_1.date);
            TB_Temp_Day.Text = $"{weatherData.fcst_day_1.tmin} °C / {weatherData.fcst_day_1.tmax} °C";
            string iconWeatherNextDay = weatherData.fcst_day_1.icon;
            Next_Day_Icon.Source = new BitmapImage(new Uri(iconWeatherNextDay));
            TB_Weather_NextDay.Text = weatherData.fcst_day_1.condition;

            // Prévisions pour les jours suivants (jusqu'à 4 jours)
            TB_Day_AfterTomorrow.Text = FormatDayAndDate(weatherData.fcst_day_2.day_long, weatherData.fcst_day_2.date);
            TB_Temp_AfterDay.Text = $"{weatherData.fcst_day_2.tmin} °C / {weatherData.fcst_day_2.tmax} °C";
            string iconWeatherAfterNextDay = weatherData.fcst_day_2.icon;
            AfterNext_Day_Icon.Source = new BitmapImage(new Uri(iconWeatherAfterNextDay));
            TB_Weather_AfterNextDay.Text = weatherData.fcst_day_2.condition;

            TB_Day_3Days.Text = FormatDayAndDate(weatherData.fcst_day_3.day_long, weatherData.fcst_day_3.date);
            TB_Temp_3Days.Text = $"{weatherData.fcst_day_3.tmin} °C / {weatherData.fcst_day_3.tmax} °C";
            string iconWeather3Days = weatherData.fcst_day_3.icon;
            Day_3_Icon.Source = new BitmapImage(new Uri(iconWeather3Days));
            TB_Weather_3Days.Text = weatherData.fcst_day_3.condition;

            TB_Day_4Days.Text = FormatDayAndDate(weatherData.fcst_day_4.day_long, weatherData.fcst_day_4.date);
            TB_Temp_4Days.Text = $"{weatherData.fcst_day_4.tmin} °C / {weatherData.fcst_day_4.tmax} °C";
            string iconWeather4Days = weatherData.fcst_day_4.icon;
            Day_4_Icon.Source = new BitmapImage(new Uri(iconWeather4Days));
            TB_Weather_4Days.Text = weatherData.fcst_day_4.condition;
        }

        // Récupère les données météo pour une ville donnée
        public async Task<WeatherResponse> GetWeather(string cityName)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync($"https://www.prevision-meteo.ch/services/json/{cityName}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(content); // Affiche le contenu de la réponse pour le débogage
                        if (string.IsNullOrWhiteSpace(content) || content.Contains("\"errors\""))
                        {
                            MessageBox.Show($"La ville '{cityName}' n'existe pas ou les données sont indisponibles.", "Erreur de données", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return null;
                        }
                        return JsonConvert.DeserializeObject<WeatherResponse>(content);
                    }
                    else
                    {
                        MessageBox.Show("Erreur de récupération des données météo. Vérifiez le nom de la ville ou réessayez plus tard.",
                            "Erreur de connexion", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur est survenue lors de la connexion à l'API : {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
        }
    }
}

// Classes pour désérialiser la réponse de l'API météo
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