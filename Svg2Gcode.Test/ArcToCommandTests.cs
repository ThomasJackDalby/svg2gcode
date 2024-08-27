using Dalby.Maths.Geometry;
using Svg2Gcode.Svg.Paths;

namespace Svg2Gcode.Test
{
    public class ArcToCommandTests
    {
        [Theory]
        [InlineData(0, 10, 0)]
        [InlineData(0.5 * Math.PI, 0, 10)]
        [InlineData(Math.PI, -10, 0)]
        public void ArcNew(double theta, double x, double y)
        {
            // Assemble
            ArcToCommand.Arc arc = new(new(0, 0), new(10, 10), 0, 0, Math.PI);

            // Action
            Vector2D result = arc.GetPoint(theta);

            // Assert
            Assert.Equal(x, result.X, 1e-6);
            Assert.Equal(y, result.Y, 1e-6);
        }


        [Theory]
        [InlineData(0, 10, 0)]
        [InlineData(0.5 * Math.PI, 0, 10)]
        [InlineData(Math.PI, -10, 0)]
        public void ArcFrom(double theta, double x, double y)
        {
            // Assemble
            ArcToCommand.Arc arc = ArcToCommand.Arc.From(new(0, 10), new(10, 10), new(5, 5), 0, false, false);
            
            // Action
            Vector2D result = arc.GetPoint(theta);

            // Assert
            Assert.Equal(x, result.X, 1e-6);
            Assert.Equal(y, result.Y, 1e-6);
        }
    }
}