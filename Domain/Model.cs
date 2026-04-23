using System.Drawing;

namespace Domain
{
    public class Model
    {
        public Point point1 { get; set; }
        public Point point2 { get; set; }
        public string type { get; set; }

        public override string ToString()
        {
            return $"{point1} {point2} {type}";
        }
    }
}
