using System.Drawing;

namespace Domain
{
    public class Model
    {
        public PointF point1 { get; set; }
        public PointF point2 { get; set; }
        public string type { get; set; }
        public bool isPreview { get; set; }

        public override string ToString()
        {
            return $"{point1} {point2} {type}";
        }
    }
}
