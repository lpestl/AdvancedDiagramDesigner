using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DiagramDesigner.Functionality
{
    public class TabItemHeader : StackPanel
    {
        public event MouseButtonEventHandler CloseMouseUpEventHandler;

        public TextBlock HeaderTextBlock { get; set; }

        public TabItemHeader(string caption)
        {
            Orientation = Orientation.Horizontal;

            Height = 21;

            var closeImage = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Delete.png")),
                Margin = new Thickness(5, -5, -5, 0),
                Width = 13,
                Height = 13
            };

            closeImage.AddHandler(MouseUpEvent, new MouseButtonEventHandler(CloseMouseUp));

            HeaderTextBlock = new TextBlock {Text = caption};
            this.Children.Add(HeaderTextBlock);
            this.Children.Add(closeImage);
        }

        private void CloseMouseUp(object sender, MouseButtonEventArgs e)
        {
            CloseMouseUpEventHandler?.Invoke(sender, e);
        }
    }
}
