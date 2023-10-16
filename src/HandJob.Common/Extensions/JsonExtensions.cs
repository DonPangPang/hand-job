using Newtonsoft.Json;

namespace HandJob.Common;

public static class JsonExtensions
{
    public static string ToJson(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static object? ToObject<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json) ?? default;
    }
}