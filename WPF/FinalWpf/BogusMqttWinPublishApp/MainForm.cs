using System.Text;
using System;
using System;
using System;
using System.Threading;
using Bogus;
using MetroFramework.Forms;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;

namespace BogusMqttWinPublishApp
{
    public partial class MainForm : MetroForm
    {
        public static string MqttBrokerUrl { get; private set; }

        public static MqttClient BrokerClient { get; set; }

        private static Thread MqttThread { get; set; }

        private static Faker<SensorInfo> SensorFaker { get; set; }

        private static string CurrValue { get; set; }
        public MainForm()
        {
            InitializeComponent();
            InitializeAll();
        }

        private void InitializeAll()
        {
            MqttBrokerUrl = "localhost"; // 또는 127.0.0.1 / 210.119.12.76

            string[] Rooms = new[] { "FirstFloor", "SecondFloor", "ThirdFloor" };

            SensorFaker = new Faker<SensorInfo>()
            .RuleFor(o => o.Dev_Id, f => f.PickRandom(Rooms))
            .RuleFor(o => o.Curr_Time, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss.ff"))
            .RuleFor(o => o.Temp, f => float.Parse(f.Random.Float(19.0f, 35.9f).ToString("0.00")))
            .RuleFor(o => o.Humid, f => float.Parse(f.Random.Float(40.0f, 63.9f).ToString("0.0")));
        }
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            ConnectMqttBroker(); //MQTT브로커 접속
            StartPublish(); //fake 센싱 메시지 전송
        }
        private void ConnectMqttBroker()
        {
            BrokerClient = new MqttClient(TxtBrokerIp.Text);
            BrokerClient.Connect("FakerDaemon");
        }


        private void StartPublish()
        {
            MqttThread = new Thread(new ThreadStart(LoopPublish));
            //MqttThread = new Thread(() => LoopPublish);
            MqttThread.Start();
        }
        private void LoopPublish()
        {
            while (true)
            {
                SensorInfo value = SensorFaker.Generate();
                CurrValue = JsonConvert.SerializeObject(value, Formatting.Indented);
                BrokerClient.Publish("home/device/data/", Encoding.Default.GetBytes(CurrValue));
                //Console.WriteLine($"Publish : {CurrValue}");
                this.Invoke(new Action(() =>
                {
                    RtbLog.AppendText($"Publish : {CurrValue}\n");
                    RtbLog.ScrollToCaret();
                }));

                Thread.Sleep(1000);
            }
        }
    }
}

