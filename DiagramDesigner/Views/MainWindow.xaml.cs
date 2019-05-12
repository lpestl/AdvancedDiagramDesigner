using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DiagramDesigner.Functionality;

namespace DiagramDesigner.Views
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<DiagramTabItem> diagramTabs { get; set; } = new ObservableCollection<DiagramTabItem>();
        public MainWindow()
        {
            InitializeComponent();

            diagramTabs.CollectionChanged += DiagramTabsOnCollectionChanged;

            DesignerCanvasesHolder.ItemsSource = diagramTabs;

            diagramTabs.Add(new DiagramTabItem
                {
                    Header = Properties.Resources.NewDiagram,
                    Content = new DiagramControl()
                });
        }

        private void DiagramTabsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetVisibilityTabItemHeaders(diagramTabs.Count > 1 ? Visibility.Visible : Visibility.Collapsed);
        }

        private void SetVisibilityTabItemHeaders(Visibility visibility)
        {
            Style s = new Style();
            s.Setters.Add(new Setter(UIElement.VisibilityProperty, visibility));
            DesignerCanvasesHolder.ItemContainerStyle = s;
        }

        private void CloseDiagram_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DesignerCanvasesHolder.Items.RemoveAt(DesignerCanvasesHolder.SelectedIndex);
        }
    }
}
