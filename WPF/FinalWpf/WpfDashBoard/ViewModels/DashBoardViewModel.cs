using Caliburn.Micro;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using WpfDashBoard.Helpers;
using System.Windows.Forms;
using WpfDashBoard.Models;
using System.Security.Cryptography.X509Certificates;
using MjpegProcessor;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Net;
using System.Xml;
using Org.BouncyCastle.X509;
using CoolSms;

namespace WpfDashBoard.ViewModels
{

    class DashBoardViewModel : Conductor<object> 
    {
        SmsApi api = new SmsApi(new SmsApiOptions
        {
            ApiKey = "NCSUVQXRT7H2CI7D",
            ApiSecret = "HCXLBMGZU5US6QY8MSD1IDXCFO05NN8S",
            DefaultSenderId = "010-9295-6608" // 문자 보내는 사람 번호, coolsms 홈페이지에서 발신자 등록한 번호 필수
        });


        #region #############변수 부분#############
        private int flag_entered_DashBoard = 0;
        private int flag_alarm = 0;
        private int flag_ShowTempHumid = 0;

        private int IsChanged = 0;
        private int IsChanged_alarm = 0;
        private int IsChanged_ShowTempHumid = 0;

        private int cnt_sub = 0;
        private int cnt_sub_ShowTempHumid = 0;

        private int HumidTemplogcount;



        public string _mjpeg_url_3F = "http://210.119.12.80:81/?action=stream";
        public string _mjpeg_url_2F = "http://210.119.12.80:80/?action=stream";
        MjpegDecoder _mjpeg_3F;
        MjpegDecoder _mjpeg_2F;

        Timer Timer = new Timer();
        private double firstFloorTemp;
        private double firstFloorHumid;
        private double secondFloorTemp;
        private double secondFloorHumid;
        private double thirdFloorTemp;
        private double thirdFloorHumid;
        private string dashBoardTime;
        private string dashBoardDate;

        public string entrance_Photo;
        public string enteringinfo;

        private string weather_Photo;
        private string CategoryTemp;
        private float outTemp;
        private float outHum;
        private string dashBoard_Alarm;

        bool FirstLedOn = true;
        bool SecondLedOn = true;
        bool ThirdLedOn = true;

        bool Third_Temp_Humid_Change=true;
        bool Water_Change=true;
        bool Fire_Change=true;
        bool Detection_Change = true;
        #endregion

        #region #############생성자 부분#############
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
        public string DashBoardTime
        {
            get => dashBoardTime;
            set
            {
                dashBoardTime = value;
                NotifyOfPropertyChange(() => DashBoardTime);
            }
        }
        public string DashBoardDate
        {
            get => dashBoardDate;
            set
            {
                dashBoardDate = value;
                NotifyOfPropertyChange(() => DashBoardDate);
            }
        }
        public string Entrance_Photo
        {
            get => entrance_Photo;
            set
            {
                entrance_Photo = value;
                NotifyOfPropertyChange(() => Entrance_Photo);
            }
        }
        public string Enteringinfo
        {
            get => enteringinfo;
            set
            {
                enteringinfo = value;
                NotifyOfPropertyChange(() => Enteringinfo);
            }
        }

        public string Weather_Photo
        {
            get => weather_Photo;
            set
            {
                weather_Photo = value;
                NotifyOfPropertyChange(() => Weather_Photo);
            }
        }

        public float OutTemp
        {
            get => outTemp;
            set
            {
                outTemp = value;
                NotifyOfPropertyChange(() => OutTemp);
            }
        }

        public float OutHum
        {
            get => outHum;
            set
            {
                outHum = value;
                NotifyOfPropertyChange(() => OutHum);
            }
        }

        public string DashBoard_Alarm
        {
            get => dashBoard_Alarm;
            set
            {
                dashBoard_Alarm = value;
                NotifyOfPropertyChange(() => DashBoard_Alarm);
            }
        }
        public int alarm_cnt;
        public int Alarm_cnt
        {
            get => alarm_cnt;
            set
            {
                alarm_cnt = value;
                NotifyOfPropertyChange(() => Alarm_cnt);
            }
        }

        BitmapImage imageDisplay_3F;
        public BitmapImage ImageDisplay_3F
        {
            get => imageDisplay_3F;
            set
            {
                imageDisplay_3F = value;
                NotifyOfPropertyChange(() => ImageDisplay_3F);
            }
        }

        public BitmapImage imageDisplay_2F;
        public BitmapImage ImageDisplay_2F
        {
            get => imageDisplay_2F;
            set
            {
                imageDisplay_2F = value;
                NotifyOfPropertyChange(() => ImageDisplay_2F);
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
        public DashBoardViewModel()
        {

            _mjpeg_3F = new MjpegDecoder();
            _mjpeg_2F = new MjpegDecoder();
            _mjpeg_3F.FrameReady += mjpeg_FrameReady_3F;
            _mjpeg_2F.FrameReady += mjpeg_FrameReady_2F;
            //======================================
            Commons.Water_Count = 0;
            TimerLog();
            BasedDateTime = Commons.OPENEDTIME;
            OutTemHum();
            
            InitMqtt();
        }

        private void InitMqtt()
        {
            try
            {
                string Topic = "Temp_Humid";
                Commons.BROKERCLIENT = new MqttClient("210.119.12.77");

                if (Commons.BROKERCLIENT.IsConnected != true)
                {
                    Commons.BROKERCLIENT.MqttMsgPublishReceived += BROKERCLIENT_MqttMsgPublishReceived;
                    Commons.BROKERCLIENT.Connect("MqttMonitor");
                    Commons.BROKERCLIENT.Subscribe(new string[] { Topic },
                        new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                }
            }
            catch (Exception ex) { }
            
        }

        #region Today부분
        // 프로그램이 켜진시간 Based 
        private DateTime basedDateTime;
        public DateTime BasedDateTime
        {
            get => basedDateTime;
            set
            {
                basedDateTime = value;
                NotifyOfPropertyChange(() => BasedDateTime);
            }
        }

        ////Time DB
        //BindableCollection<TimeModel> time;
        //public BindableCollection<TimeModel> Time
        //{
        //    get => time;
        //    set
        //    {
        //        time = value;
        //        NotifyOfPropertyChange(() => Time);
        //    }
        //}
        #endregion

        #endregion

        #region #############사용자 함수 부분#############


        private void BROKERCLIENT_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
            var currDatas = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            //FirstFloorTemp = double.Parse(currDatas["firsttemp"]);
            //FirstFloorHumid = double.Parse(currDatas["firsthumid"]);
            //SecondFloorTemp = double.Parse(currDatas["secondtemp"]);
            //SecondFloorHumid = double.Parse(currDatas["secondhumid"]);
            //ThirdFloorTemp = double.Parse(currDatas["thirdtemp"]);
            //ThirdFloorHumid = double.Parse(currDatas["thirdhumid"]);

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
        private void TimerLog()
        {
            Timer.Interval = 1000;
            Timer.Tick += Timer_Tick1; //1초마다 발생
            Timer.Start();
        }

        private void Timer_Tick1(object sender, EventArgs e)
        {
            InitMqtt();
            DashBoardTime = DateTime.Now.ToString("HH:mm:ss");
            DashBoardDate = DateTime.Now.ToString("yyyy년 M월 d일 dddd");
            Entrance();
            Alarmfunc();
            ShowTempHumid();

            //_mjpeg_3F = new MjpegDecoder();
            //_mjpeg_2F = new MjpegDecoder();
            _mjpeg_3F.FrameReady += mjpeg_FrameReady_3F;
            _mjpeg_2F.FrameReady += mjpeg_FrameReady_2F;
            _mjpeg_3F.ParseStream(new Uri(_mjpeg_url_3F));
            _mjpeg_2F.ParseStream(new Uri(_mjpeg_url_2F));

            using (MySqlConnection conn = new MySqlConnection(Commons.CONSTRING))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(Commons.THTbl.COUNT_TH, conn);
                HumidTemplogcount = int.Parse(cmd.ExecuteScalar().ToString());
            }
        }

        private void mjpeg_FrameReady_3F(object sender, FrameReadyEventArgs e)
        {
            ImageDisplay_3F = e.BitmapImage;
        }
        private void mjpeg_FrameReady_2F(object sender, FrameReadyEventArgs e)
        {
            ImageDisplay_2F = e.BitmapImage;
        }

        //private void ImageDisplay_Loaded(object sender, RoutedEventArgs e)
        //{
        //    _mjpeg_url_3F = "http://210.119.12.80:81/?action=stream";
        //    _mjpeg_url_2F = "http://210.119.12.80:80/?action=stream";
        //    _mjpeg_3F = new MjpegDecoder();
        //    _mjpeg_2F = new MjpegDecoder();
        //    _mjpeg_3F.FrameReady += mjpeg_FrameReady_3F;
        //    _mjpeg_2F.FrameReady += mjpeg_FrameReady_2F;
        //    _mjpeg_3F.ParseStream(new Uri(_mjpeg_url_3F));
        //    _mjpeg_2F.ParseStream(new Uri(_mjpeg_url_2F));
        //}

        public void Entrance()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.CONSTRING))       //DB접속
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(Commons.CarTbl.DASH_SELECT_CAR, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    Cars = new BindableCollection<CarModel>();
                    while (reader.Read())
                    {
                        var temp = new CarModel
                        {
                            Num = reader["Num"].ToString(),
                            CarNumber = reader["CarNumber"].ToString(),
                            EnteredDate = reader["EnteredDate"].ToString(),
                    };
                        Cars.Add(temp);

                        //1F에 사진 띄우기
                        if (flag_entered_DashBoard == 0 || IsChanged < Int32.Parse(temp.Num))
                        {
                            IsChanged = Int32.Parse(temp.Num);
                            Enteringinfo = temp.EnteredDate + " " + temp.CarNumber;
                            Entrance_Photo = System.IO.Directory.GetCurrentDirectory()
                                        + "/../../" + "media/"
                                        + temp.EnteredDate
                                        + " "
                                        + temp.CarNumber
                                        + ".jpg";
                            flag_entered_DashBoard++;
                        }
                    }
                }
                switch (char.Parse(Commons.WEATHER)) 
                {
                    case '1'://맑음
                        Weather_Photo = "/media/free-icon-sun-789395.png";
                        break;
                    case '3'://구름많음
                        Weather_Photo = "/media/free-icon-cloudy-3508964.png";
                        break;
                    case '4'://흐림
                        Weather_Photo = "/media/free-icon-rainy-3506913.png";
                        break;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }


        public void Alarmfunc()
        {
            try
            {
                using (MySqlConnection conn2 = new MySqlConnection(Commons.CONSTRING))       //DB접속
                {
                    conn2.Open();
                    MySqlCommand cmd_alarm = new MySqlCommand(Commons.THTbl.Alarm_TH, conn2);
                    MySqlDataReader reader_alarm = cmd_alarm.ExecuteReader();
                    Alarm = new BindableCollection<AlarmModel>();
                    while (reader_alarm.Read())
                    {
                        var temp_alarm = new AlarmModel
                        {
                            Num = reader_alarm["Num"].ToString(),
                            Curr_Time = reader_alarm["Curr_Time"].ToString(),
                            tempWarning_1F = reader_alarm["1F_tempWarning"].ToString(),
                            tempWarning_2F = reader_alarm["2F_tempWarning"].ToString(),
                            tempWarning_3F = reader_alarm["3F_tempWarning"].ToString(),
                            WaterWarning = reader_alarm["WaterWarning"].ToString(),
                            FireWarning = reader_alarm["FireWarning"].ToString(),
                            DetectionWarning = reader_alarm["DetectionWarning"].ToString(),
                        };
                        Alarm.Add(temp_alarm);


                        if (IsChanged_alarm < Int32.Parse(temp_alarm.Num))
                        {
                            IsChanged_alarm = Int32.Parse(temp_alarm.Num);
                            flag_alarm++;

                            if(flag_alarm>=2)
                            {
                                if (temp_alarm.tempWarning_1F == "1")
                                    DashBoard_Alarm += "1층의 온도가 높습니다.\r\n";
                                if (temp_alarm.tempWarning_2F == "1")
                                    DashBoard_Alarm += "2층의 온도가 높습니다.\r\n";
                                if (temp_alarm.tempWarning_3F == "1")
                                {
                                    if(Third_Temp_Humid_Change==true)
                                    {
                                        Commons.BROKERCLIENT.Publish("SmartBuilding/Fan/", Encoding.Default.GetBytes("Fan_On"));
                                        DashBoard_Alarm += "3층의 온도가 높습니다.\r에어컨이 작동됩니다.\n";
                                        cnt_sub++;
                                        Third_Temp_Humid_Change = false;
                                    }
                                }
                                else if(temp_alarm.tempWarning_3F == "0")
                                {
                                    if(Third_Temp_Humid_Change==false)
                                    {
                                        Commons.BROKERCLIENT.Publish("SmartBuilding/Fan/", Encoding.Default.GetBytes("Fan_Off"));
                                        Third_Temp_Humid_Change = true;
                                    }

                                }

                                if (temp_alarm.WaterWarning == "1")
                                {
                                    if(Water_Change==true)
                                    {
                                        Commons.BROKERCLIENT.Publish("SmartBuilding/Water/", Encoding.Default.GetBytes("Water_On"));
                                        DashBoard_Alarm += "주차장의 수위가 너무 높습니다.\r\n";
                                        cnt_sub++;
                                        Commons.Water_Count++;
                                        Water_Change = false;

                                        if (Commons.Water_Count == 1)
                                        {
                                            using (MySqlConnection conn4 = new MySqlConnection(Commons.CONSTRING))       //DB접속
                                            {
                                                conn4.Open();
                                                MySqlCommand cmd_Mes = new MySqlCommand(Commons.THTbl.SELECT_Telephone, conn4);
                                                MySqlDataReader reader_Mes = cmd_Mes.ExecuteReader();
                                                TempHumid = new BindableCollection<TempHumidModel>();
                                                while (reader_Mes.Read())
                                                {
                                                    var Number = reader_Mes["Telephone"].ToString();
                                                    var result = api.SendMessageAsync(Number, "[긴급] 주차장 침수 발생!!\n주차한 차량을 안전한 장소로 이동하여 주시기 바랍니다.\n-부경대 IOT 3조-");
                                                }
                                            }
                                        }
                                    }

                                }

                                else if(temp_alarm.WaterWarning == "0")
                                {
                                    if(Water_Change==false)
                                    {
                                        Commons.BROKERCLIENT.Publish("SmartBuilding/Water/", Encoding.Default.GetBytes("Water_Off"));
                                        Water_Change = true;
                                    }
                                }

                                if (temp_alarm.FireWarning == "1")
                                {
                                    if(Fire_Change==true)
                                    {
                                        Commons.BROKERCLIENT.Publish("SmartBuilding/Fire/", Encoding.Default.GetBytes("Fire_On"));
                                        DashBoard_Alarm += "3층에서 화재가 발생했습니다.\r\n";
                                        cnt_sub++;
                                        Fire_Change = false;
                                    }

                                }
                                else if(temp_alarm.FireWarning == "0")
                                {
                                    if(Fire_Change==false)
                                    {
                                        Commons.BROKERCLIENT.Publish("SmartBuilding/Fire/", Encoding.Default.GetBytes("Fire_Off"));
                                        Fire_Change = true;
                                    }
                                }

                                if (temp_alarm.DetectionWarning == "1")
                                {
                                    if(Detection_Change==true)
                                    {
                                        Commons.BROKERCLIENT.Publish("SmartBuilding/Detection/", Encoding.Default.GetBytes("Detection_On"));
                                        DashBoard_Alarm += "침입이 감지되었습니다.\r\n";
                                        cnt_sub++;
                                        Detection_Change = false;
                                    }

                                }
                                else if(temp_alarm.DetectionWarning == "0")
                                {
                                    if(Detection_Change==false)
                                    {
                                        Commons.BROKERCLIENT.Publish("SmartBuilding/Detection/", Encoding.Default.GetBytes("Detection_Off"));
                                        Detection_Change = true;
                                    }
                                }
                                if (cnt_sub >= 1) { Alarm_cnt++; cnt_sub = 0; }
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        

        public void ShowTempHumid()
        {
            try
            {
                using (MySqlConnection conn3 = new MySqlConnection(Commons.CONSTRING))       //DB접속
                {
                    conn3.Open();
                    MySqlCommand cmd_ShowTempHumid = new MySqlCommand(Commons.THTbl.SELECT_TH, conn3);
                    MySqlDataReader reader_ShowTempHumid = cmd_ShowTempHumid.ExecuteReader();
                    TempHumid = new BindableCollection<TempHumidModel>();
                    while (reader_ShowTempHumid.Read())
                    {
                        var temp_ShowTempHumid = new TempHumidModel
                        {
                            Id = reader_ShowTempHumid["Id"].ToString(),
                            Curr_Time = reader_ShowTempHumid["Curr_Time"].ToString(),
                            FirstFloorTemp = reader_ShowTempHumid["FirstFloorTemp"].ToString(),
                            FirstFloorHumid = reader_ShowTempHumid["FirstFloorHumid"].ToString(),
                            SecondFloorTemp = reader_ShowTempHumid["SecondFloorTemp"].ToString(),
                            SecondFloorHumid = reader_ShowTempHumid["SecondFloorHumid"].ToString(),
                            ThirdFloorTemp = reader_ShowTempHumid["ThirdFloorTemp"].ToString(),
                            ThirdFloorHumid = reader_ShowTempHumid["ThirdFloorHumid"].ToString(),
                        };
                        TempHumid.Add(temp_ShowTempHumid);

                        if (flag_ShowTempHumid == 0 || IsChanged_ShowTempHumid < Int32.Parse(temp_ShowTempHumid.Id))
                        {
                            flag_ShowTempHumid++;
                            cnt_sub_ShowTempHumid++;
                            IsChanged_ShowTempHumid = Int32.Parse(temp_ShowTempHumid.Id);

                            if (flag_ShowTempHumid >= 2 || cnt_sub_ShowTempHumid >= 2)
                            {
                                FirstFloorTemp = Convert.ToDouble(temp_ShowTempHumid.FirstFloorTemp);
                                FirstFloorHumid = Convert.ToDouble(temp_ShowTempHumid.FirstFloorHumid);
                                SecondFloorTemp = Convert.ToDouble(temp_ShowTempHumid.SecondFloorTemp);
                                SecondFloorHumid = Convert.ToDouble(temp_ShowTempHumid.SecondFloorHumid);
                                ThirdFloorTemp = Convert.ToDouble(temp_ShowTempHumid.ThirdFloorTemp);
                                ThirdFloorHumid = Convert.ToDouble(temp_ShowTempHumid.ThirdFloorHumid);
                                cnt_sub_ShowTempHumid = 0;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }


        //바깥 온습도 보여주는 함수
        private void OutTemHum()
        {
            WebClient wc = null;
            XmlDocument doc = null;

            wc = new WebClient() { Encoding = Encoding.UTF8 };
            doc = new XmlDocument();
            StringBuilder str = new StringBuilder();
            str.Append("http://apis.data.go.kr/1360000/VilageFcstInfoService/getUltraSrtNcst");
            str.Append("?serviceKey=LuZTEEVveo8mSqWLLl8umhdbZvlAu0xSQKBsxd09An5GRvQeu1UJFZ%2BdiLIQAD6%2BN5ZKXEg62x2o2yZE4WsKWQ%3D%3D");
            str.Append("&pageNo=1");
            str.Append("&numOfRows=200");
            str.Append("&dataType=XML");
            str.Append("&base_date=" + DateTime.Now.ToString("yyyyMMdd"));
            str.Append("&base_time=0800");
            str.Append("&nx=97");
            str.Append("&ny=75");


            string xml = wc.DownloadString(str.ToString());
            doc.LoadXml(xml);
            XmlElement root = doc.DocumentElement;
            XmlNodeList items = doc.GetElementsByTagName("item");

            foreach (XmlNode item in items)
            {
                CategoryTemp = item["category"].InnerText;
                if (CategoryTemp == "T1H")
                {
                    OutTemp = float.Parse(item["obsrValue"].InnerText);
                }
                if (CategoryTemp == "REH")
                {
                    OutHum = float.Parse(item["obsrValue"].InnerText);

                }
            }
        }

        #region LED부분
        public void FirstLed()
        {
            if (!FirstLedOn)
            {
                Commons.BROKERCLIENT.Publish("SmartBuilding/Led/", Encoding.Default.GetBytes("Floor1_On"));
                FirstLedOn = true;
            }
            else
            {
                Commons.BROKERCLIENT.Publish("SmartBuilding/Led/", Encoding.Default.GetBytes("Floor1_Off"));
                FirstLedOn = false;
            }

        }
        public void SecondLed()
        {
            if (!SecondLedOn)
            {
                Commons.BROKERCLIENT.Publish("SmartBuilding/Led/", Encoding.Default.GetBytes("Floor2_On"));
                SecondLedOn = true;
            }
            else
            {
                Commons.BROKERCLIENT.Publish("SmartBuilding/Led/", Encoding.Default.GetBytes("Floor2_Off"));
                SecondLedOn = false;
            }

        }
        public void ThirdLed()
        {
            if (!ThirdLedOn)
            {
                Commons.BROKERCLIENT.Publish("SmartBuilding/Led/", Encoding.Default.GetBytes("Floor3_On"));
                ThirdLedOn = true;
            }
            else
            {
                Commons.BROKERCLIENT.Publish("SmartBuilding/Led/", Encoding.Default.GetBytes("Floor3_Off"));
                ThirdLedOn = false;
            }

        }
        #endregion led부분끝

        #endregion
    }
}

