using Caliburn.Micro;
using MySql.Data.MySqlClient;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfDashBoard.Helpers;

namespace WpfDashBoard.ViewModels
{
    class Space2FViewModel : Conductor<object>
    {
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

        private string alarmStartTime;

        public string AlarmStartTime
        {
            get => alarmStartTime;
            set
            {
                alarmStartTime = value;
                NotifyOfPropertyChange(() => AlarmStartTime);
            }
        }
        private string alarmEndTime;

        public string AlarmEndTime
        {
            get => alarmEndTime;
            set
            {
                alarmEndTime = value;
                NotifyOfPropertyChange(() => AlarmEndTime);
            }
        }
        private string alarmSecondTemp;
        public string AlarmSecondTemp
        {
            get => alarmSecondTemp;
            set
            {
                alarmSecondTemp = value;
                NotifyOfPropertyChange(() => AlarmSecondTemp);
            }
        }

        public Space2FViewModel()
        {
            InitDataFromDB();
            SetUp();
            AlarmSecondTemp = Commons.AlarmSecondTemp;
        }

        private void SetUp()
        {
            AlarmStartTime = Commons.AlramStartTime;
            AlarmEndTime = Commons.AlramEndTime;
            AlarmSecondTemp = Commons.AlarmSecondTemp;
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
                string strSelQuery = "Select SecondFloorTemp,SecondFloorHumid" +
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
                        listTemps.Add(new DataPoint(i, Convert.ToDouble(reader["SecondFloorTemp"])));
                        listHumids.Add(new DataPoint(i, Convert.ToDouble(reader["SecondFloorHumid"])));
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
        public void Setup()
        {
            Commons.AlarmSecondTemp = AlarmSecondTemp;
            Commons.AlramStartTime = AlarmStartTime;
            Commons.AlramEndTime = AlarmEndTime;
            Commons.BROKERCLIENT.Publish("2F_AlramStartTimeconfig", Encoding.Default.GetBytes(AlarmStartTime));
            Commons.BROKERCLIENT.Publish("2F_AlramEndTimeconfig", Encoding.Default.GetBytes(AlarmEndTime));
            Commons.BROKERCLIENT.Publish("2F_tempconfig", Encoding.Default.GetBytes(AlarmSecondTemp));

        }
    }
}
