﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Web.Pipeline;

namespace Squidex.Web;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiCostsAttribute(double costs) : ServiceFilterAttribute(typeof(ApiCostsFilter)), IApiCostsFeature
{
    public double Costs { get; } = costs;
}
