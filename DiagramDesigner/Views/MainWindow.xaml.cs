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
        public ObservableCollection<DiagramTabItem> DiagramTabs { get; set; } = new ObservableCollection<DiagramTabItem>();
        public MainWindow()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New_Executed));

            InitializeComponent();

            DiagramTabs.CollectionChanged += DiagramTabsOnCollectionChanged;

            //DesignersTabControl.ItemsSource = DiagramTabs;

            DiagramTabs.Add(new DiagramTabItem
                {
                    Header = Properties.Resources.NewDiagram,
                    Content = new DiagramControl()
                });
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DiagramTabs.Add(new DiagramTabItem
            {
                Header = Properties.Resources.NewDiagram,
                Content = new DiagramControl()
            });

            e.Handled = true;
        }

        private void DiagramTabsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetVisibilityTabItemHeaders(DiagramTabs.Count > 1 ? Visibility.Visible : Visibility.Collapsed);
        }

        private void SetVisibilityTabItemHeaders(Visibility visibility)
        {
            Style s = new Style();
            s.Setters.Add(new Setter(UIElement.VisibilityProperty, visibility));
            DesignersTabControl.ItemContainerStyle = s;
        }

        private void CloseDiagram_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DiagramTabs.RemoveAt(DesignersTabControl.SelectedIndex);
        }
    }
}
