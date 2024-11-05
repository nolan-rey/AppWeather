﻿using System;
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

namespace AppWeather
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); // Initialisation des composants de l'interface utilisateur
            LoadWeatherData(); // Chargement des données météo lors de l'ouverture de la fenêtre
        }

        // Méthode asynchrone pour charger et afficher les données météo
        private async void LoadWeatherData()
        {
            var weatherData = await GetWeather(); // Appel de l'API pour obtenir les données météo
            if (weatherData != null)
            {
                // Mise à jour de l'interface utilisateur avec les données météo
                TB_Temp.Text = $"Température: {weatherData.Main.temp} °C";
                TB_Humidity.Text = $"Humidité: {weatherData.Main.humidity} %";
                TB_Pression.Text = $"Préssion: {weatherData.Main.pressure} hPa";
                TB_TempHight.Text = $"Temp Max: {weatherData.Main.temp_max} °C";
                TB_TempLow.Text = $"Temp Min: {weatherData.Main.temp_min} °C";
                TB_Recentie.Text = $"Ressentie: {weatherData.Main.feels_like} °C";
            }
            else
            {
                // Affiche un message d'erreur si les données n'ont pas pu être chargées
                TB_Temp.Text = "Erreur de chargement des données";
                TB_Humidity.Text = "Erreur de chargement des données";
                TB_Pression.Text = "Erreur de chargement des données";
                TB_TempHight.Text = "Erreur de chargement des données";
                TB_TempLow.Text = "Erreur de chargement des données";
                TB_Recentie.Text = "Erreur de chargement des données";
            }
        }

        // Méthode pour obtenir les données météo de l'API
        public async Task<WeatherResponse> GetWeather()
        {
            HttpClient client = new HttpClient(); // Création d'un client HTTP
                                                  // Envoi de la requête GET à l'API météo
            HttpResponseMessage response = await client.GetAsync("https://api.openweathermap.org/data/2.5/weather?q=annecy,fr&appid=c21a75b667d6f7abb81f118dcf8d4611&units=metric");

            if (response.IsSuccessStatusCode) // Vérifie si la requête a réussi
            {
                // Lit le contenu JSON de la réponse et le désérialise en un objet WeatherResponse
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WeatherResponse>(content);
            }
            else
            {
                // Retourne null en cas d'erreur dans la requête
                return null;
            }
        }
    }

    // Classes de modèle pour représenter la réponse de l'API météo
    public class Clouds
    {
        public int all { get; set; } // Pourcentage de couverture nuageuse
    }

    public class Coord
    {
        public double lon { get; set; } 
        public double lat { get; set; } 
    }

    public class WeatherResponse
    {
        public Main Main { get; set; } 
    }

    public class Main
    {
        public double temp { get; set; } 
        public double feels_like { get; set; } 
        public double temp_min { get; set; } 
        public double temp_max { get; set; } 
        public int pressure { get; set; } 
        public int humidity { get; set; } 
        public int sea_level { get; set; } 
        public int grnd_level { get; set; } 
    }

    public class Root
    {
        public Coord coord { get; set; }
        public List<Weather> weather { get; set; } 
        public string @base { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; } 
        public Wind wind { get; set; } 
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; } 
        public int timezone { get; set; } 
        public int id { get; set; }
        public string name { get; set; } 
        public int cod { get; set; } 
    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public string country { get; set; } 
        public int sunrise { get; set; } 
        public int sunset { get; set; } 
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; } 
        public string description { get; set; } 
        public string icon { get; set; } 
    }

    public class Wind
    {
        public double speed { get; set; } 
        public int deg { get; set; } 
    }