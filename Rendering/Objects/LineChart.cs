using System;
using System.Collections.Generic;
using System.Text;

namespace MapsetVerifierBackend.Rendering.Objects
{
    public class LineChart : Chart<Series>
    {
        public string XLabel { get; }
        public string YLabel { get; }

        public LineChart(string title, string xLabel, string yLabel, List<Series> data = null)
            : base(title, data)
        {
            XLabel = xLabel;
            YLabel = yLabel;
        }
    }
}
