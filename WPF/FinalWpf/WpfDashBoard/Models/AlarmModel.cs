using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDashBoard.Models
{
    public class AlarmModel
    {
        public string Num { get; set; }
        public string Curr_Time { get; set; }
        public string tempWarning_1F { get; set; }
        public string tempWarning_2F { get; set; }
        public string tempWarning_3F { get; set; }
        public string WaterWarning { get; set; }
        public string FireWarning { get; set; }
        public string DetectionWarning { get; set; }
        public string AlramStartTimeconfig_2F{get;set;}
        public string AlramEndTimeconfig_2F{get;set;}
        public string AlramStartTimeconfig_3F{get;set;}
        public string AlramEndTimeconfig_3F{get;set;}
    }
}
