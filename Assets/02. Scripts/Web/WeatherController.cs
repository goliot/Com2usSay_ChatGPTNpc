using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

using System;

[Serializable]
public class WeatherData
{
    public Coord coord;
    public Weather[] weather;
    public string station;
    public Main main;
    public int visibility;
    public Wind wind;
    public Clouds clouds;
    public int dt;
    public Sys sys;
    public int id;
    public string name;
    public int cod;
}

[Serializable]
public class Coord
{
    public float lon;
    public float lat;
}

[Serializable]
public class Weather
{
    public int id;
    public string main;
    public string description;
    public string icon;
}

[Serializable]
public class Wind
{
    public float speed;
    public float deg;
}

[Serializable]
public class Main
{
    public float temp;
    public int pressure;
    public int humidity;
    public float temp_min;
    public float temp_max;
}

[Serializable]
public class Clouds
{
    public int all;
}

[Serializable]
public class Sys
{
    public int type;
    public int id;
    public float message;
    public string country;
    public int sunrise;
    public int sunset;
}

public class WeatherController : MonoBehaviour
{
    public string CityName = "Seoul";
    //API 주소
    //===============================================
    public string API_ADDRESS = $"api.openweathermap.org/data/2.5/weather?q=Seoul&appid={ApiKeys.OPENWEATHER_API_KEY}";
    //===============================================

    //날씨 데이터가 다운로드되면 CallBack으로 필요한 함수로 돌아간다
    public delegate void WeatherDataCallback(WeatherData weatherData);

    //다운로드된 날씨 데이터. 중복 다운로드를 막기위하여 저장해둔다
    private WeatherData _weatherData;

    /// <summary>
    /// API로부터 날씨 데이터를 받아온다
    /// </summary>
    public void GetWeather(WeatherDataCallback callback, string cityName)
    {
        //현재의 날씨 데이터가 없다면 API로부터 받아온다
        //if (_weatherData == null)
        //{
        //    API_ADDRESS = $"api.openweathermap.org/data/2.5/weather?q={cityName}&appid={ApiKeys.OPENWEATHER_API_KEY}";
        //    StartCoroutine(CoGetWeather(callback));
        //}
        //else
        //{
        //    //현재의 날씨 데이터가 존재한다면 그 날씨데이터를 그대로 사용한다
        //    callback(_weatherData);
        //}
        API_ADDRESS = $"api.openweathermap.org/data/2.5/weather?q={cityName}&appid={ApiKeys.OPENWEATHER_API_KEY}";
        StartCoroutine(CoGetWeather(callback));
    }

    /// <summary>
    /// 날씨 API로부터 정보를 받아온다
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator CoGetWeather(WeatherDataCallback callback)
    {
        Debug.Log("날씨 정보를 다운로드합니다");

        var webRequest = UnityWebRequest.Get(API_ADDRESS);
        yield return webRequest.SendWebRequest();

        //만약 에러가 있을 경우
        if (webRequest.isHttpError || webRequest.isNetworkError)
        {
            Debug.Log(webRequest.error);
            yield break;
        }

        //다운로드 완료
        var downloadedTxt = webRequest.downloadHandler.text;

        Debug.Log("날씨 정보가 다운로드 되었습니다! : " + downloadedTxt);

        //유니티 언어와 겹치므로 base를 사용할 수 없기때문에 Replace가 필요하다
        string weatherStr = downloadedTxt.Replace("base", "station");

        _weatherData = JsonUtility.FromJson<WeatherData>(weatherStr);
        callback(_weatherData);
    }
}