using AForge.Imaging;
using AForge.Imaging.Filters;
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
        GrayScale
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
            DoGrayScale(r,g,b);
        }

        /// <summary>
        /// Blur
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            CurrentMode = Mode.Blur;
            DoBlur();
        }

        private void DoBlur()
        {
            AForge.Imaging.Filters.Blur blur = new Blur();
            pictureBox1.Image = blur.Apply(image).ToManagedImage();
        }

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
                default:
                    break;
            }
        }
    }
}
