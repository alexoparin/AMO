using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AMO.Core.Infrastructure;

namespace AMO.Core
{
    public class ParameterCollection : IParameterCollection
    {
        private readonly Dictionary<string, object> _inner;


        public ParameterCollection()
        {
            _inner = new Dictionary<string, object>();
        }


        public bool HasParam(string paramName)
        {
            validateParamName(paramName);
            return _inner.ContainsKey(paramName);
        }

        public object GetParam(string paramName)
        {
            validateParamName(paramName);

            object value;
            return _inner.TryGetValue(paramName, out value) ? value : null;
        }

        public T GetParam<T>(string paramName)
        {
            return getParamInternal<T>(paramName);
        }

        public void SetParam(string paramName, object paramValue)
        {
            setParamInternal(paramValue, paramName);
        }

        public bool RemoveParam(string paramName)
        {
            validateParamName(paramName);

            return _inner.Remove(paramName);
        }

        #region ICollection<KeyValuePair<string, object>>

        public int Count      { get { return _inner.Count; } }
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly { get { return false; } }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            object value;
            return _inner.TryGetValue(item.Key, out value) && value == item.Value;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            if (array == null)                            { throw new ArgumentNullException("array"); }
            if (arrayIndex >= array.Length)               { throw new ArgumentOutOfRangeException("arrayIndex"); }
            if (array.Length < _inner.Count + arrayIndex) { throw new ArgumentException("Array is too small to house all parameters.", "array"); }

            foreach (KeyValuePair<string, object> parameter in _inner)
            {
                array[arrayIndex++] = parameter;
            }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            _inner[item.Key] = item.Value;
        }

        public void Clear()
        {
            _inner.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            object value;
            return _inner.TryGetValue(item.Key, out value) && Equals(value, item.Value) && _inner.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        protected T getParamInternal<T>([CallerMemberName] string paramName = "")
        {
            validateParamName(paramName);

            var value = GetParam(paramName);
            return typeof(T).IsValueType && value == null ? default(T) : (T)value;
        }

        protected void setParamInternal(object paramValue, [CallerMemberName] string paramName = "")
        {
            validateParamName(paramName);

            _inner[paramName] = paramValue;
        }

        private static void validateParamName(string paramName)
        {
            if (paramName == null)
            {
                throw new ArgumentNullException("paramName");
            }
        }
    }
}
