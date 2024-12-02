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

        private List<Label> CameraPositionLabels;

        // The state of the trackball
        private bool IsEnabled;

        private double speed = 1;

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

        public List<Label> Labels
        {
            get { return CameraPositionLabels ?? (CameraPositionLabels = new List<Label>()); }
            set { CameraPositionLabels = value; }
        }

        public bool Enabled
        {
            get { return IsEnabled && (SlaveViewPorts != null) && (SlaveViewPorts.Count > 0); }
            set { IsEnabled = value; UpdateCameraPositionLabels(); }
        }

        public void Attach(FrameworkElement element)
        {
            element.MouseMove += MouseMoveHandler;
            element.MouseRightButtonDown += MouseRightButtonDownHandler;
            element.MouseRightButtonUp += MouseRightButtonUpHandler;
            element.MouseWheel += OnMouseWheel;

            element.KeyDown += KeyDown;
        }

        public void Detach(FrameworkElement element)
        {
            element.MouseMove -= MouseMoveHandler;
            element.MouseRightButtonDown -= MouseRightButtonDownHandler;
            element.MouseRightButtonUp -= MouseRightButtonUpHandler;
            element.MouseWheel -= OnMouseWheel;
        }

        private void UpdateCameraPositionLabels()
        {
            ProjectionCamera camera = (ProjectionCamera)SlaveViewPorts[0].Camera;

            CameraPositionLabels[0].Content = "X : " + camera.Position.X.ToString("000.00");
            CameraPositionLabels[1].Content = "Z : " + camera.Position.Y.ToString("000.00");
            CameraPositionLabels[2].Content = "Y : " + camera.Position.Z.ToString("000.00");
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

                double angleY = u * diffY;

                // Cross Product gets a vector that is perpendicular to the passed in vectors (order does matter, reverse the order and the vector will point in the reverse direction)
                Vector3D cp = Vector3D.CrossProduct(camera.UpDirection, camera.LookDirection);
                cp.Normalize();

                Matrix3D mY = new Matrix3D();
                mY.Rotate(new Quaternion(cp, angleY)); // Rotate about the vector from the cross product
                camera.LookDirection = mY.Transform(camera.LookDirection);

                double angleX = u * diffX;

                Matrix3D mX = new Matrix3D();
                mX.Rotate(new Quaternion(camera.UpDirection, angleX)); // Rotate about the camera's up direction to look left/right
                camera.LookDirection = mX.Transform(camera.LookDirection);

                InitialPoint = e.MouseDevice.GetPosition(el);
            }
        }

        private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            e.Handled = true;

            UIElement el = (UIElement)sender;
            Mouse.OverrideCursor = Cursors.ScrollAll;
            InitialPoint = e.MouseDevice.GetPosition(el);

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

            UpdateCameraPositionLabels();
        }

        private void MoveForwardBackwards(double u)
        {
            ProjectionCamera camera = (ProjectionCamera)SlaveViewPorts[0].Camera;
            Vector3D direction = camera.LookDirection;
            Point3D position = camera.Position;

            direction.Normalize();
            position = position + u * direction;

            camera.Position = position;
        }

        private void MoveUpDown(double u)
        {
            ProjectionCamera camera = (ProjectionCamera)SlaveViewPorts[0].Camera;
            Vector3D direction = new Vector3D(0, 1, 0);
            Point3D position = camera.Position;

            u = u / 2;

            direction.Normalize();
            position = position + u * direction;

            camera.Position = position;
        }

        private void MoveSideways(double u)
        {
            ProjectionCamera camera = (ProjectionCamera)SlaveViewPorts[0].Camera;
            Vector3D direction = new Vector3D(camera.LookDirection.Z, 0, -camera.LookDirection.X);
            Point3D position = camera.Position;

            u = u / 2;

            direction.Normalize();
            position = position + u * direction;

            camera.Position = position;
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                speed = 1.75;
            }
            else
            {
                speed = 1;
            }

            if (Keyboard.IsKeyDown(Key.W))
            {
                MoveForwardBackwards(speed);
            }
            
            if (Keyboard.IsKeyDown(Key.A))
            {
                MoveSideways(speed);
            }
            
            if (Keyboard.IsKeyDown(Key.S))
            {
                MoveForwardBackwards(-speed);
            }
            
            if (Keyboard.IsKeyDown(Key.D))
            {
                MoveSideways(-speed);
            }
            
            if (Keyboard.IsKeyDown(Key.Space))
            {
                MoveUpDown(speed);
            }
            
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                MoveUpDown(-speed);
            }

            UpdateCameraPositionLabels();
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

            UpdateCameraPositionLabels();
        }
    }
}