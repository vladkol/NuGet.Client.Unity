﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace NuGet.Client.Unity.Tests
{
    public class TestCollectionOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var sortedMethods = new SortedDictionary<int, TTestCase>();

            foreach (TTestCase testCase in testCases)
            {
                IAttributeInfo attribute = testCase.TestMethod.Method.
                GetCustomAttributes((typeof(TestPriorityAttribute)
                .AssemblyQualifiedName)).FirstOrDefault();

                var priority = attribute.GetNamedArgument<int>("Priority");
                sortedMethods.Add(priority, testCase);
            }

            return sortedMethods.Values;
        }
    }
}
