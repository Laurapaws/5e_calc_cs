using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

int SEED = 111551;
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

    current_weather_object.name = weather_data.weather[weather_roll - 1].name;
    current_weather_object.effects = weather_data.weather[weather_roll - 1].effects;
    current_weather_object.calming1 = weather_data.weather[weather_roll - 1].calming1;
    current_weather_object.calming2 = weather_data.weather[weather_roll - 1].calming2;
    current_weather_object.worsening1 = weather_data.weather[weather_roll - 1].worsening1;
    current_weather_object.worsening2 = weather_data.weather[weather_roll - 1].worsening2;
    current_weather_object.roll = weather_data.weather[weather_roll - 1].roll;
    current_weather_object.length_dice = weather_data.weather[weather_roll - 1].lengthdice;
    current_weather_object.length_dice_count = weather_data.weather[weather_roll - 1].lengthcount;
    current_weather_object.duration = roll(current_weather_object.length_dice_count, current_weather_object.length_dice);

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

string weather_status()
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
    string next_weather_name = weather.GetType().GetProperty(status).GetValue(weather).ToString();

    Weather new_weather = new Weather();

    foreach (var token in weather_data.weather)
    {
        if (next_weather_name.Equals(token["name"].ToString()))
        {
            new_weather = populate_weather_object(new_weather, token["roll"].ToObject<int>());
            break;
        }
    }

    return new_weather;
}

void print_weather(Weather weather, bool effects, bool clock)
{
    Console.WriteLine($"{weather.name}");
    Console.WriteLine($"Duration: {weather.duration.ToString()} hours");

    if(clock)
    {
        Console.WriteLine($"{weather.start_time}:00 - {weather.end_time}:00");
    }
    Console.WriteLine("------------");

    if (effects)
    {
        Console.WriteLine("Effects: ");
        foreach (var effect in weather.effects)
        {
            Console.WriteLine(effect);
        }
    }
    

    Console.WriteLine();
    Console.WriteLine("===================================");  

}

void weather_sample()
{
    Weather starting_weather = long_rest();
    Weather another_weather = next_weather(starting_weather);
    Weather another_one = next_weather(another_weather);

    print_weather(starting_weather, false, false);
    print_weather(another_weather, false, false);
    print_weather(another_one, false, false);
}

List<Weather> calculate_day(int day_length)
{
    int hour_tracker = 0;
    List<Weather> weather_list = new List<Weather>();

    Weather starting_weather = long_rest();
    weather_list.Add(starting_weather);
    hour_tracker = hour_tracker + starting_weather.duration;

    Weather new_weather = next_weather(starting_weather);
    weather_list.Add(new_weather);
    hour_tracker = hour_tracker + new_weather.duration;

    while (hour_tracker < day_length)
    {
        new_weather = next_weather(new_weather);

        if ((new_weather.duration + hour_tracker) > day_length)
        {
            int remainder_duration = day_length - hour_tracker;
            new_weather.duration = remainder_duration;
        }
        else if (hour_tracker == day_length) {
            break;
        }

        weather_list.Add(new_weather);
        hour_tracker = hour_tracker + new_weather.duration;
    }
    return weather_list;
}

List<Weather> populate_times(List<Weather> weather_list, int current_time, int day_length)
{
    weather_list[0].start_time = current_time;

    if ((current_time + weather_list[0].duration) >= day_length)
    {
        weather_list[0].end_time = (weather_list[0].duration - (day_length - current_time));
        current_time = weather_list[0].end_time;
    }
    else
    {
        weather_list[0].end_time = current_time + weather_list[0].duration;
        current_time = weather_list[0].end_time;
    }

    for (int i = 0; i < weather_list.Count; i++)
    {
        if (i != 0)
        {
            if ((current_time + weather_list[i].duration) >= day_length)
            {
                weather_list[i].start_time = weather_list[i - 1].end_time;
                weather_list[i].end_time = (weather_list[i].duration - (day_length - current_time));
                current_time = weather_list[i].end_time;
            }
            else
            {
                weather_list[i].start_time = weather_list[i - 1].end_time;
                weather_list[i].end_time = current_time + weather_list[i].duration;
                current_time = weather_list[i].end_time;
            }
        }
    }

    return weather_list;
}

List<Weather> todays_weather = calculate_day(24);
todays_weather = populate_times(todays_weather, 9, 24);

foreach(var x in todays_weather) {
    Console.WriteLine("------------");
    print_weather(x, true, true);
    Console.WriteLine("");
}

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