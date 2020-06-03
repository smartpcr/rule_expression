﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class FunctionExpression
    {
        private readonly MethodInspect callInfo;

        private readonly MethodInspect[] supportedMethods =
        {
            new MethodInspect("Count", typeof(IEnumerable<string>), typeof(string), typeof(Enumerable)),
            new MethodInspect("Count", typeof(string[]), typeof(string), typeof(Enumerable)),
            new MethodInspect("Count", typeof(IEnumerable<decimal>), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("Count", typeof(decimal[]), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("Count", typeof(Enumerable), typeof(object), typeof(Enumerable)),
            new MethodInspect("Count", typeof(object[]), typeof(object), typeof(Enumerable)),
            new MethodInspect("DistinctCount", typeof(IEnumerable<string>), typeof(string), typeof(Enumerable)),
            new MethodInspect("DistinctCount", typeof(string[]), typeof(string), typeof(Enumerable)),
            new MethodInspect("DistinctCount", typeof(IEnumerable<decimal>), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("DistinctCount", typeof(decimal[]), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("DistinctCount", typeof(Enumerable), typeof(object), typeof(Enumerable)),
            new MethodInspect("DistinctCount", typeof(object[]), typeof(object), typeof(Enumerable)),

            new MethodInspect("Average", typeof(IEnumerable<decimal>), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("Average", typeof(decimal[]), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("Max", typeof(IEnumerable<decimal>), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("Max", typeof(decimal[]), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("Min", typeof(IEnumerable<decimal>), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("Min", typeof(decimal[]), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("Sum", typeof(IEnumerable<decimal>), typeof(decimal), typeof(Enumerable)),
            new MethodInspect("Sum", typeof(decimal[]), typeof(decimal), typeof(Enumerable))
        };

        private readonly Expression target;

        public FunctionExpression(Expression target, Type targetType, string methodName)
        {
            this.target = target;
            callInfo = supportedMethods.FirstOrDefault(m =>
                m.TargetType == targetType && m.MethodName.Equals(methodName, StringComparison.Ordinal));
            if (callInfo == null)
            {
                if (targetType.IsGenericType)
                {
                    var argType = targetType.GetGenericArguments()[0];
                    callInfo = new MethodInspect(methodName, targetType, argType, typeof(Enumerable));    
                }
                else if (targetType.IsArray)
                {
                    var argType = targetType.GetElementType();
                    callInfo = new MethodInspect(methodName, targetType, argType, typeof(Enumerable));
                }
            }

            if (callInfo == null) throw new NotSupportedException("Operator in condition is not supported for field type");
        }

        public MethodCallExpression Create()
        {
            switch (callInfo.MethodName)
            {
                case "DistinctCount":
                    return CreateDistinctCount();
                case "Count":
                    return Expression.Call(
                        callInfo.ExtensionType,
                        callInfo.MethodName,
                        new[] {callInfo.ArgumentType},
                        target);
                default:
                    return Expression.Call(
                        callInfo.ExtensionType,
                        callInfo.MethodName,
                        null,
                        target);
            }
        }

        private MethodCallExpression CreateDistinctCount()
        {
            Type[] typeArgument;
            if (callInfo.ArgumentType == typeof(string[]) || callInfo.ArgumentType == typeof(decimal[]))
                typeArgument = new[] {callInfo.ArgumentType.GetElementType()};
            else
            {
                typeArgument = new[] {callInfo.ArgumentType};
            }

            var distinct = Expression.Call(
                callInfo.ExtensionType,
                "Distinct",
                typeArgument,
                target);
            var count = Expression.Call(
                callInfo.ExtensionType,
                "Count",
                typeArgument,
                distinct);
            return count;
        }
    }
}