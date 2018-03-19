using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ocelog
{
    public static class ObjectMerging
    {
        public static Dictionary<string, object> Flatten(IEnumerable<object> allFields)
        {
            if (!allFields.Any())
                return new Dictionary<string, object>();

            return allFields
                .Select(ToDictionary)
                .Aggregate((first, second) => Merge(second, first));
        }

        private static Dictionary<string, object> Merge(Dictionary<string, object> doc1, Dictionary<string, object> doc2)
        {
            var mergedDictionary = new Dictionary<string, object>(doc1);
            foreach (var pair in doc2)
            {
                var key = pair.Key;
                var val = pair.Value;
                if (mergedDictionary.ContainsKey(key))
                {
                    val = Merge(mergedDictionary[key], val);
                }
                mergedDictionary[key] = ToDictionaryOrObject(val, new object[] { });
            }
            return mergedDictionary;
        }

        private static object Merge(object doc1, object doc2)
        {
            if (IsSimpleType(doc1))
                return doc1;

            if (typeof(IEnumerable<object>).IsAssignableFrom(doc1.GetType()) && typeof(IEnumerable<object>).IsAssignableFrom(doc2.GetType()))
                return ((IEnumerable<object>)doc2).Concat((IEnumerable<object>)doc1);

            return Merge(ToDictionary(doc1), ToDictionary(doc2));
        }

        public static Dictionary<string, object> ToDictionary(object fields)
        {
            return ToDictionary(fields, new object[] { });
        }

        public static Dictionary<string, object> ToDictionary(object fields, object[] stack)
        {
            if (stack.Contains(fields))
                return new Dictionary<string, object>() { { "OcelogWarning", "Found a Circular Reference" } };

            var type = fields.GetType();
            if (IsCompatibleDictionary(type))
                return BoxDictionaryValues(type.GenericTypeArguments[1], fields);

            return type.GetProperties()
                .Where(prop => !IsDelegateType(prop.PropertyType))
                .Where(prop => prop.CanRead
                    && prop.GetAccessors()
                        .Any(access => access.ReturnType != typeof(void)
                            && access.GetParameters().Length == 0))
                .Where(prop => SafeGetValue(fields, prop) != null)
                .ToDictionary(prop => prop.Name, prop => ToDictionaryOrObject(SafeGetValue(fields, prop), stack.Push(fields)));
        }

        private static bool IsCompatibleDictionary(Type type)
        {
            return type.GetInterfaces()
                .Where(valueInterface => valueInterface.IsGenericType)
                .Where(valueInterface => valueInterface.GenericTypeArguments.First() == typeof(string))
                .Any(valueInterface => (typeof(IDictionary<,>).IsAssignableFrom(valueInterface.GetGenericTypeDefinition())));
        }

        private static Dictionary<string, object> BoxDictionaryValues(Type valueType, object fields)
        {
            var method = typeof(ObjectMerging).GetMethod("GenericBoxDictionaryValues", BindingFlags.Static | BindingFlags.NonPublic);
            var genericMethod = method.MakeGenericMethod(valueType);
            return (Dictionary<string, object>)genericMethod.Invoke(null, new[] { fields });
        }

        private static Dictionary<string, object> GenericBoxDictionaryValues<T>(object fields)
        {
            var dictionary = (IDictionary<string, T>)fields;

            return dictionary.ToDictionary<KeyValuePair<string, T>, string, object>(pair => pair.Key, pair => pair.Value);
        }

        private static bool IsCompatibleEnumerable(Type type)
        {
            return type.GetInterfaces()
                .Where(valueInterface => valueInterface.IsGenericType)
                .Any(valueInterface => (typeof(IEnumerable<>).IsAssignableFrom(valueInterface.GetGenericTypeDefinition())));
        }

        private static IEnumerable<object> BoxEnumerableValues(Type valueType, object fields)
        {
            var method = typeof(ObjectMerging).GetMethod("GenericBoxEnumerableValues", BindingFlags.Static | BindingFlags.NonPublic);
            var genericMethod = method.MakeGenericMethod(valueType);
            return (IEnumerable<object>)genericMethod.Invoke(null, new[] { fields });
        }

        private static IEnumerable<object> GenericBoxEnumerableValues<T>(object fields)
        {
            var enumerable = (IEnumerable<T>)fields;

            return enumerable.Cast<object>();
        }

        private static object SafeGetValue(object fields, System.Reflection.PropertyInfo prop)
        {
            try
            {
                return prop.GetValue(fields);
            }
            catch
            {
                return new Dictionary<string, object>() { { "OcelogWarning", "Exception thrown by invocation" } };
            }
        }

        private static object ToDictionaryOrObject(object fields, object[] stack)
        {
            if (IsSimpleType(fields))
                return fields;

            if (IsInSpecialCaseList(fields))
                return fields.ToString();

            if (fields.GetType().IsArray || (fields is IEnumerable && !IsCompatibleEnumerable(fields.GetType())))
                return ToList(((IEnumerable)fields).Cast<object>(), stack);

            if (!IsCompatibleDictionary(fields.GetType()) && IsCompatibleEnumerable(fields.GetType()))
                return ToList(BoxEnumerableValues(fields.GetType().GenericTypeArguments[0], fields), stack);

            if (IsPredicate(fields))
                return fields;

            return ToDictionary(fields, stack);
        }

        private static object ToList(IEnumerable<object> fields, object[] stack)
        {
            return fields.Select(val => ToDictionaryOrObject(val, stack));
        }

        private static bool IsSimpleType(object fields)
        {
            var type = fields.GetType();

            return type.IsValueType
                || type == typeof(string)
                || type.IsEnum;
        }

        private static bool IsInSpecialCaseList(object fields)
        {
            var type = fields.GetType();

            return typeof(MethodBase).IsAssignableFrom(type);
        }

        private static bool IsPredicate(object content)
        {
            var type = content.GetType();

            return type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof(Predicate<>)
                   && type.GenericTypeArguments.Length == 1;
        }

        private static bool IsDelegateType(Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type);
        }

        private static object[] Push(this object[] @this, object toPush)
        {
            return @this.Concat(new object[] { toPush }.AsEnumerable()).ToArray();
        }
    }
}
