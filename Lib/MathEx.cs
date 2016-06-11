using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lada
{
    public static class MathEx
    {
        public static TimeSpan          Max(TimeSpan t1, TimeSpan t2)
        {
            return t1 > t2 ? t1 : t2;
        }
        public static TimeSpan          Min(TimeSpan t1, TimeSpan t2)
        {
            return t1 < t2 ? t1 : t2;
        }
        public static int               GreatestCommonDivisor(int v1, int v2)
        {
            // take absolute values
            if (v1 < 0)
                v1 = -v1;
            if (v2 < 0)
                v2 = -v2;
            do
            {
                if (v1 < v2)
                    v2 = Interlocked.Exchange (ref v1, v2); // swap the two operands
                v1 = v1 % v2;
            } while (v1 != 0);
            return v2;
        }
        public static double            GreatestCommonDivisor(double v1, double v2)
        {
            // take absolute values
            if (v1 < 0)
                v1 = -v1;
            if (v2 < 0)
                v2 = -v2;
            do
            {
                if (v1 < v2)
                    v2 = Interlocked.Exchange (ref v1, v2); // swap the two operands
                v1 = v1 % v2;
            } while (v1 != 0);
            return v2;
        }
        public static IEnumerable<int>  GetDivisorsLessThanOrEqual(int value, int max)
        {
            Debug.Assert (max >= 1);
            max = Math.Min (max, Math.Abs (value / 2));
            yield return 1;
            for (int i = 2; i <= max; ++i)
                if ((value % i) == 0)
                    yield return i;
        }

        // State-less functions
        public static int               Step(int x)
        {
            return x < 0 ? 0 : 1;
        }
        public static int               Rect(int x, int width = 1, int height = 1)
        {
            return x < 0 || x >= width ? 0 : height;
        }
        // State-less periodic functions
        public static int               Ceiling(int x, int period)
        {
            var n = x / period;
            if (x > 0 && x % period != 0)
                n += 1;
            return n * period;
        }
        public static int               Floor(int x, int period)
        {
            var n = x / period;
            if (x < 0 && x % period != 0)
                n -= 1;
            return n * period;
        }
        public static int               SawTooth(int x, int period)
        {
            var y = x % period;
            return y < 0 ? y + period : y;
        }
        public static int               Snap(int x, int period)
        {
            return Floor (x + period / 2, period);
        }

        // State-full functions
        /// <summary>
        /// Associates the input with the result of an operator that takes the path of a loop,
        /// and its next state depends on its past state.
        /// </summary>
        /// <param name="state">Represents the state of the operator.</param>
        /// <param name="x">Represents the input of the operator</param>
        /// <param name="width">Represents the width of the loop</param>
        /// <param name="height">Represents the height of the loop</param>
        /// <returns></returns>
        public static int               Hysteron(ref int state, int x, int width = 1, int height = 1)
        {
            if (x >= width)
                return state = height;
            else if (x <= 0)
                return state = 0;
            else
                return state;
        }

        // Functional operators
        public static Func<int, int>    Multiply(Func<int, int> f, Func<int, int> g)
        {
            return x => f (x) * g (x);
        }
        public static Func<int, int>    Compose(Func<int, int> f, Func<int, int> g)
        {
            return x => f (g (x));
        }
        public static Func<int, int>    Floor(Func<int, int> f, int period)
        {
            return x => f (Floor (x, period));
        }
        public static Func<int, int>    Ceiling(Func<int, int> f, int period)
        {
            return x => f (Ceiling (x, period));
        }
        public static Func<int, int>    Periodic(Func<int, int> f, int period)
        {
            return x => f (SawTooth (x, period));
        }
        public static Func<int, int>    Modulate(Func<int, int> carrier, Func<int, int> cellFunction, int period)
        {
            return x =>
            {
                var n0 = Floor (carrier, period) (x);
                var n1 = Floor (carrier, period) (x + period);
                return n0 + (n1 - n0) * (Periodic (cellFunction, period) (x));
            };
        }
    }
}
