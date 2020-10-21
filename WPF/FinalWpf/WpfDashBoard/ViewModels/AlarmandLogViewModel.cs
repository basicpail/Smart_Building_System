using Caliburn.Micro;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using WpfDashBoard.Helpers;
using WpfDashBoard.Models;


namespace WpfDashBoard.ViewModels
{

    public class AlarmandLogViewModel : Conductor<object>
    {
        #region ####################변수부분####################

        private string num;
        private string carNumber;
        private string enteredDate;
        private int carlogcount;
        private int thlogcount;
        private int flag_entered_Alarm = 0;
        private int IsChanged;

        private double firstFloorTemp;
        private double firstFloorHumid;
        private double secondFloorTemp;
        private double secondFloorHumid;
        private double thirdFloorTemp;
        private double thirdFloorHumid;

        Timer Timer = new Timer();

        #endregion

        #region ####################생성자부분####################
        public AlarmandLogViewModel()
        {
            CarLog();
            ThLog();
            AlarmLog();
            string Topic = "home/device/Car/";
            Commons.BROKERCLIENT = new MqttClient("210.119.12.77");
            string Topic2 = "Temp_Humid";
            Commons.BROKERCLIENT2 = new MqttClient("210.119.12.77");

            try
            {
                if (Commons.BROKERCLIENT.IsConnected != true)
                {
                    Commons.BROKERCLIENT.MqttMsgPublishReceived += BROKERCLIENT_MqttMsgPublishReceived;
                    Commons.BROKERCLIENT.Connect("MqttMonitor");
                    Commons.BROKERCLIENT.Subscribe(new string[] { Topic },
                        new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

                    Commons.BROKERCLIENT2.MqttMsgPublishReceived += BROKERCLIENT_MqttMsgPublishReceived2;
                    Commons.BROKERCLIENT2.Connect("MqttMonitor");
                    Commons.BROKERCLIENT2.Subscribe(new string[] { Topic2 },
                        new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                }

                TimerLog();     //Timer 실행
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string Num
        {
            get => num;
            set
            {
                num = value;
                NotifyOfPropertyChange(() => Num);
            }
        }

        public string CarNumber
        {
            get => carNumber;
            set
            {
                carNumber = value;
                NotifyOfPropertyChange(() => CarNumber);
            }
        }

        public string EnteredDate
        {
            get => enteredDate;
            set
            {
                enteredDate = value;
                NotifyOfPropertyChange(() => EnteredDate);
            }
        }

        public string Registered;
        public string registered
        {
            get => Registered;
            set
            {
                Registered = value;
                NotifyOfPropertyChange(() => registered);
            }
        }
        public double FirstFloorTemp
        {
            get => firstFloorTemp;
            set
            {
                firstFloorTemp = value;
                NotifyOfPropertyChange(() => FirstFloorTemp);
            }
        }
        public double FirstFloorHumid
        {
            get => firstFloorHumid;
            set
            {
                firstFloorHumid = value;
                NotifyOfPropertyChange(() => FirstFloorHumid);
            }
        }
        public double SecondFloorTemp
        {
            get => secondFloorTemp;
            set
            {
                secondFloorTemp = value;
                NotifyOfPropertyChange(() => SecondFloorTemp);
            }
        }
        public double SecondFloorHumid
        {
            get => secondFloorHumid;
            set
            {
                secondFloorHumid = value;
                NotifyOfPropertyChange(() => SecondFloorHumid);
            }
        }
        public double ThirdFloorTemp
        {
            get => thirdFloorTemp;
            set
            {
                thirdFloorTemp = value;
                NotifyOfPropertyChange(() => ThirdFloorTemp);
            }
        }
        public double ThirdFloorHumid
        {
            get => thirdFloorHumid;
            set
            {
                thirdFloorHumid = value;
                NotifyOfPropertyChange(() => ThirdFloorHumid);
            }
        }
        public string carphoto;
        public string CarPhoto
        {
            get => carphoto;
            set
            {
                carphoto = value;
                NotifyOfPropertyChange(() => CarPhoto);
            }
        }

        //public string carphoto_Entrance;
        //public string Carphoto_Entrance
        //{
        //    get => carphoto_Entrance;
        //    set
        //    {
        //        NotifyOfPropertyChange(() => CarPhoto);
        //    }
        //}

        //public string openTheDoor;
        //public string OpenTheDoor
        //{
        //    get => openTheDoor;
        //    set
        //    {
        //        openTheDoor = value;
        //        Commons.BROKERCLIENT.Publish("gate", OpenTheDoor_Bytes);
        //        NotifyOfPropertyChange(() => OpenTheDoor);
        //    }
        //}

        //로그 선택
        CarModel selectedcar;
        public CarModel SelectedCar
        {
            get => selectedcar;
            set
            {
                selectedcar = value;
                if (value != null)
                {
                    CarPhoto = System.IO.Directory.GetCurrentDirectory()
                        + "/../../" + "media/"
                        + value.EnteredDate
                        + " "
                        + value.CarNumber
                        + ".jpg";
                }
                NotifyOfPropertyChange(() => SelectedCar);
            }
        }

        //온습도 리스트
        BindableCollection<TempHumidModel> tempHumid;
        public BindableCollection<TempHumidModel> TempHumid
        {
            get => tempHumid;
            set
            {
                tempHumid = value;
                NotifyOfPropertyChange(() => TempHumid);
            }
        }
        
        //전체 Car 리스트
        BindableCollection<CarModel> cars;
        public BindableCollection<CarModel> Cars
        {
            get => cars;
            set
            {
                cars = value;
                NotifyOfPropertyChange(() => Cars);
            }
        }
        //전체 Employees 리스트
        BindableCollection<EmployeesModel> employees_list;
        public BindableCollection<EmployeesModel> Employees_list
        {
            get => employees_list;
            set
            {
                employees_list = value;
                NotifyOfPropertyChange(() => Employees_list);
            }
        }

        BindableCollection<AlarmModel> alarm;
        public BindableCollection<AlarmModel> Alarm
        {
            get => alarm;
            set
            {
                alarm = value;
                NotifyOfPropertyChange(() => Alarm);
            }
        }

        //MQTT
        private void BROKERCLIENT_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
            var currDatas = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            CarNumber = string.Format(currDatas["Dev_Id"]);
            EnteredDate = string.Format(currDatas["Curr_Time"]);

            
            using (var conn = new MySqlConnection(Commons.CONSTRING))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(Commons.CarTbl.INSERT_CAR, conn);

                    MySqlParameter paramCarNumber = new MySqlParameter("@CarNumber", MySqlDbType.VarChar, 15);
                    paramCarNumber.Value = currDatas["Dev_Id"];
                    cmd.Parameters.Add(paramCarNumber);

                    MySqlParameter paramEnteredDate = new MySqlParameter("@EnteredDate", MySqlDbType.VarChar, 100);
                    paramEnteredDate.Value = currDatas["Curr_Time"];
                    cmd.Parameters.Add(paramEnteredDate);

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void BROKERCLIENT_MqttMsgPublishReceived2(object sender, MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
            var currDatas = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            FirstFloorTemp = double.Parse(currDatas["firsttemp"]);
            FirstFloorHumid = double.Parse(currDatas["firsthumid"]);
            SecondFloorTemp = double.Parse(currDatas["secondtemp"]);
            SecondFloorHumid = double.Parse(currDatas["secondhumid"]);
            ThirdFloorTemp = double.Parse(currDatas["thirdtemp"]);
            ThirdFloorHumid = double.Parse(currDatas["thirdhumid"]);

            using (var conn = new MySqlConnection(Commons.CONSTRING))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(Commons.THTbl.INSERT_TH, conn);

                    MySqlParameter paramCurrTime = new MySqlParameter("@Curr_Time", MySqlDbType.DateTime);
                    paramCurrTime.Value = DateTime.Parse(currDatas["Curr_time"]);
                    cmd.Parameters.Add(paramCurrTime);

                    MySqlParameter paramFirstFloorTemp = new MySqlParameter("@FirstFloorTemp", MySqlDbType.Float);
                    paramFirstFloorTemp.Value = currDatas["firsttemp"];
                    cmd.Parameters.Add(paramFirstFloorTemp);

                    MySqlParameter paramFirstFloorHumid = new MySqlParameter("@FirstFloorHumid", MySqlDbType.Float);
                    paramFirstFloorHumid.Value = currDatas["firsthumid"];
                    cmd.Parameters.Add(paramFirstFloorHumid);

                    MySqlParameter paramSecondFloorTemp = new MySqlParameter("@SecondFloorTemp", MySqlDbType.Float);
                    paramSecondFloorTemp.Value = currDatas["secondtemp"];
                    cmd.Parameters.Add(paramSecondFloorTemp);

                    MySqlParameter paramSecondFloorHumid = new MySqlParameter("@SecondFloorHumid", MySqlDbType.Float);
                    paramSecondFloorHumid.Value = currDatas["secondhumid"];
                    cmd.Parameters.Add(paramSecondFloorHumid);

                    MySqlParameter paramThirdFloorTemp = new MySqlParameter("@ThirdFloorTemp", MySqlDbType.Float);
                    paramThirdFloorTemp.Value = currDatas["thirdtemp"];
                    cmd.Parameters.Add(paramThirdFloorTemp);

                    MySqlParameter paramThirdFloorHumid = new MySqlParameter("@ThirdFloorHumid", MySqlDbType.Float);
                    paramThirdFloorHumid.Value = currDatas["thirdhumid"];
                    cmd.Parameters.Add(paramThirdFloorHumid);

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                }
            }

        }

        #endregion

        #region ####################사용자함수####################

        //타이머일정시간마다 돌려서 SELECT COUNT실행, COUNT값이 다르면 carLog실행해주기.
        private void TimerLog()
        {
            Timer.Interval = 1000;
            Timer.Tick += Timer_Tick1; //1000초마다 발생
            Timer.Start();
        }
        // Commons.CARLOGCOUNT
        private void Timer_Tick1(object sender, EventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(Commons.CONSTRING))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(Commons.CarTbl.COUNT_CAR, conn);
                carlogcount = int.Parse(cmd.ExecuteScalar().ToString());
            }
            using(MySqlConnection conn = new MySqlConnection(Commons.CONSTRING))
            {
                conn.Open();
                MySqlCommand cmd2 = new MySqlCommand(Commons.THTbl.COUNT_TH, conn);
                thlogcount = int.Parse(cmd2.ExecuteScalar().ToString());
            }
            if (Commons.CARLOGCOUNT < carlogcount)
            {
                CarLog();
            }
            if(Commons.THLOGCOUNT < thlogcount)
            {
                ThLog();
            }
            if (Commons.AlarmLogCOUNT < thlogcount)
            {
                AlarmLog();
            }
        }

        private void AlarmLog()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.CONSTRING))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(Commons.THTbl.Alarm_TH, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    Alarm = new BindableCollection<AlarmModel>();
                    while (reader.Read())
                    {
                        var temp = new AlarmModel
                        {
                            Num = reader["Num"].ToString(),
                            Curr_Time = reader["Curr_Time"].ToString(),
                            tempWarning_1F = reader["1F_tempWarning"].ToString(),
                            tempWarning_2F = reader["2F_tempWarning"].ToString(),
                            tempWarning_3F = reader["3F_tempWarning"].ToString(),
                            WaterWarning = reader["WaterWarning"].ToString(),
                            FireWarning = reader["FireWarning"].ToString(),
                            DetectionWarning = reader["DetectionWarning"].ToString(),
                        };
                        Alarm.Add(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ThLog()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.CONSTRING))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(Commons.THTbl.SELECT_TH, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    TempHumid = new BindableCollection<TempHumidModel>();
                    while (reader.Read())
                    {
                        var temp = new TempHumidModel
                        {
                            Id = reader["Id"].ToString(),
                            Curr_Time = reader["Curr_Time"].ToString(),
                            FirstFloorTemp = reader["FirstFloorTemp"].ToString(),
                            FirstFloorHumid = reader["FirstFloorHumid"].ToString(),
                            SecondFloorTemp = reader["SecondFloorTemp"].ToString(),
                            SecondFloorHumid = reader["SecondFloorHumid"].ToString(),
                            ThirdFloorTemp = reader["ThirdFloorTemp"].ToString(),
                            ThirdFloorHumid = reader["ThirdFloorHumid"].ToString(),
                        };
                        TempHumid.Add(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //CarLog띄우는 함수
        public void CarLog()
        {
           
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.CONSTRING))       //DB접속
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(Commons.CarTbl.ALARM_SELECT_CAR, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    Cars = new BindableCollection<CarModel>();

                    //MySqlConnection conn2 = new MySqlConnection(Commons.CONSTRING);
                    //conn2.Open();
                    //MySqlCommand cmd_mem_CarNumber = new MySqlCommand(mem_CarNumber, conn2);
                    //MySqlDataReader reader_mem_CarNumber = cmd_mem_CarNumber.ExecuteReader(); //에러해결
                    //Employees_list = new BindableCollection<EmployeesModel>();


                    while (reader.Read())
                    {
                        var temp = new CarModel
                        {
                            Num = reader["Num"].ToString(),
                            CarNumber = reader["CarNumber"].ToString(),
                            EnteredDate = reader["EnteredDate"].ToString(),
                            registered = reader["registered"].ToString(),
                        };
                        Cars.Add(temp);


                        //log창에 사진 띄우기
                        if (flag_entered_Alarm == 0 || IsChanged < Int32.Parse(temp.Num))
                        {
                            IsChanged = Int32.Parse(temp.Num);
                            CarPhoto = System.IO.Directory.GetCurrentDirectory()
                                        + "/../../" + "media/"
                                        + temp.EnteredDate
                                        + " "
                                        + temp.CarNumber
                                        + ".jpg";
                            //SELECT_CAR = mem_CarNumber;
                            //conn.Close();
                            //conn.Open();
                            //MySqlCommand cmd_mem_CarNumber = new MySqlCommand(mem_CarNumber, conn);
                            //MySqlDataReader reader_mem_CarNumber = cmd_mem_CarNumber.ExecuteReader(); //에러

                            //while (reader_mem_CarNumber.Read())
                            //{
                            //    var temp2 = new EmployeesModel
                            //    {
                            //        CarNumber = reader_mem_CarNumber["CarNumber"].ToString(),
                            //    };
                            //    Employees_list.Add(temp2);

                            //    if(temp2.CarNumber == temp.CarNumber)
                            //    {
                            //        Commons.BROKERCLIENT.Publish("gate", Encoding.UTF8.GetBytes("GateOpen"));
                            //        flag++;
                            //    }
                            //}
                            flag_entered_Alarm++;
                        }
                        //Entered();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        bool Barricade_on = true;

        public void Barricade()
        {
            if (!Barricade_on)
            {
                Barricade_on = true;
            }
            else
            {
                Commons.BROKERCLIENT.Publish("gate", Encoding.UTF8.GetBytes("GateOpen"));
                Barricade_on = false;
            }

        }

        #endregion
    }
}