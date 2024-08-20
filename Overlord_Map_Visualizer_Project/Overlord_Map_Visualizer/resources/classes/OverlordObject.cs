using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using DrawingPoint = System.Drawing.Point;

namespace Overlord_Map_Visualizer
{
    enum OverlordObjectType
    {
        BrownMinionGate,
        RedMinionGate,
        GreenMinionGate,
        BlueMinionGate,
        TowerGate,
        TowerGateVariant,
        HealthPillar,
        ManaPillar,
        MinionPillar,
        SpellStone,
        SpellCatalyst,
        Smelter,
        Mould,
        ForgeStone,
        MinionHive,
        OtherObjects
    }
    class OverlordObject
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public OverlordObjectType Type { get; set; }

        public OverlordObject(float x, float y, float z, OverlordObjectType type)
        {
            X = x;
            Y = y;
            Z = z;
            Type = type;
        }

        public Bitmap DrawMinionGate(Bitmap entireLocationBitmap, SolidBrush objectSolidColor)
        {
            int diameter = 7;

            using (Graphics locationGraphics = Graphics.FromImage(entireLocationBitmap))
            using (Pen objectPen = new Pen(objectSolidColor))
            {
                locationGraphics.DrawEllipse(objectPen, X - (diameter / 2), Z - (diameter / 2), diameter, diameter);
                locationGraphics.FillEllipse(objectSolidColor, X - (diameter / 2), Z - (diameter / 2), diameter, diameter);

                return entireLocationBitmap;
            }
        }

        public GeometryModel3D DrawCube(DiffuseMaterial material, float diameter)
        {
            float radius = diameter / 2;
            GeometryModel3D cube = new GeometryModel3D(new MeshGeometry3D(), material);
            cube.BackMaterial = cube.Material;

            Point3DCollection point3DCollection = new Point3DCollection();
            Int32Collection triangleIndices = new Int32Collection();

            point3DCollection.Add(new Point3D(X - radius, Y - radius, Z - radius));
            point3DCollection.Add(new Point3D(X + radius, Y - radius, Z - radius));
            point3DCollection.Add(new Point3D(X - radius, Y + radius, Z - radius));
            point3DCollection.Add(new Point3D(X + radius, Y + radius, Z - radius));
            point3DCollection.Add(new Point3D(X - radius, Y - radius, Z + radius));
            point3DCollection.Add(new Point3D(X + radius, Y - radius, Z + radius));
            point3DCollection.Add(new Point3D(X - radius, Y + radius, Z + radius));
            point3DCollection.Add(new Point3D(X + radius, Y + radius, Z + radius));

            ((MeshGeometry3D)cube.Geometry).Positions = point3DCollection;

            triangleIndices.Add(2);
            triangleIndices.Add(3);
            triangleIndices.Add(1);

            triangleIndices.Add(2);
            triangleIndices.Add(1);
            triangleIndices.Add(0);

            triangleIndices.Add(7);
            triangleIndices.Add(1);
            triangleIndices.Add(3);

            triangleIndices.Add(7);
            triangleIndices.Add(5);
            triangleIndices.Add(1);

            triangleIndices.Add(6);
            triangleIndices.Add(5);
            triangleIndices.Add(7);

            triangleIndices.Add(6);
            triangleIndices.Add(4);
            triangleIndices.Add(5);

            triangleIndices.Add(6);
            triangleIndices.Add(2);
            triangleIndices.Add(0);

            triangleIndices.Add(6);
            triangleIndices.Add(0);
            triangleIndices.Add(4);

            triangleIndices.Add(2);
            triangleIndices.Add(7);
            triangleIndices.Add(3);

            triangleIndices.Add(2);
            triangleIndices.Add(6);
            triangleIndices.Add(7);

            triangleIndices.Add(0);
            triangleIndices.Add(1);
            triangleIndices.Add(5);

            triangleIndices.Add(0);
            triangleIndices.Add(5);
            triangleIndices.Add(4);

            ((MeshGeometry3D)cube.Geometry).TriangleIndices = triangleIndices;

            Transform3DGroup myTransformGroup = new Transform3DGroup();

            // Create a transform to scale the size.
            ScaleTransform3D myScaleTransform = new ScaleTransform3D();

            // Create a transform to rotate the button
            RotateTransform3D myRotateTransform = new RotateTransform3D();

            //Create a transform to move from one position to other
            TranslateTransform3D myTranslateTransform = new TranslateTransform3D();

            myTransformGroup.Children.Add(myScaleTransform);
            myTransformGroup.Children.Add(myRotateTransform);
            myTransformGroup.Children.Add(myTranslateTransform);

            cube.Transform = myTransformGroup;

            return cube;
        }

        public GeometryModel3D DrawPyramid(DiffuseMaterial material, float diameter, float height)
        {
            float radius = diameter / 2;
            GeometryModel3D cube = new GeometryModel3D(new MeshGeometry3D(), material);
            cube.BackMaterial = cube.Material;

            Point3DCollection point3DCollection = new Point3DCollection();
            Int32Collection triangleIndices = new Int32Collection();

            point3DCollection.Add(new Point3D(X - radius, Y, Z - radius));
            point3DCollection.Add(new Point3D(X - radius, Y, Z + radius));
            point3DCollection.Add(new Point3D(X + radius, Y, Z - radius));
            point3DCollection.Add(new Point3D(X + radius, Y, Z + radius));
            point3DCollection.Add(new Point3D(X, Y + height, Z));

            ((MeshGeometry3D)cube.Geometry).Positions = point3DCollection;

            triangleIndices.Add(0);
            triangleIndices.Add(1);
            triangleIndices.Add(4);

            triangleIndices.Add(0);
            triangleIndices.Add(2);
            triangleIndices.Add(4);

            triangleIndices.Add(3);
            triangleIndices.Add(1);
            triangleIndices.Add(4);

            triangleIndices.Add(3);
            triangleIndices.Add(2);
            triangleIndices.Add(4);

            triangleIndices.Add(0);
            triangleIndices.Add(1);
            triangleIndices.Add(3);

            triangleIndices.Add(0);
            triangleIndices.Add(2);
            triangleIndices.Add(3);

            ((MeshGeometry3D)cube.Geometry).TriangleIndices = triangleIndices;

            Transform3DGroup myTransformGroup = new Transform3DGroup();

            // Create a transform to scale the size.
            ScaleTransform3D myScaleTransform = new ScaleTransform3D();

            // Create a transform to rotate the button
            RotateTransform3D myRotateTransform = new RotateTransform3D();

            //Create a transform to move from one position to other
            TranslateTransform3D myTranslateTransform = new TranslateTransform3D();

            myTransformGroup.Children.Add(myScaleTransform);
            myTransformGroup.Children.Add(myRotateTransform);
            myTransformGroup.Children.Add(myTranslateTransform);

            cube.Transform = myTransformGroup;

            return cube;
        }

        public Bitmap DrawTowerGate(Bitmap entireLocationBitmap, int x, int y)
        {
            using (Graphics locationGraphics = Graphics.FromImage(entireLocationBitmap))
            using (Pen borderPen = new Pen(new SolidBrush(Color.FromArgb(255, 255, 255, 255))))
            using (Pen objectPen = new Pen(new SolidBrush(Color.FromArgb(255, 000, 000, 000))))
            {
                locationGraphics.DrawLine(objectPen, 0, 0, 0, 0);
                GraphicsPath path = new GraphicsPath();
                DrawingPoint point01 = new DrawingPoint(x - 14, y - 09);
                DrawingPoint point02 = new DrawingPoint(x - 13, y - 09);
                DrawingPoint point03 = new DrawingPoint(x - 13, y - 11);
                DrawingPoint point04 = new DrawingPoint(x - 12, y - 11);
                DrawingPoint point05 = new DrawingPoint(x - 12, y - 12);
                DrawingPoint point06 = new DrawingPoint(x - 10, y - 12);
                DrawingPoint point07 = new DrawingPoint(x - 10, y - 10);
                DrawingPoint point08 = new DrawingPoint(x - 09, y - 10);
                DrawingPoint point09 = new DrawingPoint(x - 09, y - 08);
                DrawingPoint point10 = new DrawingPoint(x - 08, y - 08);

                locationGraphics.DrawPath(objectPen, path);

                //locationGraphics.DrawRectangle(objectPen, new Rectangle(x, y, 28, 24));
                return entireLocationBitmap;
            }
        }

        public Bitmap DrawTowerGateVariant(Bitmap entireLocationBitmap)
        {
            int diameter = 15;

            using (Graphics locationGraphics = Graphics.FromImage(entireLocationBitmap))
            using (Pen borderPen = new Pen(new SolidBrush(Color.FromArgb(255, 255, 255, 255))))
            using (Pen objectPen = new Pen(new SolidBrush(Color.FromArgb(255, 000, 000, 000))))
            using (SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0)))
            using (SolidBrush objectBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0)))
            {
                DrawingPoint origin = new DrawingPoint((int)(X - (diameter / 2)), (int)(Z - (diameter / 2)));
                locationGraphics.DrawEllipse(borderPen, origin.X, origin.Y, diameter, diameter);
                locationGraphics.FillEllipse(backgroundBrush, X - (diameter / 2), Z - (diameter / 2), diameter, diameter);

                locationGraphics.DrawRectangle(objectPen, new Rectangle(origin.X + 11, origin.Y + 8, 3, 6));
                locationGraphics.FillRectangle(objectBrush, new Rectangle(origin.X + 11, origin.Y + 8, 3, 6));

                locationGraphics.DrawRectangle(objectPen, new Rectangle(origin.X + 6, origin.Y + 8, 3, 6));
                locationGraphics.FillRectangle(objectBrush, new Rectangle(origin.X + 6, origin.Y + 8, 3, 6));

                locationGraphics.DrawRectangle(objectPen, new Rectangle(origin.X + 1, origin.Y + 8, 3, 6));
                locationGraphics.FillRectangle(objectBrush, new Rectangle(origin.X + 1, origin.Y + 8, 3, 6));

                locationGraphics.DrawRectangle(objectPen, new Rectangle(origin.X + 4, origin.Y + 3, 6, 7));
                locationGraphics.FillRectangle(objectBrush, new Rectangle(origin.X + 4, origin.Y + 3, 6, 7));

                locationGraphics.DrawRectangle(objectPen, new Rectangle(origin.X + 3, origin.Y + 1, 8, 1));
                locationGraphics.FillRectangle(objectBrush, new Rectangle(origin.X + 3, origin.Y + 1, 8, 1));

                return entireLocationBitmap;
            }
        }
    }
}
