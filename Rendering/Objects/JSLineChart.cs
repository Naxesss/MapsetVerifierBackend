using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Serialization;

namespace MapsetVerifierBackend.Rendering.Objects
{
    public class JSLineChart : JSChart<Point, Series>
    {
        // https://www.chartjs.org/docs/latest/getting-started/usage.html

        /*
         *  Json Seralization Mapping
         *  Note: Capitalization of variables matter; they must be lowercased.
        */

        protected struct DataSet : IDataSet
        {
            public string      label           { get; set; }
            public List<Point> data            { get; set; }
            public string      backgroundColor { get; set; }
            public string      borderColor     { get; set; }
            public bool        fill            { get; set; }
        }

        /*
         *  Attributes & Constructors
        */

        public JSLineChart(LineChart lineChart)
            : base(lineChart)
        { }

        protected override IDataSet ToDataSet(Series series)
        {
            return new DataSet()
            {
                label           = series.Label,
                data            = series.Points.Select(vec => new Point(vec)).ToList(),
                backgroundColor = $"rgba({series.Color.R}, {series.Color.G}, {series.Color.B}, {series.Color.A / 255f})",
                borderColor     = $"rgba({series.Color.R}, {series.Color.G}, {series.Color.B}, {series.Color.A / 255f})",
                fill            = false
            };
        }
    }
}
