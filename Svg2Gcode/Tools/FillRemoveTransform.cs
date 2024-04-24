using Dalby.Maths.Geometry;
using Svg2Gcode.Svg;

namespace Svg2Gcode.Tools
{
    internal class FillRemoveTransform : ISvgDocumentTransform
    {
        // converts shapes masked by other shapes into simple lines

        public SvgDocument Transform(SvgDocument input)
        {
            ISpatialTree maskTree = new TriangularTree();
            SvgDocument output = new();
            foreach (Shape shape in input.GetShapes()) // order these by depth? do top first?
            {
                // if the element does not intersect the tree, we can add it normally
                if (!maskTree.Intersects(shape)) output.Elements.Add(shape);

                // otherwise, need to break the shape down into a series of smaller paths
                foreach (Path2D path in shape.GetPaths()) // 
                {
                    // want to add this path to the current drawing. Can only do that if no masks cover it
                    // as we're adding top down, if we're ok based on whats already drawn, it won't be covered by anything else.
                    
                    
                    
                    //foreach (Path2D p in maskTree.Intersect(path))
                    //{
                    //    output.Elements.Add(p);
                    //}
                }
                maskTree.AddShape(shape);
            }
            throw new NotImplementedException();
        }
    }

    public class TriangularTree : ISpatialTree
    {
        public void AddShape(Shape shape)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Path2D> Intersect(Path2D path)
        {
            throw new NotImplementedException();
        }

        public bool Intersects(Shape shape)
        {
            throw new NotImplementedException();
        }
    }

    public interface ISpatialTree
    {
        void AddShape(Shape shape);
        IEnumerable<Path2D> Intersect(Path2D path);

        // Does the shape intersect with anything in the tree
        bool Intersects(Shape shape);
    }

    public interface ISvgDocumentTransform
    {
        SvgDocument Transform(SvgDocument input);
    }
}
