﻿using System.Windows;

namespace AKG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Renderer renderer;

        private static double _angleX = 0;
        private static double _angleY = 0;
        private static double _angleZ = 0;
        private static double _scale = 1;
        private static double _scaleDiff = 1;
        private static float[] movement = { 0, 0, 0 };
        private static float[] camera_movement = { VectorTransformation.eye.X, VectorTransformation.eye.Y, VectorTransformation.eye.Z };

        public double angleX
        {
            get { return _angleX; }
            set
            {
                if (value != _angleX)
                {
                    _angleX = value;
                    lbRotateX.Content = angleX.ToString("#.##");

                    VectorTransformation.TransformVectors((float)_angleX, (float)_angleY, (float)_angleZ, (float)_scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                }
            }
        }

        public double angleY
        {
            get { return _angleY; }
            set
            {
                if (value != _angleY)
                {
                    _angleY = value;
                    lbRotateY.Content = angleY.ToString("#.##");

                    VectorTransformation.TransformVectors((float)_angleX, (float)_angleY, (float)_angleZ, (float)_scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                }
            }
        }

        public double angleZ
        {
            get { return _angleZ; }
            set
            {
                if (value != _angleZ)
                {
                    _angleZ = value;
                    lbRotateZ.Content = angleZ.ToString("#.##");

                    VectorTransformation.TransformVectors((float)_angleX, (float)_angleY, (float)_angleZ, (float)_scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                }
            }
        }

        public double scale
        {
            get { return _scale; }
            set
            {
                if (value != _scale)
                {
                    _scale = value;
                    lbscale.Content = scale.ToString("#.##########");

                    VectorTransformation.TransformVectors((float)_angleX, (float)_angleY, (float)_angleZ, (float)_scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            renderer = new Renderer(img);

            Loaded += delegate
            {
                VectorTransformation.width = (float)img.ActualWidth;
                VectorTransformation.height = (float)img.ActualHeight;

                Model.ReadFile("..\\..\\..\\objects\\pekka.obj");

                VectorTransformation.UpdateViewPort();

                VectorTransformation.UpdateCameraBasicVectors();
                VectorTransformation.TransformVectors((float)angleX, (float)angleY, (float)angleZ, (float)scale, movement[0], movement[1], movement[2]);

                renderer.DrawModel();
            };
        }

        private void window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.I:
                    angleX += 0.1;
                    break;
                case System.Windows.Input.Key.K:
                    angleX -= 0.1;
                    break;
                case System.Windows.Input.Key.J:
                    angleY += 0.1;
                    break;
                case System.Windows.Input.Key.L:
                    angleY -= 0.1;
                    break;
                case System.Windows.Input.Key.U:
                    angleZ += 0.1;
                    break;
                case System.Windows.Input.Key.O:
                    angleZ -= 0.1;
                    break;
                case System.Windows.Input.Key.T:
                    scale += _scaleDiff;
                    break;
                case System.Windows.Input.Key.G:
                    scale -= _scaleDiff;
                    break;
                case System.Windows.Input.Key.D:
                    movement[0] += 1;

                    VectorTransformation.TransformVectors((float)_angleX, (float)_angleY, (float)_angleZ, (float)_scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.A:
                    movement[0] -= 1;

                    VectorTransformation.TransformVectors((float)_angleX, (float)_angleY, (float)_angleZ, (float)_scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.Q:
                    movement[1] -= 1;

                    VectorTransformation.TransformVectors((float)_angleX, (float)_angleY, (float)_angleZ, (float)_scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.E:
                    movement[1] += 1;

                    VectorTransformation.TransformVectors((float)_angleX, (float)_angleY, (float)_angleZ, (float)_scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.W:
                    movement[2] += 1;

                    VectorTransformation.TransformVectors((float)_angleX, (float)_angleY, (float)_angleZ, (float)_scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.S:
                    movement[2] -= 1;

                    VectorTransformation.TransformVectors((float)_angleX, (float)_angleY, (float)_angleZ, (float)_scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad6:
                    camera_movement[0] += 1;
                    VectorTransformation.eye.X = camera_movement[0];

                    VectorTransformation.TransformVectors((float)angleX, (float)angleY, (float)angleZ, (float)scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad4:
                    camera_movement[0] -= 1;
                    VectorTransformation.eye.X = camera_movement[0];

                    VectorTransformation.TransformVectors((float)angleX, (float)angleY, (float)angleZ, (float)scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad7:
                    camera_movement[1] -= 1;
                    VectorTransformation.eye.Y = camera_movement[1];

                    VectorTransformation.TransformVectors((float)angleX, (float)angleY, (float)angleZ, (float)scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad9:
                    camera_movement[1] += 1;
                    VectorTransformation.eye.Y = camera_movement[1];

                    VectorTransformation.TransformVectors((float)angleX, (float)angleY, (float)angleZ, (float)scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad8:
                    camera_movement[2] += 1;
                    VectorTransformation.eye.Z = camera_movement[2];

                    VectorTransformation.TransformVectors((float)angleX, (float)angleY, (float)angleZ, (float)scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad5:
                    camera_movement[2] -= 1;
                    VectorTransformation.eye.Z = camera_movement[2];

                    VectorTransformation.TransformVectors((float)angleX, (float)angleY, (float)angleZ, (float)scale, movement[0], movement[1], movement[2]);
                    renderer.DrawModel();
                    break;
            }
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            VectorTransformation.width = (float)img.ActualWidth;
            VectorTransformation.height = (float)img.ActualHeight;

            VectorTransformation.UpdateViewPort();
            VectorTransformation.TransformVectors((float)angleX, (float)angleY, (float)angleZ, (float)scale, movement[0], movement[1], movement[2]);
            
            renderer.DrawModel();
        }
    }
}
