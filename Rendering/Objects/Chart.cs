using System;
using System.Collections.Generic;
using System.Text;

namespace MapsetVerifierBackend.Rendering.Objects
{
    public class Chart<T>
    {
        public string Title { get; }
        public List<T> Data { get; private set; }

        public Chart(string title, List<T> data = null)
        {
            Title = title;
            Data = data ?? new List<T>();
        }
    }
}
