using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelog
{
    public static class ObjectMerging
    {
        public static Dictionary<string, object> Flatten(IEnumerable<object> allFields)
        {
            if (!allFields.Any())
                return new Dictionary<string, object>();

            return (Dictionary<string, object>)allFields
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
            return ToDictionary(fields, new object [] { });
        }

        public static Dictionary<string, object> ToDictionary(object fields, object[] stack)
        {
            if (stack.Contains(fields))
                return new Dictionary<string, object>() { { "OcelogWarning", "Found a Circular Reference" } };

            var type = fields.GetType();
            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                && type.GetGenericArguments().First() == typeof(string))
                return BoxDictionaryValues(type.GetGenericArguments()[1], fields);

            return fields.GetType().GetProperties()
                .Where(prop => prop.CanRead
                    && prop.GetAccessors().Any(access => access.ReturnType != typeof(void) && access.GetParameters().Length == 0))
                .Where(prop => prop.GetValue(fields) != null)
                .ToDictionary(prop => prop.Name, prop => ToDictionaryOrObject(prop.GetValue(fields), stack.Push(fields)));
        }

        private static Dictionary<string, object> BoxDictionaryValues(Type valueType, object fields)
        {
            var method = typeof(ObjectMerging).GetMethod("GenericBoxDictionaryValues", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var genericMethod = method.MakeGenericMethod(valueType);
            return (Dictionary<string, object>)genericMethod.Invoke(null, new[] { fields });
        }

        private static Dictionary<string, object> GenericBoxDictionaryValues<T>(object fields)
        {
            var dictionary = (Dictionary<string, T>)fields;

            return dictionary.ToDictionary<KeyValuePair<string, T>, string, object>(pair => pair.Key, pair => pair.Value);
        }

        private static object ToDictionaryOrObject(object fields, object[] stack)
        {
            if (IsSimpleType(fields))
                return fields;

            if (fields.GetType().IsArray)
                return ToList((object[])fields, stack);

            if (typeof(IEnumerable<object>).IsAssignableFrom(fields.GetType()))
                return ToList((IEnumerable<object>)fields, stack);

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
        
        private static bool IsPredicate(object content)
        {
            var type = content.GetType();

            return type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof(Predicate<>)
                   && type.GetGenericArguments().Length == 1;
        }
        
        private static object[] Push(this object [] @this, object toPush)
        {
            return @this.Concat(new object[] { toPush }.AsEnumerable()).ToArray();
        }
    }
}
