using System.Collections.Generic;

namespace AMO.Core.Infrastructure
{
    public interface IParameterCollection : ICollection<KeyValuePair<string, object>>
    {
        bool   HasParam(string paramName);
        object GetParam(string paramName);
        T      GetParam<T>(string paramName);
        void   SetParam(string paramName, object paramValue);
        bool   RemoveParam(string paramName);
    }
}
