using Caliburn.Micro;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WpfDashBoard.Helpers.Commons;
using WpfDashBoard.Models;
using MvvmDialogs;
using System.Windows;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;

namespace WpfDashBoard.ViewModels
{
    public class MembershipViewModel : Conductor<object>, IHaveDisplayName
    {
        #region ###############속성영역###############
        readonly IWindowManager windowManager;
        readonly IDialogService dialogService;

        private int num;
        private string memName;
        private string carModel;
        private string carNumber;
        private string telephone;
        private string entered;
        private string enteredDate;

        #endregion

        #region ###############생성자 영역###############

        public MembershipViewModel(IWindowManager windowManager, IDialogService dialogService) //Dialog를 위한 windowManager,,,
        {
            this.windowManager = windowManager;
            this.dialogService = dialogService;
            GetEmployees();
        }

        public int Num
        {
            get => num;
            set
            {
                num = value;
                NotifyOfPropertyChange(() => Num);
                NotifyOfPropertyChange(() => CanSaveEmployee);
                NotifyOfPropertyChange(() => CanDeleteEmployee);
            }
        }

        public string MemName
        {
            get => memName;
            set
            {
                memName = value;
                NotifyOfPropertyChange(() => MemName);
                NotifyOfPropertyChange(() => CanSaveEmployee);
            }
        }

        public string CarModel
        {
            get => carModel;
            set
            {
                carModel = value;
                NotifyOfPropertyChange(() => CarModel);
                NotifyOfPropertyChange(() => CanSaveEmployee);
            }
        }

        public string CarNumber
        {
            get => carNumber;
            set
            {
                carNumber = value;
                NotifyOfPropertyChange(() => CarNumber);
                NotifyOfPropertyChange(() => CanSaveEmployee);
            }
        }

        public string Telephone
        {
            get => telephone;
            set
            {
                telephone = value;
                NotifyOfPropertyChange(() => Telephone);
                NotifyOfPropertyChange(() => CanSaveEmployee);
            }
        }

        public string Entered
        {
            get => entered;
            set
            {
                entered = value;
                NotifyOfPropertyChange(() => Entered);
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

        //전체 employees 리스트
        BindableCollection<EmployeesModel> employees;
        public BindableCollection<EmployeesModel> Employees
        {
            get => employees;
            set
            {
                employees = value;
                NotifyOfPropertyChange(() => Employees);
            }
        }

        EmployeesModel selectedEmployee;
        public EmployeesModel SelectedEmployee                  //리스트 중 선택된 하나의 employee
        {
            get => selectedEmployee;
            set
            {
                selectedEmployee = value;
                if (value != null)
                {
                    Num = value.Num;
                    MemName = value.MemName;
                    CarModel = value.CarModel;
                    CarNumber = value.CarNumber;
                    Telephone = value.Telephone;
                    Entered = value.Entered;
                    EnteredDate = value.EnteredDate;
                }
                NotifyOfPropertyChange(() => SelectedEmployee);
            }
        }

        #endregion


        #region ###############사용자 함수영역###############
        public void NewEmployee()   //초기화 하는 함수
        {
            Num = 0;
            MemName = CarModel = CarNumber = Telephone = Entered = EnteredDate= string.Empty;
        }

        private void GetEmployees()   //화면에 디스플레이 하는 함수
        {
            using (MySqlConnection conn = new MySqlConnection(CONSTRING))       //DB접속
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(EmployeesTbl.SELECT_EMPLOYEE, conn);                                      
                MySqlDataReader reader = cmd.ExecuteReader();
                Employees = new BindableCollection<EmployeesModel>();
                while (reader.Read())
                {
                    var temp = new EmployeesModel
                    {
                        Num = (int)reader["Num"],
                        MemName = reader["MemName"].ToString(),
                        CarModel = reader["CarModel"].ToString(),
                        CarNumber = reader["CarNumber"].ToString(),
                        Telephone = reader["Telephone"].ToString(),
                        Entered = reader["Entered"].ToString(),
                        EnteredDate = reader["EnteredDate"].ToString(),
                    };
                    Employees.Add(temp);
                }
            }
        }
        public bool CanSaveEmployee
        {
            get => !(string.IsNullOrEmpty(MemName) || string.IsNullOrEmpty(Telephone));
        }
        public void SaveEmployee()     //갱신하는 함수
        {
            int resultRow = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(CONSTRING))       //DB접속
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    if (Num == 0)  //insert
                        cmd.CommandText = EmployeesTbl.INSERT_EMPLOYEE;
                    else             //update
                        cmd.CommandText = EmployeesTbl.UPDATE_EMPLOYEE;

                    if (Num != 0)
                    {
                        MySqlParameter paramid = new MySqlParameter("@Num", MySqlDbType.Int32);
                        paramid.Value = num;
                        cmd.Parameters.Add(paramid);
                    }
                    MySqlParameter paramMemName = new MySqlParameter("@MemName", MySqlDbType.VarChar, 45);
                    paramMemName.Value = MemName;
                    cmd.Parameters.Add(paramMemName);

                    MySqlParameter paramCarModel = new MySqlParameter("@CarModel", MySqlDbType.VarChar,45);
                    paramCarModel.Value = CarModel;
                    cmd.Parameters.Add(paramCarModel);

                    MySqlParameter paramCarNumber = new MySqlParameter("@CarNumber", MySqlDbType.VarChar, 15);
                    paramCarNumber.Value = CarNumber;
                    cmd.Parameters.Add(paramCarNumber);

                    MySqlParameter paramTelephone = new MySqlParameter("@Telephone", MySqlDbType.VarChar, 20);
                    paramTelephone.Value = Telephone;
                    cmd.Parameters.Add(paramTelephone);
                    resultRow = cmd.ExecuteNonQuery();
                }

                if (resultRow > 0)
                {
                    DialogViewModel dialogVM = new DialogViewModel();
                    dialogVM.DisplayName = "저장했습니당";
                    bool? success = windowManager.ShowDialog(dialogVM);
                    GetEmployees();                        
                    NewEmployee();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool CanDeleteEmployee
        {
            get => !(Num == 0);
        }
        public void DeleteEmployee()
        {
            int resultRow = 0;
            using (MySqlConnection conn = new MySqlConnection(CONSTRING))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(EmployeesTbl.DELETE_EMPLOYEE, conn);
                cmd.Connection = conn;
                cmd.CommandText = EmployeesTbl.DELETE_EMPLOYEE;

                MySqlParameter paramNum = new MySqlParameter("@Num", MySqlDbType.Int32);
                paramNum.Value = Num;
                cmd.Parameters.Add(paramNum);

                resultRow = cmd.ExecuteNonQuery();

            }
            if (resultRow > 0)
            {
                DialogViewModel dialogVM = new DialogViewModel();
                dialogVM.DisplayName = "삭제했습니다";
                bool? success = windowManager.ShowDialog(dialogVM);
                GetEmployees();
                NewEmployee();
            }
        }
        
        #endregion
    }
}
