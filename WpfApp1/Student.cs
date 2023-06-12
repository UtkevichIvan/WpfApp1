using System;
using System.Text.Json.Serialization;

namespace WpfApp1;

public class Student
{
    [JsonIgnore]
    public int Id { get; set; }
    [JsonIgnore]
    public string CardId { get; set; } = "";
    [JsonIgnore]
    public int Age { get; set; }
    public string Name { get; set; }
    public string SecondName { get; set; }
    public string Patronomic { get; set; }
    public int Course { get; set; }
    public int Group { get; set; }
    [JsonIgnore]
    public string Image { get; set; }
    [JsonIgnore]
    public string ImageCamera { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is Student person) return CardId == person.CardId;
        return false;
    }

    public override int GetHashCode()
    {
        int res = 0;
        for (int i = 0; i < CardId.Length; i++)
        {
            res += CardId[i] * (i + 1);
        }

        return res;
    }
}
