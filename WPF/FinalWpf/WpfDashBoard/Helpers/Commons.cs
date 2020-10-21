using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace WpfDashBoard.Helpers
{
    public class Commons
    {
        public static int Water_Count { get; set; }
        public static string BROKERHOST { get; set; }
        public static string PUB_TOPIC { get; set; } //home/device/data
        public static MqttClient BROKERCLIENT { get; set; }
        public static MqttClient BROKERCLIENT2 { get; set; }
        public static string CONNSTRING { get; set; }
        public static bool ISCONNECT { get; set; }
        public static string Carphoto_Entrance { get; set; }

        public static int CARLOGCOUNT { get; set; }                // CarLogCount저장
        public static int THLOGCOUNT { get; set; }                 //Temp,Humid저장
        public static int AlarmLogCOUNT { get; set; }
        public static int TEMPHUMIDCOUNT { get; set; }

        public static DateTime OPENEDTIME { get; set; } //처음 프로그램을 시작한 시간을 저장
        public static double TOTALTIME { get; set; }          //시작부터 종료까지의 시간 저장
        public static string WEATHER { get; set; }             //날씨정보 저장
        public static int FLAG { get; set; }

        public static string AlarmWater { get; set; }
        public static string AlramStartTime { get; set; }
        public static string AlramEndTime { get; set; }
        public static string AlarmFanStartTime { get; set; }
        public static string AlarmFanEndTime { get; set; }

        public static string AlarmFirstTemp { get; set; }
        public static string AlarmSecondTemp { get; set; }
        public static string AlarmThirdTemp { get; set; }


        internal static readonly string CONSTRING =
            "Data source=210.119.12.66;Port=3306;Database=final_data;Uid=root;Password=mysql_p@ssw0rd;";

        #region ###################사원관리테이블######################
        public class EmployeesTbl
        {
            public static string SELECT_EMPLOYEE = "SELECT Num,  " +
                                                                                      "              MemName,    " +
                                                                                      "              CarModel,      " +
                                                                                      "              CarNumber,   " +
                                                                                      "              Telephone,     " +
                                                                                      "              Entered,         " +
                                                                                      "              EnteredDate  " +
                                                                                      " FROM member ";

            public static string UPDATE_EMPLOYEE = "UPDATE member " +
                                                " SET                                                            " +
                                                "               MemName = @MemName,      " +
                                                "               CarModel = @CarModel,          " +
                                                "               CarNumber = @CarNumber,   " +
                                                "               Telephone = @Telephone        " + 
                                                " WHERE Num = @Num                            ";

            public static string INSERT_EMPLOYEE = "INSERT INTO member " +
                                                                                   " (                                    " +
                                                                                   "    MemName,              " +
                                                                                   "    CarModel,                " +
                                                                                   "    CarNumber,             " +
                                                                                   "    Telephone                " +
                                                                                   " )                                   " +
                                                                                   " VALUES                       " +
                                                                                   " (                                   " +
                                                                                   "    @MemName,         " +
                                                                                   "    @CarModel,           " +
                                                                                   "    @CarNumber,        " +
                                                                                   "    @Telephone           " +
                                                                                   " )                                  ";

            public static string DELETE_EMPLOYEE = "DELETE FROM member " +
                                                                                    " WHERE Num = @Num       ";
        }

        #endregion

        #region ###################온습도 테이블###################
        public  class THTbl
        {
            public static string INSERT_TH = " INSERT INTO tempandhumid " +
                                    " ( " +
                                    " Curr_Time, " +
                                    " FirstFloorTemp, " +
                                    " FirstFloorHumid, " +
                                    " SecondFloorTemp, " +
                                    " SecondFloorHumid, " +
                                    " ThirdFloorTemp, " +
                                    " ThirdFloorHumid) " +
                                    " VALUES " +
                                    " ( " +
                                    " @Curr_Time, " +
                                    " @FirstFloorTemp, " +
                                    " @FirstFloorHumid, " +
                                    " @SecondFloorTemp, " +
                                    " @SecondFloorHumid, " +
                                    " @ThirdFloorTemp, " +
                                    " @ThirdFloorHumid) ";

            public static string SELECT_TH = "SELECT Id," +
                                                            " Curr_Time, " +
                                                            "  FirstFloorTemp, " +
                                                            "  FirstFloorHumid, " +
                                                            "  SecondFloorTemp, " +
                                                            "  SecondFloorHumid, " +
                                                            "  ThirdFloorTemp, " +
                                                            "  ThirdFloorHumid " +
                                             " FROM final_data.tempandhumid " +
                                             " ORDER BY Id DESC ";
            public static string SELECT_Telephone = "SELECT Telephone FROM member WHERE Entered = '입차'";

            public static string COUNT_TH = "SELECT COUNT(*) FROM tempandhumid";

            public static string Alarm_TH = "SELECT Num," +
                                                            " Curr_Time, " +
                                                            "  1F_tempWarning, " +
                                                            "  2F_tempWarning, " +
                                                            "  3F_tempWarning, " +
                                                            "  WaterWarning, " +
                                                            "  FireWarning, " +
                                                            "  DetectionWarning " +
                                             " FROM final_data.critical " +
                                             " ORDER BY Num DESC ";
        }
        #endregion

        #region ###################주차테이블###################
        public class CarTbl
        {
            public static string DASH_SELECT_CAR = " SELECT Num, " +
                                               " CarNumber,    " +
                                               " EnteredDate   " +
                                               " FROM membercar       " +
                                               " ORDER BY Num DESC ";
            public static string ALARM_SELECT_CAR = " SELECT Num, " +
                                                 " CarNumber,    " +
                                                 " EnteredDate,   " +
                                                 " registered " +
                                                 " FROM membercar       " +
                                                 " ORDER BY Num DESC ";
            public static string COUNT_CAR = "SELECT COUNT(*) FROM membercar";
            public static string INSERT_CAR = "INSERT INTO membercar         " +
                                                          "                      (CarNumber,       " +
                                                          "                       EnteredDate)     " +
                                                          " VALUES                                     " +
                                                          "                     (@CarNumber,    " +
                                                          "                       @EnteredDate) ";
        }
        #endregion
    }
}
