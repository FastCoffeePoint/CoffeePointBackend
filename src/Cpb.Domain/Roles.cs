using System.Text.Json.Serialization;

namespace Cpb.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Roles
{
    Customer = 0,
    Admin = 1,
}