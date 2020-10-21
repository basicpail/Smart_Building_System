using Caliburn.Micro;
using MvvmDialogs;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WpfDashBoard.Helpers;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Xml;

namespace WpfDashBoard.ViewModels
{
    public class MainViewModel : Conductor<object>,IHaveDisplayName
    {
        readonly IWindowManager windowManager;
        readonly IDialogService dialogService;
        
        //날씨값 받기위한 변수
        string Category = "";

        public MainViewModel(IWindowManager windowManager, IDialogService dialogService)
        {
            this.windowManager = windowManager;
            this.dialogService = dialogService;
            GetCountLogs();
            OpenedTimer();  //Based Time
            Weather();
            Setting();
            if (dashVM == null)
                dashVM = new DashBoardViewModel();

            ActivateItem(dashVM);
            //ActivateItem(new DashBoardViewModel());
        }


        private void Setting()
        {
            Commons.AlarmWater = "10";
            Commons.AlramStartTime = "21:00";
            Commons.AlramEndTime = "06:00";
            Commons.AlarmFanStartTime = "21:00";
            Commons.AlarmFanEndTime = "06:00";
            Commons.AlarmFirstTemp = "27";
            Commons.AlarmSecondTemp = "27";
            Commons.AlarmThirdTemp = "27";

        }

        //날씨 보여주는 함수
        private void Weather()
        {
            WebClient wc = null;
            XmlDocument doc = null;

            wc = new WebClient() { Encoding = Encoding.UTF8 };
            doc = new XmlDocument();
            StringBuilder str = new StringBuilder();
            str.Append("http://apis.data.go.kr/1360000/VilageFcstInfoService/getVilageFcst");
            str.Append("?serviceKey=LuZTEEVveo8mSqWLLl8umhdbZvlAu0xSQKBsxd09An5GRvQeu1UJFZ%2BdiLIQAD6%2BN5ZKXEg62x2o2yZE4WsKWQ%3D%3D");
            str.Append("&pageNo=1");
            str.Append("&numOfRows=200");
            str.Append("&dataType=XML");
            str.Append("&base_date="+ DateTime.Now.ToString("yyyyMMdd"));
            str.Append("&base_time=0800");
            str.Append("&nx=97");
            str.Append("&ny=75");
    
            string xml = wc.DownloadString(str.ToString());
            doc.LoadXml(xml);
            XmlElement root = doc.DocumentElement;
            XmlNodeList items = doc.GetElementsByTagName("item");

            foreach (XmlNode item in items)
            {
                Category = item["category"].InnerText;
                if(Category == "SKY")
                {
                    Commons.WEATHER = item["fcstValue"].InnerText;
                }
            }
        }

        //Count Log보내주기위한 함수
        private void GetCountLogs()
        {
            using(MySqlConnection conn = new MySqlConnection(Commons.CONSTRING))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(Commons.CarTbl.COUNT_CAR,conn);
                Commons.CARLOGCOUNT = int.Parse(cmd.ExecuteScalar().ToString());
                MySqlCommand cmd2 = new MySqlCommand(Commons.THTbl.COUNT_TH, conn);
                Commons.THLOGCOUNT = int.Parse(cmd2.ExecuteScalar().ToString());
                MySqlCommand cmd3 = new MySqlCommand(Commons.THTbl.COUNT_TH, conn);
                Commons.TEMPHUMIDCOUNT = int.Parse(cmd2.ExecuteScalar().ToString());
            }
        }

        private void OpenedTimer()
        {
            Commons.OPENEDTIME = DateTime.Now;
        }

        DashBoardViewModel dashVM = null;
        //SpaceViewModel SpaceVM = null;
        //AlarmandLogViewModel AlarmVM = null;
        //MembershipViewModel MembershipVM =null;

        public void LoadDashBoard()
        {
            if (dashVM == null)
                dashVM = new DashBoardViewModel();

            ActivateItem(dashVM);
            //ActivateItem(new DashBoardViewModel());
            
        }
        public void LoadSpace()
        {
            ActivateItem(new SpaceViewModel());
        }
        public void LoadAlramandLog()
        {
            ActivateItem(new AlarmandLogViewModel());
        }

        public void LoadMembership()
        {
            ActivateItem(new MembershipViewModel(windowManager, dialogService));

        }
    }
}
