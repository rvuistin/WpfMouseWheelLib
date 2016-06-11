using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lada.Maths
{
    public class Int32HysteronGenerator
    {
        /// <summary>
        /// Computes a list of adjacent windowed hysteron operators that fit into a given period sampled at a given interval.
        /// </summary>
        /// <param name="hysteronCount">Identifies the desired number of adjacent windowed hysteron operators</param>
        /// <param name="period">Identifies the size of the adjacent windowed hysteron list</param>
        /// <param name="interval">Identifies the sampling interval</param>
        /// <param name="minHysteronWidth">Identifies the minimum hysteron width in normalized coordinates</param>
        /// <param name="minEndpointWidth">Identifies the minimum width of the left margin of the first template and the right margin of the last template in normalized coordinates</param>
        /// <returns>A list of hysteron operators</returns>
        public static IEnumerable<Func<int, int>> CreateFunctions(int hysteronCount, int period, int interval = 1, int minHysteronWidth = 2, int minEndpointWidth = 1)
        {
            int pos = 0;
            foreach (var template in CreateTemplates (period / interval, hysteronCount, minHysteronWidth, minEndpointWidth))
            {
                var delay = interval * (pos + template[0]);
                var width = interval * template[1];
                RefFunc<int, int, int> hf = (ref int state, int x) => MathEx.Hysteron (ref state, x - delay, width);
                yield return new Int32StateFunctionAdaptor (hf).F;
                pos += template.Sum ();
            }
        }

        /// <summary>
        /// Computes, in normalized coordinates, the optimal list of adjacent windowed hysterons that can fit into a given size.
        /// </summary>
        /// <param name="patternSize">Identifies the full size of the template list</param>
        /// <param name="hysteronCount">Identifies the desired number of windowed hysterons</param>
        /// <param name="minHysteronWidth">Identifies the minimum hysteron width</param>
        /// <param name="minEndpointWidth">Identifies the minimum width of the left margin of the first template and the right margin of the last template</param>
        /// <returns>A list of hysteron templates</returns>
        /// <remarks>
        /// An hysteron template consists of a windowed hysteron where the hysteron loop is fully contained in the given window
        /// It is represented by an array of 3 values in normalized coordinates:
        /// The first identifies the width of the left margin (the length between the front edges of the window and the hysteron loop)
        /// The second identifies the width of the hysteron loop
        /// The third identifies the width of the right margin (the length between the back edges of the hysteron loop and the window)
        /// </remarks>
        private static IEnumerable<int[]> CreateTemplates(int patternSize, int hysteronCount, int minHysteronWidth, int minEndpointWidth)
        {
            var availableSize = patternSize - 2 * minEndpointWidth;
            var hysteronWidth = availableSize / hysteronCount;
            if (hysteronWidth >= minHysteronWidth)
            {
                int[][] templates = new int[hysteronCount][];
                for (int i = 0; i < hysteronCount; ++i)
                    templates[i] = new int[3] { 0, hysteronWidth, 0 };
                // initialize left and right endpoints widths
                templates[0][0] = templates[hysteronCount - 1][2] = minEndpointWidth;
                if (hysteronWidth > minHysteronWidth)
                    for (int i = 0; i < hysteronCount; ++i)
                        EquilibrateTemplate (templates[i], minHysteronWidth);
                foreach (var t in templates)
                    yield return t;
            }
        }
        private static void EquilibrateTemplate(int[] h, int minHysteresis)
        {
            if (h[0] < h[2])
                SymmetrizeEndpoint (h, 0, 2, minHysteresis);
            else if (h[2] < h[0])
                SymmetrizeEndpoint (h, 2, 0, minHysteresis);
            EquilibrateEndpoints (h);
        }
        private static void SymmetrizeEndpoint(int[] h, int i0, int i2, int minHysteresis)
        {
            var delta = h[i2] - h[i0];
            if (h[1] - delta >= minHysteresis)
            {
                h[1] -= delta;
                h[i0] += delta;
            }
        }
        private static void EquilibrateEndpoints(int[] h)
        {
            var endpointSize = Math.Max (h[0], h[2]);
            var delta = (h[1] - endpointSize) / 3;
            if (delta > 0)
            {
                h[0] += delta;
                h[1] -= delta * 2;
                h[2] += delta;
            }
        }
    }
}
