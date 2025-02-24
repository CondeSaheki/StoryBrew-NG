using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

namespace StoryBrew.Runtime;

public partial class Bootstrap
{
    private string getSchema(bool indented = true)
    {
        using StringWriter stringWriter = new();
        using JsonTextWriter jsonWriter = new(stringWriter) { Formatting = indented ? Formatting.Indented : Formatting.None };

        JSchemaGenerator generator = new();

        jsonWriter.WriteStartObject();
        foreach (var script in getScripts())
        {
            var name = script.FullName ?? throw new Exception("Failed to get script name");
            var schema = generator.Generate(script);

            jsonWriter.WritePropertyName(name);
            schema.WriteTo(jsonWriter);
        }
        jsonWriter.WriteEndObject();

        return stringWriter.ToString();
    }
}