using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HandJob.WebApi.Filters;

public class SwaggerEnumFilter : IDocumentFilter
{
    /// <summary>
    /// 实现IDocumentFilter接口的Apply函数
    /// </summary>
    /// <param name="swaggerDoc"> </param>
    /// <param name="context">    </param>
    public void Apply(Microsoft.OpenApi.Models.OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var dict = GetAllEnum();

        foreach (var item in swaggerDoc.Components.Schemas)
        {
            var property = item.Value;
            var typeName = item.Key;
            if (property.Enum is { Count: > 0 })
            {
                Type? itemType = null;
                itemType = dict.TryGetValue(typeName, out var value) ? value : null;
                var list = new List<OpenApiInteger>();
                foreach (var val in property.Enum)
                {
                    list.Add((OpenApiInteger)val);
                }
                property.Description += DescribeEnum(itemType!, list);
            }
        }
    }

    private static Dictionary<string, Type> GetAllEnum()
    {
        Assembly ass = Assembly.Load("HandJob.Domain");
        Type[] types = ass.GetTypes();
        Dictionary<string, Type> dict = new Dictionary<string, Type>();

        foreach (Type item in types)
        {
            if (item.IsEnum)
            {
                dict.Add(item.Name, item);
            }
        }
        return dict;
    }

    private static string DescribeEnum(Type type, List<OpenApiInteger> enums)
    {
        var enumDescriptions = new List<string>();
        foreach (var item in enums)
        {
            if (type == null) continue;
            var value = Enum.Parse(type, item.Value.ToString());
            var desc = GetDescription(type, value);

            enumDescriptions.Add(string.IsNullOrEmpty(desc)
                ? $"{item.Value.ToString()}：{Enum.GetName(type, value)}；"
                : $"{item.Value.ToString()}：{Enum.GetName(type, value)}，{desc}；");
        }
        return $"<br><div>{Environment.NewLine}{string.Join("<br/>" + Environment.NewLine, enumDescriptions)}</div>";
    }

    private static string GetDescription(Type t, object value)
    {
        foreach (MemberInfo mInfo in t.GetMembers())
        {
            if (mInfo.Name == t.GetEnumName(value))
            {
                foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                {
                    if (attr.GetType() == typeof(DescriptionAttribute))
                    {
                        return ((DescriptionAttribute)attr).Description;
                    }
                }
            }
        }
        return string.Empty;
    }
}