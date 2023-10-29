using System.Xml.Linq;
using Svg2Gcode.Svg;

public class SvgDocument
{
    public List<Element> Elements { get; } = new();

    public IEnumerable<Shape> GetShapes()
    {
        foreach (Element element in Elements)
        {
            if (element is Shape shape) yield return shape;
            //else if (element is 
        }
    }

    public void Save(string filePath)
    {
        throw new NotImplementedException();

        //PathShapeFormatter formatter = new();
        //string data = formatter.Format(svgDocument.Elements.OfType<PathShape>().First());
        //File.WriteAllText("data.txt", data);
    }

    public static SvgDocument? Load(string filePath)
    {
        using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        XDocument? document = XDocument.Load(stream);
        return document is not null ? Load(document) : null;
    }
    public static SvgDocument? Load(XDocument xDocument)
    {
        SvgDocument svgDocument = new();
        if (xDocument.Root is null) return null;
        foreach (XElement xElement in xDocument.Root.Elements())
        {
            Element? element = null;
            if (xElement.Name.LocalName == "path") element = PathShape.From(xElement);
            else if (xElement.Name.LocalName == "line") element = LineShape.From(xElement);

            if (element is not null) svgDocument.Elements.Add(element);
        }
        return svgDocument;
    }
}