using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DiagramDesigner.Functionality;

namespace DiagramDesigner.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DesignerCanvasesHolder.Items.Add(new DiagramTabItem
            {
                Header = Properties.Resources.NewDiagram,
                Content = new DiagramControl()
            });
        }

        private void CloseDiagram_MouseDown(object sender, MouseButtonEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
