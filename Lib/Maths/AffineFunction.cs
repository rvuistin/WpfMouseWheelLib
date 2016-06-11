namespace Lada.Maths
{
    public class AffineFunction
    {
        public                  AffineFunction()
        {
        }
        public                  AffineFunction(double slope, double yIntercept)
        {
            this.Slope      = slope;
            this.YIntercept = yIntercept;
        }
        public                  AffineFunction(double ax, double ay, double slope)
        {
            this.Slope      = slope;
            this.YIntercept = ay - slope * ax;
        }
        public                  AffineFunction(double ax, double ay, double bx, double by)
        {
            this.Slope      = (by - ay) / (bx - ax);
            this.YIntercept = ay - Slope * ax;
        }

        public double           Slope
        {
            get;
        }
        public double           YIntercept
        {
            get;
        }
        public double           XIntercept
        {
            get
            {
                return X (0);
            }
        }
        public double           Y(double x)
        {
            return this.Slope * x + this.YIntercept;
        }
        public double           X(double y)
        {
            return DoubleEx.IsZero (this.Slope) ? double.NaN : (y - this.YIntercept) / this.Slope;
        }
        public AffineFunction   SetYIntercept(double value)
        {
            if (DoubleEx.AreClose (this.YIntercept, value))
            {
                return this;
            }
            return new AffineFunction (this.Slope, value);
        }
        public AffineFunction   SetXIntercept(double value) => this.SetYIntercept (-this.Slope * value);
        public AffineFunction   TranslateTo(double ax, double ay)
        {
            return this.SetYIntercept(ay - this.Slope * ax);
        }
    }
}
