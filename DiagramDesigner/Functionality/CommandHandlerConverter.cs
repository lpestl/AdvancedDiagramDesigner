using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using DiagramDesigner.Views;

namespace DiagramDesigner.Functionality
{
    public class CommandHandlerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is DiagramTabItem diagramTabItem) && (diagramTabItem.Content is DiagramControl diagramControl))
            {
                return diagramControl.Designer;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
