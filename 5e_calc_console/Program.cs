using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

int SEED = 12263149;
string WEATHER_DATA = File.ReadAllText("weather.json");
Random random = new Random(SEED);
dynamic weather_data = JsonConvert.DeserializeObject(WEATHER_DATA);

int roll(int dice_count, int max_dice_value)
{
    int total = 0;

    for (int i = 1; i<= dice_count; i++)
    {
        total = total + random.Next(1, max_dice_value+1);
    }

    return total;
}

Weather populate_weather_object(Weather current_weather_object, int weather_roll)
{
    string weather_roll_string = weather_roll.ToString();

    current_weather_object.name = weather_data.weather[weather_roll_string].name;
    current_weather_object.effects = weather_data.weather[weather_roll_string].effects;
    current_weather_object.calming1 = weather_data.weather[weather_roll_string].calming1;
    current_weather_object.calming2 = weather_data.weather[weather_roll_string].calming2;
    current_weather_object.worsening1 = weather_data.weather[weather_roll_string].worsening1;
    current_weather_object.worsening2 = weather_data.weather[weather_roll_string].worsening2;
    current_weather_object.roll = weather_data.weather[weather_roll_string].roll;
    current_weather_object.length_dice = weather_data.weather[weather_roll_string].lengthdice;
    current_weather_object.length_dice_count = weather_data.weather[weather_roll_string].lengthcount;

    return current_weather_object;
}

Weather long_rest()
{
    int weather_roll = roll(1, 12);
    Weather current_weather = new Weather
    {
        name = "",
        calming1 = "",
        calming2 = "",
        worsening1 = "",
        worsening2 = "",
        duration = 1,
        start_time = 1,
        end_time = 1,
        roll = weather_roll,
        length_dice = 0,
        length_dice_count = 0,
    };
    current_weather = populate_weather_object(current_weather, weather_roll);

    return current_weather;
}

String weather_status()
{
    string weather_status = "Error weather not calculated";
    int roll_result = roll(1, 100);

    if (roll_result <= 25)
    {
        weather_status = "calming1";
    }
    else if (roll_result <= 50)
    {
        weather_status = "calming2";
    }
    else if (roll_result <= 75)
    {
        weather_status = "worsening1";
    }
    else
    {
        weather_status = "worsening2";
    }

    return weather_status;

}

Weather next_weather(Weather weather)
{
    string status = weather_status();
    object temp1 = weather.GetType().GetProperty(status).GetValue(weather);
    string next_weather_name = temp1.ToString();

    string json_string = weather_data.weather.ToString();
    var jsonData = JObject.Parse(json_string).Children();
    List<JToken> tokens = jsonData.Children().ToList();

    Weather new_weather = new Weather();

    foreach (JToken token in tokens)
    {
        if (next_weather_name.Equals(token["name"].ToString()))
        {
            new_weather = populate_weather_object(new_weather, token["roll"].ToObject<int>());
            break;
        }
    }

    return new_weather;
}

Weather starting_weather = long_rest();

Weather another_weather = next_weather(starting_weather);

Console.ReadLine();

public class Weather
{
    public string name { get; set; }
    public Newtonsoft.Json.Linq.JArray? effects { get; set; }
    public string calming1 { get; set; }
    public string calming2 { get; set; }
    public string worsening1 { get; set; }
    public string worsening2 { get; set; }
    public int duration { get; set; }
    public int start_time { get; set; }
    public int end_time { get; set; }
    public int roll { get; set; }
    public int length_dice { get; set; }
    public int length_dice_count { get; set; }

    public Weather()
    {
        name = "";
        calming1 = "";
        calming2 = "";
        worsening1 = "";
        worsening2 = "";
        duration = 1;
        start_time = 1;
        end_time = 1;
        roll = 1;
        length_dice = 0;
        length_dice_count = 0;
    }

}