using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WASJL
{
    public interface IJSONObject { 
        T GetJSONObject<T>(string key) where T: class, new();
    }
    public class WAS_Deserializer : IJSONObject
    {
        public string JSONString { get; }

        private Dictionary<object,object> _BackingDict = new Dictionary<object,object>();
        public WAS_Deserializer(string jsonString)
        {
            JSONString = jsonString??throw new ArgumentNullException(nameof(jsonString));
            WAS_Lexer lexer = new WAS_Lexer(jsonString);
            WAS_Parser parser = new WAS_Parser(lexer);
            _BackingDict = parser.Parse() as Dictionary<object, object> ?? new Dictionary<object, object>();
            if (!_BackingDict.Any()) throw new Exception("Unable to parse JSON string.");
        }
        public T GetJSONObject<T>(string? key = null) where T : class, new()
        {
            Dictionary<object, object> _value = string.IsNullOrWhiteSpace(key) ? _BackingDict : (Dictionary<object, object>)_BackingDict[key];
            T result = new T();
            ConvertDictionaryToObject<T>(result, _value);

            return result;
        }

        private static void ConvertDictionaryToObject<T>(T? result, Dictionary<object, object> dict) where T : class, new() {
            if (!dict.Any()) return;
            foreach (var kvp in dict) {
                var property = result?.GetType().GetProperty(kvp.Key.ToString() ?? string.Empty);
                if (property != null&& (property.CanWrite || property.GetSetMethod(true).IsPublic)) 
                {
                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            Type genericArgType = property.PropertyType.GetGenericArguments().FirstOrDefault();
                            Type listType = typeof(List<>).MakeGenericType(genericArgType);
                            IList nestedList = Activator.CreateInstance(listType) as IList ?? new List<object>();
                            List<object>? listData = kvp.Value as List<object>;

                            if (listData != null)
                            {
                                foreach (object? item in listData)
                                {
                                    if (genericArgType.IsClass)
                                    {
                                        object? nestedObj = Activator.CreateInstance(genericArgType);
                                        ConvertDictionaryToObject(nestedObj, item as Dictionary<object, object> ?? new Dictionary<object, object>());
                                        nestedList.Add(nestedObj);
                                    }
                                    else
                                    {
                                        object convertedListitem = Convert.ChangeType(item, genericArgType);
                                        nestedList.Add(convertedListitem);
                                    }
                                }
                            }
                            property.SetValue(result, nestedList);
                        }
                        else
                        {
                            object nestedObj = Activator.CreateInstance(property.PropertyType);
                            Dictionary<object, object>? nestedData = kvp.Value as Dictionary<object, object>;
                            if (nestedData != null)
                            {
                                ConvertDictionaryToObject(nestedObj, nestedData);
                            }
                            property.SetValue(result, nestedObj);
                        }
                    }
                    else if (property.PropertyType.IsEnum)
                    {
                        if (kvp.Value.GetType() != typeof(int)) {
                            throw new Exception("Expected int value, recieved " + kvp.Value.ToString() + "while populating enum " + property.PropertyType.Name);
                        }
                        if ((int)kvp.Value >= property.PropertyType.GetEnumValues().Length) {
                            throw new Exception("Index " + kvp.Value.ToString()+" does not correspond to any value in enum "+ property.PropertyType.Name);
                        }
                        object convertedValue = Convert.ChangeType(kvp.Value, typeof(int));
                        property.SetValue(result, convertedValue);
                    }
                    else {
                        object convertedValue = Convert.ChangeType(kvp.Value, property.PropertyType);
                        property.SetValue(result, convertedValue);
                    }
                }
            }
        }
    }
}
