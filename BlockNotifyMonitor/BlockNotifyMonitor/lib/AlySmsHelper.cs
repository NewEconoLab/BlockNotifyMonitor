using System;
using System.Collections.Generic;
using System.Text;

namespace BlockNotifyMonitor
{
    class AlySmsHelper
    {
        public AlySmsConfig config { get; set; }

    }
    class AlySmsConfig
    {
        public string accessKeyId { get; set; }
        public string accessKeySecret { get; set; }
        public string phoneNumbers { get; set; }
        public string dailyTime { get; set; }
    }
}
