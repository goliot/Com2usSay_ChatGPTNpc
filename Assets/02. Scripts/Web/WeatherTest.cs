using System;
using UnityEngine;
using UnityEngine.UI;

public enum ECity
{
    Seoul,
    Tokyo,
    Paris,
    Washington,
    Beijing,
    Bankok,
    Hanoi,
    London,
}

public class WeatherTest : MonoBehaviour
{
    [SerializeField] private WeatherController weatherController;
    private Text text;

    private void Start()
    {
        string cityName = GetRandomCityString();
        weatherController.GetWeather(OnWeatherReceived, cityName);
        text = GetComponent<Text>();
    }

    private void OnWeatherReceived(WeatherData data)
    {
        Debug.Log("도시 이름: " + data.name);
        Debug.Log("온도: " + data.main.temp);
        Debug.Log("날씨 설명: " + data.weather[0].description);

        text.text = $"{data.name}\n{data.main.temp}\n{data.weather[0].description}";
    }

    private string GetRandomCityString()
    {
        Array values = Enum.GetValues(typeof(ECity));
        ECity randomCity = (ECity)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        return randomCity.ToString();
    }
}