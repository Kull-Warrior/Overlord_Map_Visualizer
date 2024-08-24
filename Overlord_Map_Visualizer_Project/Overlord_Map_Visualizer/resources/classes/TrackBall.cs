// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Overlord_Map_Visualizer
{
    public class Trackball
    {
        private List<Viewport3D> SlaveViewPorts;

        // The state of the trackball
        private bool IsEnabled;
        private bool IsRotatingVertically = false;
        private bool IsRotatingHorizontally = false;

        // The state of the current drag
        private Point InitialPoint; // Initial point of drag

        public Trackball()
        {
            Reset();
        }

        public List<Viewport3D> Slaves
        {
            get { return SlaveViewPorts ?? (SlaveViewPorts = new List<Viewport3D>()); }
            set { SlaveViewPorts = value; }
        }

        public bool Enabled
        {
            get { return IsEnabled && (SlaveViewPorts != null) && (SlaveViewPorts.Count > 0); }
            set { IsEnabled = value; }
        }

        public void Attach(FrameworkElement element)
        {
            element.MouseMove += MouseMoveHandler;
            element.MouseLeftButtonDown += MouseLeftButtonDownHandler;
            element.MouseLeftButtonUp += MouseLeftButtonUpHandler;
            element.MouseRightButtonDown += MouseRightButtonDownHandler;
            element.MouseRightButtonUp += MouseRightButtonUpHandler;
            element.MouseWheel += OnMouseWheel;
        }

        public void Detach(FrameworkElement element)
        {
            element.MouseMove -= MouseMoveHandler;
            element.MouseRightButtonDown -= MouseRightButtonDownHandler;
            element.MouseRightButtonUp -= MouseRightButtonUpHandler;
            element.MouseWheel -= OnMouseWheel;
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            e.Handled = true;

            UIElement el = (UIElement)sender;

            if (el.IsMouseCaptured)
            {
                double u = 0.05;
                double diffX = InitialPoint.X - e.MouseDevice.GetPosition(el).X;
                double diffY = InitialPoint.Y - e.MouseDevice.GetPosition(el).Y;
                ProjectionCamera camera = (ProjectionCamera)SlaveViewPorts[0].Camera;

                if (IsRotatingVertically)
                {
                    double angleD = u * diffY;

                    // Cross Product gets a vector that is perpendicular to the passed in vectors (order does matter, reverse the order and the vector will point in the reverse direction)
                    Vector3D cp = Vector3D.CrossProduct(camera.UpDirection, camera.LookDirection);
                    cp.Normalize();

                    Matrix3D m = new Matrix3D();
                    m.Rotate(new Quaternion(cp, -angleD)); // Rotate about the vector from the cross product
                    camera.LookDirection = m.Transform(camera.LookDirection);
                }

                if(IsRotatingHorizontally)
                {
                    double angleD = u * diffX;

                    Matrix3D m = new Matrix3D();
                    m.Rotate(new Quaternion(camera.UpDirection, -angleD)); // Rotate about the camera's up direction to look left/right
                    camera.LookDirection = m.Transform(camera.LookDirection);
                }

                InitialPoint = e.MouseDevice.GetPosition(el);
            }
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            e.Handled = true;

            UIElement el = (UIElement)sender;
            Mouse.OverrideCursor = Cursors.ScrollWE;
            InitialPoint = e.MouseDevice.GetPosition(el);

            IsRotatingVertically = false;
            IsRotatingHorizontally = true;

            el.CaptureMouse();
        }

        private void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }

            e.Handled = true;

            UIElement el = (UIElement)sender;
            Mouse.OverrideCursor = null;
            el.ReleaseMouseCapture();
        }

        private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            e.Handled = true;

            UIElement el = (UIElement)sender;
            Mouse.OverrideCursor = Cursors.ScrollNS;
            InitialPoint = e.MouseDevice.GetPosition(el);

            IsRotatingVertically = true;
            IsRotatingHorizontally = false;

            el.CaptureMouse();
        }

        private void MouseRightButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }

            e.Handled = true;

            UIElement el = (UIElement)sender;
            Mouse.OverrideCursor = null;
            el.ReleaseMouseCapture();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            double u = 0.15;
            ProjectionCamera camera = (ProjectionCamera)SlaveViewPorts[0].Camera;
            Vector3D lookDirection = camera.LookDirection;
            Point3D position = camera.Position;

            lookDirection.Normalize();
            position = position + u * lookDirection * e.Delta;

            camera.Position = position;
        }

        public void Reset()
        {
            if (!Enabled)
            {
                return;
            }

            ProjectionCamera camera = (ProjectionCamera)SlaveViewPorts[0].Camera;

            camera.Position = new Point3D(512, 100, 512);
            camera.LookDirection = new Vector3D(-0.8, -0.2, -0.8);
        }
    }
}