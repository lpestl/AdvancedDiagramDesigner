using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiagramDesigner.Views
{
    public class CaptionChangedEventArgs : EventArgs
    {
        public string OldCaption { get; set; }

        public string NewCaption { get; set; }
    }

    /// <summary>
    /// Interaction logic for DiagramControl.xaml
    /// </summary>
    public partial class DiagramControl : UserControl
    {
        public event EventHandler<CaptionChangedEventArgs> CaptionChangedEventHandler;
        private string caption;
        public DiagramControl()
        {
            InitializeComponent();

            caption = Designer.Caption;
            Designer.PropertyChanged += DesignerOnPropertyChanged;
        }

        private void DesignerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Caption"))
            {
                CaptionChangedEventHandler?.Invoke(this, new CaptionChangedEventArgs { OldCaption = caption, NewCaption = Designer.Caption });
                caption = Designer.Caption;
            }
        }
    }
}
