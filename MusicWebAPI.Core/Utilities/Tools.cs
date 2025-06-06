using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Core.Utilities
{
    public static class Tools
    {
        private static IQueryable<T> ApplySorting<T>(IQueryable<T> source, string orderBy, bool isDescending)
        {
            if (string.IsNullOrWhiteSpace(orderBy)) return source;

            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, orderBy);
            var lambda = Expression.Lambda(property, param);

            var methodName = isDescending ? "OrderByDescending" : "OrderBy";
            var result = typeof(Queryable).GetMethods()
                                          .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                                          .MakeGenericMethod(typeof(T), property.Type)
                                          .Invoke(null, new object[] { source, lambda });

            return (IQueryable<T>)result;
        }

        public static async Task<PaginatedResult<T>> ToPaginatedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken, string orderBy = null, bool isDescending = true)
        {
            // Apply dynamic sorting first
            if (!string.IsNullOrEmpty(orderBy))
            {
                source = ApplySorting(source, orderBy, isDescending);
            }

            // Get total count of items in the query
            var totalItems = await source.CountAsync(cancellationToken);

            // Get the data for the specific page after sorting
            var itemsOnPage = await source
                .Skip((pageNumber - 1) * pageSize) // Skip the previous pages
                .Take(pageSize) // Take the current page's size
                .ToListAsync(cancellationToken);

            // Return a PaginatedList object
            return new PaginatedResult<T>(itemsOnPage, totalItems, pageNumber, pageSize);
        }

        public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                return true; // ⚠️ Allow all users – safe only for development!
            }
        }

        public class RoleBasedAuthorizationFilter : IDashboardAuthorizationFilter
        {
            private readonly string _requiredRole;

            public RoleBasedAuthorizationFilter(string requiredRole)
            {
                _requiredRole = requiredRole;
            }

            public bool Authorize([NotNull] DashboardContext context)
            {
                // Replace the problematic line with the following:
                var httpContext = (HttpContext)context.GetType()
                    .GetProperty("HttpContext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(context);

                return httpContext?.User.Identity?.IsAuthenticated == true &&
                       httpContext.User.IsInRole(_requiredRole);
            }
        }

        public static object GetUserId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }
            return userId.ToString();
        }
    }
}
