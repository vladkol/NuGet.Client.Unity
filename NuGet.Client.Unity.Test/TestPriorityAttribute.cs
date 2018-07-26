using System;
using System.Collections.Generic;
using System.Text;

namespace NuGet.Client.Unity.Test
{
    public class TestPriorityAttribute : Attribute
    {
        public int Priority { get; set; }
        public TestPriorityAttribute(int Priority)
        {
            this.Priority = Priority;
        }
    }
}
