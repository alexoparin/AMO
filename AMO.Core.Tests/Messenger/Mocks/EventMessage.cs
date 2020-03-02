using System;
using System.Collections.Generic;
using AMO.Core;

namespace DbImport2.Core.Tests.Messaging.Mocks
{
    internal class EventMessage : ParameterCollection, IEquatable<EventMessage>
    {
        public string SampleParam
        {
            get { return getParamInternal<string>(); }
            set { setParamInternal(value); }
        }


        public EventMessage(string sampleParam)
        {
            SampleParam = sampleParam;
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as EventMessage);
        }

        public bool Equals(EventMessage other)
        {
            return other != null &&
                   SampleParam == other.SampleParam;
        }

        public override int GetHashCode()
        {
            return 1044799860 + EqualityComparer<string>.Default.GetHashCode(SampleParam);
        }

        public static bool operator ==(EventMessage left, EventMessage right)
        {
            return EqualityComparer<EventMessage>.Default.Equals(left, right);
        }

        public static bool operator !=(EventMessage left, EventMessage right)
        {
            return !(left == right);
        }
    }
}
