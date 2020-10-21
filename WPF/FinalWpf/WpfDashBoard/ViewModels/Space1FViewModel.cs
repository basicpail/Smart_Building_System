using Caliburn.Micro;
using MySql.Data.MySqlClient;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using WpfDashBoard.Helpers;

namespace WpfDashBoard.ViewModels
{
    public class Space1FViewModel : Conductor<object>
    {
        public Space1FViewModel()
        {
            InitDataFromDB();

            AlarmWater = Commons.AlarmWater;
            AlarmFirstTemp = Commons.AlarmFirstTemp;
            Commons.BROKERCLIENT = new MqttClient("210.119.12.77");
            try
            {
                if (Commons.BROKERCLIENT.IsConnected != true)
                {
                    Commons.BROKERCLIENT.Connect("MqttMonitor");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private string startDate;
        public string StartDate
        {
            get => startDate;
            set
            {
                startDate = DateTime.Parse(value).ToString("yyyy-MM-dd");
                EndDate = DateTime.Parse(startDate).AddDays(1).ToString("yyyy-MM-dd");
                NotifyOfPropertyChange(() => StartDate);
                NotifyOfPropertyChange(() => EndDate);
            }
        }

        private string endDate;
        public string EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                NotifyOfPropertyChange(() => EndDate);
            }
        }

        private IList<DataPoint> tempValues;
        public IList<DataPoint> TempValues
        {
            get => tempValues;
            set
            {
                tempValues = value;
                NotifyOfPropertyChange(() => TempValues);
            }
        }
        private IList<DataPoint> humidValues;

        public IList<DataPoint> HumidValues
        {
            get => humidValues;
            set
            {
                humidValues = value;
                NotifyOfPropertyChange(() => HumidValues);
            }
        }

        private string alarmWater;
        public string AlarmWater 
        {
            get => alarmWater;
            set
            {
                alarmWater = value;
                NotifyOfPropertyChange(() => AlarmWater);
            }
        }

        private string alarmFirstTemp;
        public string AlarmFirstTemp
        {
            get => alarmFirstTemp;
            set
            {
                alarmFirstTemp = value;
                NotifyOfPropertyChange(() => AlarmFirstTemp);
            }
        }



        private void InitDataFromDB()
        {

            using (var conn = new MySqlConnection(Commons.CONSTRING))
            {
                string strSelQuery = "SELECT date_format(Curr_Time, '%Y-%m-%d') AS StartDate " +
                                     "  FROM tempandhumid " +
                                     " WHERE Id = 1 ";
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(strSelQuery, conn);
                    string result = cmd.ExecuteScalar().ToString();

                    StartDate = result;
                    //EndDate = result;
                    EndDate = DateTime.Parse(result).AddDays(1).ToString("yyyy-MM-dd");
                }
                catch (Exception ex)
                {
                }
            }
        }
        public void Search()
        {

            TempValues = HumidValues = new List<DataPoint>();
            List<DataPoint> listTemps = new List<DataPoint>();
            List<DataPoint> listHumids = new List<DataPoint>();
            using (var conn = new MySqlConnection(Commons.CONSTRING))
            {
                string strSelQuery = "Select FirstFloorTemp,FirstFloorHumid" +
                                     "  From tempandhumid " +
                                     " where " +
                                     " Curr_Time between @StartDate And @EndDate " +
                                     " order by Id ";
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(strSelQuery, conn);

                    MySqlParameter paramStartDate = new MySqlParameter("@StartDate", MySqlDbType.VarChar);
                    paramStartDate.Value = StartDate;
                    cmd.Parameters.Add(paramStartDate);
                    MySqlParameter paramEndDate = new MySqlParameter("@EndDate", MySqlDbType.VarChar);
                    paramEndDate.Value = EndDate;
                    cmd.Parameters.Add(paramEndDate);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    var i = 0;
                    while (reader.Read())
                    {
                        listTemps.Add(new DataPoint(i, Convert.ToDouble(reader["FirstFloorTemp"])));
                        listHumids.Add(new DataPoint(i, Convert.ToDouble(reader["FirstFloorHumid"])));
                        i++;
                    }
                    if (i == 0)
                    {
                        var wManager = new WindowManager();
                        //wManager.ShowDialog(new DialogViewModel($"Error|데이터가 없습니다."));
                        return;
                    }
                    //TotalCount = i;
                }
                catch (Exception ex)
                {
                    var wManager = new WindowManager();
                    //wManager.ShowDialog(new ErrorPopupViewModel($"Error|{ex.Message}"));
                }
                TempValues = listTemps;
                HumidValues = listHumids;
            }
        }
        public void SetUp()
        {
            Commons.AlarmWater = AlarmWater;
            Commons.AlarmFirstTemp = AlarmFirstTemp;
            //Commons.DashBoard_Alarm += "1층의 알람온도가" + AlarmFirstTemp + "로 설정 되었습니다.\r\n";
            Commons.BROKERCLIENT.Publish("Waterconfig", Encoding.Default.GetBytes(AlarmWater));
            Commons.BROKERCLIENT.Publish("1F_tempconfig", Encoding.Default.GetBytes(AlarmFirstTemp));
            //Commons.BROKERCLIENT.Publish("gate", Encoding.UTF8.GetBytes("GateOpen"));

        }
    }
}
