using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TypecastTTS
{
    [Header("API 세팅")]
    public string apiToken = ApiKeys.TYPECAST_API_TOKEN;      // ✅ 실 API 토큰
    public string actorId = "65c47f4f7e237f1cb0a80380";    // 세나

    [Serializable]
    public class TypecastResult
    {
        public Result result;

        [Serializable]
        public class Result
        {
            public string speak_url;
            public string speak_v2_url;
            public string play_id;
        }
    }


    // 외부에서 호출할 유일한 함수
    public async Task<AudioClip> GetSpeechClipAsync(string text)
    {
        string audioUrl = await RequestTypecastUrlAsync(text);
        if (!string.IsNullOrEmpty(audioUrl))
        {
            return await DownloadAudioClipAsync(audioUrl);
        }

        return null;
    }

    // 내부 함수: Typecast에 TTS 요청 → mp3 URL 응답
    private async Task<string> RequestTypecastUrlAsync(string text)
    {
        string jsonData = $@"
        {{
            ""text"": ""{text}"",
            ""actor_id"": ""{actorId}"",
            ""lang"": ""auto"",
            ""model_version"": ""latest"",
            ""xapi_hd"": false
        }}";

        using UnityWebRequest request = new UnityWebRequest("https://typecast.ai/api/speak", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiToken);

        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var responseJson = request.downloadHandler.text;
            var response = JsonUtility.FromJson<TypecastResult>(responseJson);
            if (response != null && response.result != null)
            {
                return response.result.speak_v2_url;
            }
            return null;
        }
        else
        {
            Debug.LogError($"TTS 요청 실패: {request.responseCode} - {request.error}");
            Debug.LogError("서버 응답: " + request.downloadHandler.text);
            return null;
        }
    }


    // 내부 함수: mp3 URL을 AudioClip으로 다운로드
    private async Task<AudioClip> DownloadAudioClipAsync(string url)
    {
        string status = "";
        string audioUrl = null;
        string jsonText = null;

        // 1) 최대 대기 시간 및 재시도 횟수 설정
        int maxRetries = 10;
        int retryDelayMs = 1000; // 1초 간격

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using (UnityWebRequest jsonRequest = UnityWebRequest.Get(url))
            {
                jsonRequest.SetRequestHeader("Authorization", "Bearer " + apiToken);
                var jsonOp = jsonRequest.SendWebRequest();

                while (!jsonOp.isDone)
                    await Task.Yield();

                if (jsonRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("v2 JSON 요청 실패: " + jsonRequest.error);
                    return null;
                }

                jsonText = jsonRequest.downloadHandler.text;
                status = ExtractDone(jsonText);

                if (status == "done")
                {
                    audioUrl = ExtractAudioUrl(jsonText);
                    break;
                }
            }

            // 아직 완료되지 않았다면 대기 후 재요청
            await Task.Delay(retryDelayMs);
        }

        if (status != "done" || string.IsNullOrEmpty(audioUrl))
        {
            Debug.LogError("음성 생성이 완료되지 않았습니다.");
            return null;
        }

        // 2) 완료된 audio_download_url로 오디오 다운로드
        using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.WAV))
        {
            var audioOp = audioRequest.SendWebRequest();

            while (!audioOp.isDone)
                await Task.Yield();

            if (audioRequest.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerAudioClip.GetContent(audioRequest);
            }
            else
            {
                Debug.LogError("오디오 다운로드 실패: " + audioRequest.error);
                return null;
            }
        }
    }


    public string ExtractDone(string json)
    {
        var jObject = JObject.Parse(json);
        return jObject["result"]?["status"]?.ToString();
    }

    public string ExtractAudioUrl(string json)
    {
        var jObject = JObject.Parse(json);
        return jObject["result"]?["audio_download_url"]?.ToString();
    }
}