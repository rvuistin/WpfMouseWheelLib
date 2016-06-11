using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Lada.Maths
{
    public class Int32FunctionTranslator : IFunction<int>
    {
        public                          Int32FunctionTranslator(Func<int, int> f, int dx = 0, int dy = 0)
        {
            this.OriginalFunction = f;
            this.TranslateX = dx;
            this.TranslateY = dy;
        }
        public Int32FunctionTranslator  Clone()
        {
            return new Int32FunctionTranslator (this.OriginalFunction, this.TranslateX, this.TranslateY);
        }
        public int                      F(int x)
        {
            return this.OriginalFunction (x - this.TranslateX) + this.TranslateY;
        }
        public Func<int, int>           OriginalFunction
        {
            get; set;
        }
        public int                      TranslateX
        {
            get; set;
        }
        public int                      TranslateY
        {
            get; set;
        }
        public void                     Translate(int dx, int dy)
        {
            this.TranslateX += dx;
            this.TranslateY += dy;
        }
        public override string          ToString()
        {
            return string.Format ("Translation=({0}, {1})", TranslateX, TranslateY);
        }
    }

    public class Int32FunctionSummator : IFunction<int>
    {
        public static Func<int, int>    CreateFunction(IEnumerable<Func<int, int>> functions)
        {
            if (functions == null)
            {
                throw new ArgumentNullException ("functions");
            }
            var functionArray = functions.ToArray ();
            int count = functionArray.Length;
            if (count == 0)
            {
                return null;
            }
            else if (count == 1)
            {
                return functionArray[0];
            }
            else
            {
                return new Int32FunctionSummator (functionArray).F;
            }
        }
        public Int32FunctionSummator    Clone()
        {
            return new Int32FunctionSummator (Functions);
        }
        public int                      F(int x)
        {
            return Functions.Sum (f => f (x));
        }
        public List<Func<int, int>>     Functions
        {
            get; private set;
        }

        protected                       Int32FunctionSummator(IEnumerable<Func<int, int>> functions)
        {
            Functions = new List<Func<int, int>> (functions);
        }
    }

    public class Int32StateFunctionAdaptor : IFunction<int>
    {
        private int _state;
        private RefFunc<int, int, int> _f;

        public Int32StateFunctionAdaptor(RefFunc<int, int, int> f, int state = 0)
        {
            _f = f;
            _state = state;
        }

        [DebuggerStepThrough]
        public int F(int x)
        {
            return _f (ref _state, x);
        }
        public int State
        {
            [DebuggerStepThrough]
            get
            {
                return _state;
            }
            [DebuggerStepThrough]
            set
            {
                _state = value;
            }
        }
        public Int32StateFunctionAdaptor Clone()
        {
            return new Int32StateFunctionAdaptor (_f, _state);
        }
    }

    public class Int32FunctionToDifferentialFunctionAdaptor : IDifferentialFunction<int>
    {
        public Int32FunctionToDifferentialFunctionAdaptor(Func<int, int> function, int x = 0, int y = 0)
        {
            Function = function;
            X = x;
            Y = y;
        }
        public Func<int, int> Function
        {
            get; private set;
        }
        public int X
        {
            get; private set;
        }
        public int Y
        {
            get; private set;
        }
        public int DF(int dx)
        {
            var y = Function (X += dx);
            var dy = y - Y;
            Y = y;
            return dy;
        }
        public Int32FunctionToDifferentialFunctionAdaptor Clone()
        {
            return new Int32FunctionToDifferentialFunctionAdaptor (Function, X, Y);
        }
    }
    public class Int32DifferentialFunctionToFunctionAdaptor : IFunction<int>
    {
        public Int32DifferentialFunctionToFunctionAdaptor(Func<int, int> differentialFunction, int x = 0, int y = 0)
        {
            DifferentialFunction = differentialFunction;
            X = x;
            Y = y;
        }
        public Func<int, int> DifferentialFunction
        {
            get; private set;
        }
        public int X
        {
            get; private set;
        }
        public int Y
        {
            get; private set;
        }
        public int F(int x)
        {
            var dx = x - X;
            X = x;
            return Y += DifferentialFunction (dx);
        }
        public Int32DifferentialFunctionToFunctionAdaptor Clone()
        {
            return new Int32DifferentialFunctionToFunctionAdaptor (DifferentialFunction, X, Y);
        }
    }

    public class Int32DifferentialFunctionPatternModulator : IDifferentialFunction<int>
    {
        private Int32FunctionTranslator _carrier;
        private int _patternHeight;
        private int _x, _y;

        public Int32DifferentialFunctionPatternModulator(Func<int, int> carrier, Func<int, int> pattern, int period)
          : this (new Int32FunctionTranslator (carrier), pattern, period)
        {
        }
        public Int32DifferentialFunctionPatternModulator(Int32FunctionTranslator carrier, Func<int, int> pattern, int period)
        {
            _carrier = carrier;
            _patternHeight = carrier.F (period);
            Pattern = pattern;
            Period = period;
        }
        public int DF(int dx)
        {
            if (dx == 0)
                return 0;
            else if (dx < 0)
                return NegativeMove (dx);
            else
                return PositiveMove (dx);
        }
        public Func<int, int> Carrier
        {
            get
            {
                return _carrier.F;
            }
        }
        public Func<int, int> Pattern
        {
            get; private set;
        }
        public int Period
        {
            get; private set;
        }
        public Int32DifferentialFunctionPatternModulator Clone()
        {
            return new Int32DifferentialFunctionPatternModulator (_carrier.Clone (), Pattern, Period);
        }
        public int Reset()
        {
            return CurrentPattern (0);
        }

        private int NegativeMove(int dx)
        {
            var x1 = _x + dx;
            if (x1 >= 0)
                return CurrentPattern (x1);

            // compute new origin in current referential
            var o1x = MathEx.Floor (x1, Period);
            var o1y = Carrier (o1x);
            // exit current cell
            var dy = _patternHeight * Pattern (0) - _y;
            // cross intermediate cells
            var v1y = Carrier (o1x + Period);
            dy += v1y;
            _patternHeight = v1y - o1y;
            // move origin to new referential
            _carrier.Translate (o1x, o1y);
            _x = x1 - o1x;
            // enter new cell
            return dy += EnterPattern (Period);
        }
        private int PositiveMove(int dx)
        {
            var x1 = _x + dx;
            if (x1 <= Period)
                return CurrentPattern (x1);

            // compute new origin in current referential
            var o1x = MathEx.Floor (x1, Period);
            var o1y = Carrier (o1x);
            // exit current cell
            var dy = _patternHeight * Pattern (Period) - _y;
            // cross intermediate cells
            dy += o1y - _patternHeight;
            // move origin to new referential
            _carrier.Translate (o1x, o1y);
            _x = x1 - o1x;
            _patternHeight = Carrier (Period);
            // enter new pattern
            return dy += EnterPattern (0);
        }
        private int EnterPattern(int xEntryPoint)
        {
            var y = _patternHeight * Pattern (xEntryPoint);
            _y = _patternHeight * Pattern (_x);
            return _y - y;
        }
        private int CurrentPattern(int x1)
        {
            var y1 = _patternHeight * Pattern (x1);
            var dy = y1 - _y;
            _x = x1;
            _y = y1;
            return dy;
        }
    }
}
