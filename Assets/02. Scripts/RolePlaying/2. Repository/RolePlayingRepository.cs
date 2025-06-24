using OpenAI;
using System.Collections.Generic;
using OpenAI.Chat;
using OpenAI.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI.Batch;
using OpenAI.Audio;
using OpenAI.Images;

public class RolePlayingRepository
{
    public string OPENAI_API_KEY = ApiKeys.OPENAI_API_KEY;

    private OpenAIClient _api;
    private List<Message> _messages = new List<Message>();

    public RolePlayingRepository()
    {
        _api = new OpenAIClient(OPENAI_API_KEY);
    }

    public async void SendMessage(Message message)
    {

    }
}