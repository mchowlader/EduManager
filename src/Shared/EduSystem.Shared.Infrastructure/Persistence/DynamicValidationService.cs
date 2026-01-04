using EduSystem.Shared.Infrastructure.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EduSystem.Shared.Infrastructure.Persistence;

public class DynamicValidationService : IDynamicValidationService
{
    public async Task<(bool IsValid, string Message)> ValidateEntityAsync<TEntity>(
        DbContext dbContext, 
        TEntity entity, 
        CancellationToken cancellationToken = default) where TEntity : class
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
        if (entityType == null) return (true, string.Empty);

        // 1. Property-Level Validation (MaxLength, Required)
        foreach (var property in entityType.GetProperties())
        {
            var value = property.PropertyInfo?.GetValue(entity);

            // Required Check
            if (!property.IsNullable)
            {
                if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
                {
                    return (false, $"{property.Name} is required.");
                }
            }

            // MaxLength Check
            var maxLength = property.GetMaxLength();
            if (maxLength.HasValue && value is string str && str.Length > maxLength.Value)
            {
                return (false, $"{property.Name} length exceeds the maximum limit of {maxLength.Value}.");
            }
        }

        // 2. Uniqueness Validation (Indices)
        var indexes = entityType.GetIndexes().Where(i => i.IsUnique);

        foreach (var index in indexes)
        {
            var properties = index.Properties;
            var query = dbContext.Set<TEntity>().AsNoTracking();

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression? combinedExpression = null;

            foreach (var property in properties)
            {
                var value = property.PropertyInfo?.GetValue(entity);
                if (value == null) continue;

                // Build expression: e.PropertyName == value
                var propertyAccess = Expression.Property(parameter, property.Name);
                var constantValue = Expression.Constant(value, property.ClrType);
                var equality = Expression.Equal(propertyAccess, constantValue);

                combinedExpression = combinedExpression == null 
                    ? equality 
                    : Expression.AndAlso(combinedExpression, equality);
            }

            if (combinedExpression == null) continue;

            // Handle Update: Exclude current record by Primary Key
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey != null)
            {
                Expression? pkExclusion = null;
                foreach (var pkProp in primaryKey.Properties)
                {
                    var pkValue = pkProp.PropertyInfo?.GetValue(entity);
                    if (pkValue == null || IsDefaultValue(pkValue, pkProp.ClrType)) continue;

                    var pkPropertyAccess = Expression.Property(parameter, pkProp.Name);
                    var pkConstantValue = Expression.Constant(pkValue, pkProp.ClrType);
                    var pkInequality = Expression.NotEqual(pkPropertyAccess, pkConstantValue);

                    pkExclusion = pkExclusion == null 
                        ? pkInequality 
                        : Expression.AndAlso(pkExclusion, pkInequality);
                }

                if (pkExclusion != null)
                {
                    combinedExpression = Expression.AndAlso(combinedExpression, pkExclusion);
                }
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
            
            if (await query.AnyAsync(lambda, cancellationToken))
            {
                var propNames = string.Join(" and ", properties.Select(p => p.Name));
                return (false, $"{entityType.DisplayName()} with this {propNames} already exists.");
            }
        }

        return (true, string.Empty);
    }

    private static bool IsDefaultValue(object value, Type type)
    {
        if (type.IsValueType)
        {
            return value.Equals(Activator.CreateInstance(type));
        }
        return value == null;
    }
}
