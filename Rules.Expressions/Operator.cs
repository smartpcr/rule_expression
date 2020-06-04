﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Operator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions
{
    public enum Operator
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterOrEqual,
        LessThan,
        LessOrEqual,
        Contains,
        NotContains,
        ContainsAll,
        NotContainsAll,
        StartsWith,
        NotStartsWith,
        In,
        NotIn,
        AllIn,
        NotAllIn,
        AnyIn,
        NotAnyIn,
        IsNull,
        NotIsNull,
        IsEmpty,
        NotIsEmpty,
        DiffWithinPct,
        AllInRangePct,
    }
}