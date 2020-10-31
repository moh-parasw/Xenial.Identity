using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Xenial.Identity.Infrastructure
{
    public class EnumRouteConstraint<TEnum> : IRouteConstraint
        where TEnum : struct, Enum
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            // retrieve the candidate value
            var candidate = values[routeKey]?.ToString();
            // attempt to parse the candidate to the required Enum type, and return the result
            var result = Enum.TryParse<TEnum>(candidate, out var _);
            return result;
        }
    }
}
