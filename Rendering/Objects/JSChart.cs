﻿using MapsetVerifierFramework.objects.components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapsetVerifierBackend.Rendering.Objects
{
    public abstract class JSChart<T, U>
    {
        // https://www.chartjs.org/docs/latest/getting-started/usage.html

        /*
         *  Json Seralization Mapping
         *  Note: Capitalization of variables matter; they must be lowercased.
        */

        /// <summary> Represents the chart itself. </summary>
        public struct ChartData
        {
            public string         label    { get; set; }
            public List<IDataSet> datasets { get; set; }
        }

        public interface IDataSet
        {
            public string  label { get; set; }
            public List<T> data  { get; set; }
        }

        /*
         *  Attributes & Constructors
        */

        public string    canvasId;
        public ChartData data;

        public JSChart(Chart<U> chart)
        {
            canvasId = $"chart-{TitleClassName(chart.Title)}";
            data     = new ChartData()
            {
                label    = chart.Title,
                datasets = chart.Data.Select(data => ToDataSet(data)).ToList()
            };
        }

        protected abstract IDataSet ToDataSet(U data);

        /*
         *  Utility
        */

        /// <summary> Returns the given chart title as a html class name (e.g. "Star Rating" -> "star-rating"). </summary>
        /// <param name="text"> The chart title to format. </param>
        public static string TitleClassName(string title) => title.ToLower().Replace(" ", "-");

        /*
         *  Serialization
        */

        public string Serialize() => JsonConvert.SerializeObject(data);
    }
}
