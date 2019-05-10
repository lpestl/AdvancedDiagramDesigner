using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DiagramDesigner.Functionality;

namespace DiagramDesigner.Controls
{
    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(DragThumb_DragDelta);
        }

        void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            DesignerItem designerItem = this.DataContext as DesignerItem;
            Functionality.DesignerCanvas designer = VisualTreeHelper.GetParent(designerItem) as Functionality.DesignerCanvas;
            if (designerItem != null && designer != null && designerItem.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

                // we only move DesignerItems
                var designerItems = designer.SelectionService.CurrentSelection.OfType<DesignerItem>();
                
                foreach (DesignerItem item in designerItems)
                {
                    double left = Canvas.GetLeft(item);
                    double top = Canvas.GetTop(item);

                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
                }

                double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                double deltaVertical = Math.Max(-minTop, e.VerticalChange);

                foreach (DesignerItem item in designerItems)
                {
                    double left = Canvas.GetLeft(item);
                    double top = Canvas.GetTop(item);

                    if (double.IsNaN(left)) left = 0;
                    if (double.IsNaN(top)) top = 0;

                    Canvas.SetLeft(item, left + deltaHorizontal);
                    Canvas.SetTop(item, top + deltaVertical);
                }

                // UPD: Check to create automatic links if the object being moved is one
                //if (designerItems.Count() == 1)
                CheckAutoCreateConnection(designerItem, designer);

                designer.InvalidateMeasure();
                e.Handled = true;
            }
        }

        private double GetDistance(DesignerItem from, DesignerItem to)
        {
            Point fromPos = new Point(Canvas.GetLeft(from) + from.Width / 2, Canvas.GetTop(from) + from.Height / 2);
            Point toPos = new Point(Canvas.GetLeft(to) + to.Width / 2, Canvas.GetTop(to) + to.Height / 2);
            Point delta = new Point(Math.Abs(fromPos.X - toPos.X), Math.Abs(fromPos.Y - toPos.Y));

            return Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        }

        private void CheckAutoCreateConnection(DesignerItem movingDesignerItem, DesignerCanvas designer)
        {
            Tuple<DesignerItem, double> nearestItem = new Tuple<DesignerItem, double>(null, double.MaxValue);
            foreach (var designerChild in designer.Children)
            {
                if ((designerChild is DesignerItem neighboringItem) && (neighboringItem != movingDesignerItem))
                {
                    var distance = GetDistance(movingDesignerItem, neighboringItem);
                    if (nearestItem.Item2 > distance)
                        nearestItem = new Tuple<DesignerItem, double>(neighboringItem, distance);
                }
            }

            if ((nearestItem.Item1 == null) || (nearestItem.Item2 > 115))
                return;

            // TODO: Create dash dot template connection
        }
    }
}
