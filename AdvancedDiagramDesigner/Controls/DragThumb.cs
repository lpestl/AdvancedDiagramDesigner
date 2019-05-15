using System;
using System.Collections.Generic;
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
            base.DragDelta += DragThumb_DragDelta;
            base.DragCompleted += OnDragCompleted;
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

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (lastAutoCreatedConnection != null)
                lastAutoCreatedConnection.StrokeDashArray = null;
            lastAutoCreatedConnection = null;
        }

        private Connection lastAutoCreatedConnection = null;
        private double GetDistance(DesignerItem from, DesignerItem to)
        {
            Point fromPos = new Point(Canvas.GetLeft(from) + from.Width / 2, Canvas.GetTop(from) + from.Height / 2);
            Point toPos = new Point(Canvas.GetLeft(to) + to.Width / 2, Canvas.GetTop(to) + to.Height / 2);
            Point delta = new Point(Math.Abs(fromPos.X - toPos.X), Math.Abs(fromPos.Y - toPos.Y));

            return Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        }

        private void CheckAutoCreateConnection(DesignerItem movingDesignerItem, DesignerCanvas designer)
        {
            // Find the nearest Item
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

            // If not founded, then return
            if ((nearestItem.Item1 == null) || (nearestItem.Item2 > 150))
            {
                if (lastAutoCreatedConnection != null)
                    designer.Children.Remove(lastAutoCreatedConnection);
                return;
            }

            // Find the nearest Connectors
            Control cd = movingDesignerItem.Template.FindName("PART_ConnectorDecorator", movingDesignerItem) as Control;
            Control nd = nearestItem.Item1.Template.FindName("PART_ConnectorDecorator", nearestItem.Item1) as Control;

            List<Connector> connectorsMoving = new List<Connector>();
            List<Connector> connectorsNearest = new List<Connector>();

            designer.GetConnectors(cd, connectorsMoving);
            designer.GetConnectors(nd, connectorsNearest);

            Tuple<Connector, Connector, double> nearestConnectors = new Tuple<Connector, Connector, double>(null, null, double.MaxValue);
            foreach (var connectorM in connectorsMoving)
            {
                foreach (var connectorN in connectorsNearest)
                {
                    Point delta = new Point(Math.Abs(connectorM.Position.X - connectorN.Position.X), Math.Abs(connectorM.Position.Y - connectorN.Position.Y));

                    var distance = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
                    if (nearestConnectors.Item3 > distance)
                        nearestConnectors = new Tuple<Connector, Connector, double>(connectorM, connectorN, distance);
                }
            }

            // If not founded, then return
            if (nearestConnectors.Item1 == null || nearestConnectors.Item2 == null)
                return;

            // Check connection already exist
            foreach (var designerChild in designer.Children)
            {
                if (designerChild is Connection connection)
                    if (((connection.Source.ParentDesignerItem == movingDesignerItem && connection.Sink.ParentDesignerItem == nearestItem.Item1) ||
                        (connection.Source.ParentDesignerItem == nearestItem.Item1 && connection.Sink.ParentDesignerItem == movingDesignerItem)) && 
                        connection.StrokeDashArray == null)
                        return;
            }
            
            Connection newConnection = null;
            if (movingDesignerItem.DateTimeCreated < nearestItem.Item1.DateTimeCreated)
            {
                if ((nearestConnectors.Item1.ValidateAddOutConnection()) &&
                    (nearestConnectors.Item2.ValidateAddInConnection()))
                    newConnection = new Connection(nearestConnectors.Item1, nearestConnectors.Item2);
                else
                    return;
            }
            else
            {
                if ((nearestConnectors.Item2.ValidateAddOutConnection()) &&
                    (nearestConnectors.Item1.ValidateAddInConnection()))
                    newConnection = new Connection(nearestConnectors.Item2, nearestConnectors.Item1);
                else
                    return;
            }

            // Remove last preview connection if it exist
            if (lastAutoCreatedConnection != null)
            {
                designer.Children.Remove(lastAutoCreatedConnection);
                lastAutoCreatedConnection = null;
            }

            // Create auto connection preview
            newConnection.StrokeDashArray = new DoubleCollection(new double[] { 1, 2 });

            Canvas.SetZIndex(newConnection, designer.Children.Count);
            designer.Children.Add(newConnection);
            lastAutoCreatedConnection = newConnection;
        }
    }
}
