using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AForgeTest.First
{
    public enum Mode
    {
        Blur,
        GrayScale,
        FindCircle,
        FindObjects
    }
    public partial class Form1 : Form
    {
        UnmanagedImage image;//картиночка
        private Mode CurrentMode;
        public Form1()
        {
            InitializeComponent();

            var img1 = AForge.Imaging.Image.FromFile(@"..\..\капчи\"+numericUpDown1.Value+".png");//загружаем картинку из папки

            image = UnmanagedImage.FromManagedImage(img1);//Инициализируем
            DoGrayScale();

            //Данный код рисует на картиночке
            //Drawing.FillRectangle(image, new Rectangle(50,50,50,50), Color.Blue);
            //AForge.IntPoint point1 = new AForge.IntPoint(4, 5);
            //AForge.IntPoint point2 = new AForge.IntPoint(100, 100);
            //Drawing.Line(image, point1, point2, Color.Red);


        }


        #region методы
        private void DoBlur()
        {
            AForge.Imaging.Filters.Blur blur = new Blur();
            pictureBox1.Image = blur.Apply(image).ToManagedImage();
        }

        private void DoGrayScale(double r, double g, double b)
        {
            CurrentMode = Mode.GrayScale;
            AForge.Imaging.Filters.Grayscale gs = new Grayscale(r, g, b);
            pictureBox1.Image = gs.Apply(image).ToManagedImage();
        }
        /// <summary>
        /// Загрейскелить картинку значениями из ползунков
        /// </summary>
        private void DoGrayScale()
        {
            double r = (hScrollBar1.Value / 100.0);
            double g = (hScrollBar2.Value / 100.0);
            double b = (hScrollBar3.Value / 100.0);
            DoGrayScale(r, g, b);
        }
        private void FindCircle()
        {
            CurrentMode = Mode.FindCircle;
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            var img1 = AForge.Imaging.Image.FromFile(@"..\..\капчи\" + numericUpDown1.Value + ".png");//загружаем картинку из папки
            
            // locate objects using blob counter
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(img1);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            // create Graphics object to draw on the image and a pen
            Graphics g = Graphics.FromImage(img1);
            Pen redPen = new Pen(Color.Red, 2);
            // check each object and draw circle around objects, which
            // are recognized as circles
            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);

                AForge.Point center;
                float radius;

                if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                {
                    g.DrawEllipse(redPen,
                        (int)(center.X - radius),
                        (int)(center.Y - radius),
                        (int)(radius * 2),
                        (int)(radius * 2));
                }
            }
            redPen.Dispose();
            g.Dispose();

            pictureBox1.Image = img1;
        }
        private void FindObjects()
        {
            CurrentMode = Mode.FindObjects;
            var bitmap = AForge.Imaging.Image.FromFile(@"..\..\капчи\" + numericUpDown1.Value + ".png");//загружаем картинку из папки

            // lock image
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, bitmap.PixelFormat);

            // step 1 - turn background to black
            ColorFiltering colorFilter = new ColorFiltering();

            colorFilter.Red = new IntRange(0, 64);
            colorFilter.Green = new IntRange(0, 64);
            colorFilter.Blue = new IntRange(0, 64);
            colorFilter.FillOutsideRange = false;

            colorFilter.ApplyInPlace(bitmapData);


            // step 2 - locating objects
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 5;
            blobCounter.MinWidth = 5;

            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            bitmap.UnlockBits(bitmapData);


            // step 3 - check objects' type and highlight
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            Graphics g = Graphics.FromImage(bitmap);
            Pen redPen = new Pen(Color.Red, 2);
            Pen yellowPen = new Pen(Color.Yellow, 2);
            Pen greenPen = new Pen(Color.Green, 2);
            Pen bluePen = new Pen(Color.Blue, 2);

            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints =
                    blobCounter.GetBlobsEdgePoints(blobs[i]);

                AForge.Point center;
                float radius;

                if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                {
                    g.DrawEllipse(yellowPen,
                        (float)(center.X - radius), (float)(center.Y - radius),
                        (float)(radius * 2), (float)(radius * 2));
                }
                else
                {
                    List<IntPoint> corners;
                    List<PointF> cornersF = new List<PointF>();


                    if (shapeChecker.IsQuadrilateral(edgePoints, out corners))
                    {
                        foreach (var corner in corners)
                        {
                            cornersF.Add(new PointF(corner.X, corner.Y));
                        }
                        if (shapeChecker.CheckPolygonSubType(corners) ==
                            PolygonSubType.Rectangle)
                        {
                            g.DrawPolygon(greenPen, cornersF.ToArray());
                        }
                        else
                        {
                            g.DrawPolygon(bluePen, cornersF.ToArray());
                        }
                    }
                    else
                    {
                        cornersF.Clear();
                        foreach (var corner in corners)
                        {
                            cornersF.Add(new PointF(corner.X, corner.Y));
                        }
                        corners = PointsCloud.FindQuadrilateralCorners(edgePoints);
                        g.DrawPolygon(redPen, cornersF.ToArray());
                    }
                }
            }

            redPen.Dispose();
            greenPen.Dispose();
            bluePen.Dispose();
            yellowPen.Dispose();
            g.Dispose();
            pictureBox1.Image = bitmap;
        }
        #endregion

        #region события

        /// <summary>
        /// GrayScale
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            DoGrayScale();
        }

        /// <summary>
        /// Загрейскелить картинку))
        /// </summary>
        /// <param name="r">Красный</param>
        /// <param name="g">Зелёный</param>
        /// <param name="b">Синий</param>


        /// <summary>
        /// Blur
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            CurrentMode = Mode.Blur;
            DoBlur();
        }


        #region скролы

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            DoGrayScale();
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            DoGrayScale();
        }

        private void hScrollBar3_Scroll(object sender, ScrollEventArgs e)
        {
            DoGrayScale();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            var img1 = AForge.Imaging.Image.FromFile(@"..\..\капчи\" + numericUpDown1.Value + ".png");//загружаем картинку из папки

            image = UnmanagedImage.FromManagedImage(img1);//Инициализируем
            switch (CurrentMode)
            {
                case Mode.Blur:
                    DoBlur();
                    break;
                case Mode.GrayScale:
                    DoGrayScale();
                    break;
                case Mode.FindCircle:
                    FindCircle();
                    break;
                case Mode.FindObjects:
                    FindObjects();
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Original
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = image.ToManagedImage();
        }


        /// <summary>
        /// Find circle
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            FindCircle();
        }


        /// <summary>
        /// FindCircleNew
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            FindObjects();
        }
        #endregion



    }
}
