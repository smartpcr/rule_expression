﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeExtension.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Helpers
{
    using System;
    using System.ComponentModel;

    public static class TypeExtension
    {
        public static object ConvertValue(this Type type, string value)
        {
            return TypeDescriptor.GetConverter(type).ConvertFrom(value);
        }

        public static bool IsNumericType(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsPrimitiveType(this Type type)
        {
            return type.IsPrimitive || type.IsValueType;
        }

        /// <summary>
        /// primitive type, value type and string, including nullable types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsScalarType(this Type type)
        {
            return type.IsPrimitiveType() || type == typeof(string) || Nullable.GetUnderlyingType(type)?.IsPrimitiveType() == true;
        }

        public static bool IsDateType(this Type type)
        {
            return Type.GetTypeCode(type) == TypeCode.DateTime;
        }
    }
}