using MapsetVerifierBackend.Rendering.Objects;
using MapsetVerifierFramework.objects.components;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapsetVerifierBackend.Rendering
{
    public class ChartRenderer : OverviewRenderer
    {
        public static string Render(LineChart chart)
        {
            JSLineChart jsChart = new JSLineChart(chart);
            return RenderField(chart.Title,
                Div("chart-container",
                    $"<canvas id=\"{jsChart.canvasId}\"></canvas>",
                    Script($"renderLineChart(\"{jsChart.canvasId}\", {jsChart.Serialize()})")
                )
            );
        }
    }
}
