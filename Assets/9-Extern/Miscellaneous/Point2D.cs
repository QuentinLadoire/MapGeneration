
namespace Miscellaneous
{
    [System.Serializable]
    public struct Point2D
    {
        public float x;
        public float y;

        public Point2D(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public static Point2D zero = new Point2D(0.0f, 0.0f);
    }
}
