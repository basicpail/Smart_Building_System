using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmDialogs.DialogTypeLocators;

namespace WpfDashBoard.Helpers
{
    public class DialogTypeLocator : IDialogTypeLocator
    {
        /// <summary>
        /// 특정 뷰 모델에다 Dialog타입을 위치시키는 메서드
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public Type Locate(INotifyPropertyChanged viewModel)
        {
            Type viewModelType = viewModel.GetType();
            var DialogFullName = viewModelType.FullName;

            DialogFullName = DialogFullName.Substring(0, DialogFullName.Length - "Model".Length);


            return viewModelType.Assembly.GetType(DialogFullName);
        }
    }
}
