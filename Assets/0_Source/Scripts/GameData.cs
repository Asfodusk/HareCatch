using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class GameData
{
    public int wallet;

    public int karma;

    public float currentTime;

    // Сохранённые выборы игрока: ID одобренных (оставленных) и выгнанных НПС.
    public List<string> approvedNpcs = new List<string>();
    public List<string> kickedNpcs = new List<string>();

    // Конструктор по умолчанию — нужен для создания пустых данных (новая игра)
    // и для корректной работы инспектора Unity.
    public GameData() { }

    // Конструктор: заполняет поля объекта из JSON-строки
    public GameData(string json)
    {
        JsonConvert.PopulateObject(json, this);
    }

    // Сериализует сам себя в JSON-строку
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
