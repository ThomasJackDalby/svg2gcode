using System.Xml.Linq;

public static class XElementExtensions 
{ 
    public static T? GetAttributeValueOrDefault<T>(this XElement xElement, string attributeName, T? defaultValue = default)
    {
        XAttribute? xAttribute = xElement.Attribute(attributeName);
        if (xAttribute is null) return defaultValue;
        return (T)Convert.ChangeType(xAttribute.Value, typeof(T));
    }

    public static T GetAttributeValue<T>(this XElement xElement, string attributeName)
    {
        XAttribute xAttribute = xElement.Attribute(attributeName) ?? throw new Exception();
        return (T)Convert.ChangeType(xAttribute.Value, typeof(T));
    }
}