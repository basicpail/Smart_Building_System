using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDashBoard.Models
{
    public class TempHumidModel
    {
        public string Id { get; set; }
        public string Curr_Time { get; set; }
        public string FirstFloorTemp { get; set; }
        public string FirstFloorHumid { get; set; }
        public string SecondFloorTemp { get; set; }
        public string SecondFloorHumid { get; set; }
        public string ThirdFloorTemp { get; set; }
        public string ThirdFloorHumid { get; set; }
    }
}
