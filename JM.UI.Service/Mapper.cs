using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JM.Application.Common.Generic
{
    public static class Mapper<TSource, TDestination> where TDestination : new()
    {
        private static readonly Func<TSource, TDestination> MapFunc = CreateMapFunc();

        private static Func<TSource, TDestination> CreateMapFunc()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var bindings = typeof(TDestination).GetProperties()
                .Where(destProp => destProp.CanWrite)
                .Select(destProp =>
                {
                    var sourceProp = typeof(TSource).GetProperty(destProp.Name);
                    if (sourceProp == null) return null;

                    return Expression.Bind(destProp, Expression.Property(sourceParam, sourceProp));
                })
                .Where(binding => binding != null);

            var body = Expression.MemberInit(Expression.New(typeof(TDestination)), bindings);
            var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);

            return lambda.Compile();
        }

        public static TDestination Map(TSource source) => MapFunc(source);
    }
}