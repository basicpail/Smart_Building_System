using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BogusMqttWinPublishApp
{
    public class SensorInfo
    {
        public string Dev_Id { get; set; }

        public string Curr_Time { get; set; }

        public float Temp { get; set; }

        public float Humid { get; set; }

    }
}
