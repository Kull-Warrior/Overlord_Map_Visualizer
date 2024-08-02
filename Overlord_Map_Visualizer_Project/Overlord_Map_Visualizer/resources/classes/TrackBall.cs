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

        private Vector3D Center;
        
        // The state of the trackball
        private bool IsRotationCenterDetermined; // Have we already determined the rotation center?
        private bool IsEnabled;
        private bool IsRotating;
        private bool IsScaling; // Are we scaling?  NOTE otherwise we're rotating
        
        // The state of the current drag
        private Point InitialPoint; // Initial point of drag
        
        private Quaternion Rotation;
        private Quaternion RotationDelta; // Change to rotation because of this drag
        
        private double Scale;
        private double ScaleDelta; // Change to scale because of this drag
        
        private Vector3D Translate;
        private Vector3D TranslateDelta;

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

        // Updates the matrices of the slaves using the rotation quaternion.
        private void UpdateSlaves(Quaternion q, double s, Vector3D t)
        {
            if (SlaveViewPorts != null)
            {
                foreach (Viewport3D i in SlaveViewPorts)
                {
                    ModelVisual3D mv = i.Children[0] as ModelVisual3D;
                    Transform3DGroup t3Dg = mv.Transform as Transform3DGroup;

                    ScaleTransform3D groupScaleTransform = t3Dg.Children[0] as ScaleTransform3D;
                    RotateTransform3D groupRotateTransform = t3Dg.Children[1] as RotateTransform3D;
                    TranslateTransform3D groupTranslateTransform = t3Dg.Children[2] as TranslateTransform3D;

                    groupScaleTransform.ScaleX = s;
                    groupScaleTransform.ScaleY = s;
                    groupScaleTransform.ScaleZ = s;
                    groupRotateTransform.Rotation = new AxisAngleRotation3D(q.Axis, q.Angle);
                    groupTranslateTransform.OffsetX = t.X;
                    groupTranslateTransform.OffsetY = t.Y;
                    groupTranslateTransform.OffsetZ = t.Z;
                }
            }
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;
            e.Handled = true;

            UIElement el = (UIElement)sender;

            if (el.IsMouseCaptured)
            {
                Vector delta = InitialPoint - e.MouseDevice.GetPosition(el);
                Vector3D t = new Vector3D();

                delta /= 2;
                Quaternion q = Rotation;

                if (IsRotating)
                {
                    // We can redefine this 2D mouse delta as a 3D mouse delta
                    // where "into the screen" is Z
                    Vector3D mouse = new Vector3D(delta.X, -delta.Y, 0);
                    Vector3D axis = Vector3D.CrossProduct(mouse, new Vector3D(0, 0, 1));
                    double len = axis.Length;
                    if (len < 0.00001 || IsScaling)
                        RotationDelta = new Quaternion(new Vector3D(0, 0, 1), 0);
                    else
                        RotationDelta = new Quaternion(axis, len);

                    q = RotationDelta * Rotation;
                }
                else
                {
                    delta /= 20;
                    TranslateDelta.X = delta.X * -1;
                    TranslateDelta.Y = delta.Y;
                }

                t = Translate + TranslateDelta;

                UpdateSlaves(q, Scale * ScaleDelta, t);
            }
        }

        private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!Enabled) return;
            e.Handled = true;

            UIElement el = (UIElement)sender;
            InitialPoint = e.MouseDevice.GetPosition(el);
            // Initialize the center of rotation to the lookatpoint
            if (!IsRotationCenterDetermined)
            {
                ProjectionCamera camera = (ProjectionCamera)SlaveViewPorts[0].Camera;
                Center = camera.LookDirection;
                IsRotationCenterDetermined = true;
            }

            IsScaling = e.MiddleButton == MouseButtonState.Pressed;

            IsRotating = Keyboard.IsKeyDown(Key.Space) == false;

            el.CaptureMouse();
        }

        private void MouseRightButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            e.Handled = true;

            // Stuff the current initial + delta into initial so when we next move we
            // start at the right place.
            if (IsRotating)
                Rotation = RotationDelta * Rotation;
            else
            {
                Translate += TranslateDelta;
                TranslateDelta.X = 0;
                TranslateDelta.Y = 0;
            }

            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            ScaleDelta += e.Delta / (double)1000;

            UpdateSlaves(Rotation, Scale * ScaleDelta, Translate);
        }

        private void Reset()
        {
            Rotation = new Quaternion(0, 0, 0, 1);
            Scale = 1;
            Translate.X = 0;
            Translate.Y = 0;
            Translate.Z = 0;
            TranslateDelta.X = 0;
            TranslateDelta.Y = 0;
            TranslateDelta.Z = 0;

            // Clear delta too, because if reset is called because of a double click then the mouse
            // up handler will also be called and this way it won't do anything.
            RotationDelta = Quaternion.Identity;
            ScaleDelta = 1;
            UpdateSlaves(Rotation, Scale, Translate);
        }
    }
}