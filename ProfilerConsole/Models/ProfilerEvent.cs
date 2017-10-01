using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProfilerConsole
{
    [Serializable]
    [DataContract]
    internal class ProfilerEvent
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public int Duration { get; set; }
    }
}
