using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using WpfDashBoard.Helpers;

namespace WpfDashBoard.Views
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainView : MetroWindow
    {
        //DateTime CloseTime;
        public MainView()
        {
            InitializeComponent();
        }

        #region Today함수
        //종료누르면 사용 시간이 DB에 저장된다.
        //private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    CloseTime = DateTime.Now;
        //    TimeSpan timeDiff = CloseTime - Commons.OPENEDTIME;
        //    Commons.TOTALTIME = timeDiff.TotalMinutes;

        //    string INSERT_TIME = "INSERT INTO usedtime  " +
        //                                           " (                                      " +
        //                                           " YDate,                            " +
        //                                           " TotalUsedTime              " + 
        //                                           " )                                      " +
        //                                           " VALUES                          " +
        //                                           " (                                      " +
        //                                           " @YDate,                        " +
        //                                           " @TotalUsedTime         " +
        //                                           " )                                      ";
        //    try
        //    {
        //        using (MySqlConnection conn = new MySqlConnection(Commons.CONSTRING))
        //        {
        //            conn.Open();
        //            MySqlCommand cmd = new MySqlCommand();
        //            cmd.Connection = conn;
        //            //날짜가 없으면 insert , 날짜가 있으면 update해서 추가
        //            cmd.CommandText = INSERT_TIME;

        //            MySqlParameter paramYDate = new MySqlParameter("@YDate", MySqlDbType.Date);
        //            paramYDate.Value = Commons.OPENEDTIME.ToString("yyyy-MM-dd");
        //            cmd.Parameters.Add(paramYDate);

        //            MySqlParameter paramTotalUsedTime = new MySqlParameter("@TotalUsedTime", MySqlDbType.Double);
        //            paramTotalUsedTime.Value = Commons.TOTALTIME;
        //            cmd.Parameters.Add(paramTotalUsedTime);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        MessageBox.Show(ex.Message);
        //    }

        //}
        #endregion

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
