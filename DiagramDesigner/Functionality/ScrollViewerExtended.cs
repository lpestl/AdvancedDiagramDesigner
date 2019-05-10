using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DiagramDesigner.Functionality
{
    public class ScrollViewerExtended : ScrollViewer
    {
        private Point lastMousePosition;
        private Cursor lastCursor;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                CaptureMouse();
                lastMousePosition = e.GetPosition(this);
                Cursor = Cursors.ScrollAll;
                e.Handled = true;
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.MiddleButton == MouseButtonState.Released)
            {
                Cursor = Cursors.Arrow;
                ReleaseMouseCapture();
                e.Handled = true;
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if ((e.MiddleButton == MouseButtonState.Pressed) && (IsMouseCaptured))
            {
                var currPosition = e.GetPosition(this);

                var deltaX = (currPosition.X - lastMousePosition.X);
                var deltaY = (currPosition.Y - lastMousePosition.Y);

                //if (Math.Abs(deltaX) < 0.25 && Math.Abs(deltaY) < 0.25)
                    Cursor = Cursors.ScrollAll;
                //else
                //{
                //    if (Math.Abs(deltaX) < 0.25)
                //    {
                //        if (deltaY > 0)
                //            Cursor = Cursors.ScrollS;
                //        else
                //            Cursor = Cursors.ScrollN;
                //    }

                //    if (Math.Abs(deltaY) < 0.25)
                //    {
                //        if (deltaX > 0)
                //            Cursor = Cursors.ScrollE;
                //        else
                //            Cursor = Cursors.ScrollW;
                //    }

                //    if (deltaX > 0 && deltaY > 0) Cursor = Cursors.ScrollSE;
                //    else if (deltaX > 0 && deltaY < 0) Cursor = Cursors.ScrollNE;
                //    else if (deltaX < 0 && deltaY > 0) Cursor = Cursors.ScrollSW;
                //    else if (deltaX < 0 && deltaY < 0) Cursor = Cursors.ScrollNW;
                //}

                ScrollToHorizontalOffset(HorizontalOffset + deltaX);
                ScrollToVerticalOffset(VerticalOffset + deltaY);

                lastMousePosition = currPosition;

                e.Handled = true;
            }
        }
    }
}
