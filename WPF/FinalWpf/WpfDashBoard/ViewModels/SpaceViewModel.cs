using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDashBoard.ViewModels
{
    class SpaceViewModel : Conductor<object>
    {
        public SpaceViewModel()
        {
            ActivateItem(new Space1FViewModel());
        }

        public void Load1F()
        {
            ActivateItem(new Space1FViewModel());
        }
        public void Load2F()
        {
            ActivateItem(new Space2FViewModel());
        }
        public void Load3F()
        {
            ActivateItem(new Space3FViewModel());
        }

    }
}
