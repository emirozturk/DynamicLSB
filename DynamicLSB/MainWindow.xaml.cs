using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DynamicLSB
{
    public partial class MainWindow : Window
    {
        readonly OpenFileDialog ofd = new OpenFileDialog();
        readonly string resultsFolder = "Sonuclar/";
        StegoBitmap sourceBitmap;
        StegoBitmap modifiedBitmap;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLoadSource_Click(object sender, RoutedEventArgs e)
        {
            ofd.Filter = "Jpeg Dosyası|*.jpg|Png Dosyası|*.png|Bmp Dosyası|*.bmp|Gif Dosyası|*.gif";
            ofd.FileOk += Ofd_FileOk;
            ofd.ShowDialog();
        }

        private void Ofd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sourceBitmap = new StegoBitmap(ofd.FileName);
            imgSource.Source = GetSource(sourceBitmap.GetImage());
            Channel? result = Stego.HasHiddenValue(sourceBitmap);
            if (result.HasValue)
                tbInput.Text = Stego.GetHiddenValue(sourceBitmap,result.Value);
            else
                CalculateLabels();
        }

        private ImageSource GetSource(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        private void CalculateLabels()
        {
            lblFileSize.Content = sourceBitmap.FileSize;
            lblMaxCapacity.Content = sourceBitmap.GetMaxCapacity();
            lblUsedCapacity.Content = tbInput.Text.Length;
            lblRemainingCapacity.Content = (sourceBitmap.GetMaxCapacity() - tbInput.Text.Length);
            lblTotalWrittenBits.Content = tbInput.Text.Length * 8;
            lblChangeInR.Content = Stego.CalculateChange(sourceBitmap.RedChannel, Stego.StringToByteArray(tbInput.Text));
            lblChangeInG.Content = Stego.CalculateChange(sourceBitmap.GreenChannel, Stego.StringToByteArray(tbInput.Text));
            lblChangeInB.Content = Stego.CalculateChange(sourceBitmap.BlueChannel, Stego.StringToByteArray(tbInput.Text));
        }

        private void SaveLabels()
        {
            List<string> output = new List<string>
            {
                "DosyaBoyutu: "+lblFileSize.Content.ToString(),
                "Maksimum Yazılabilecek Byte: "+lblMaxCapacity.Content.ToString(),
                "Kullanılan Byte Sayısı: "+lblUsedCapacity.Content.ToString(),
                "Kalan Byte Sayısı: "+lblRemainingCapacity.Content.ToString(),
                "-----------------------------------------------------------------",
                "Toplam yazılan Bit Sayısı: " +lblTotalWrittenBits.Content.ToString(),
                "R Kanalındaki değişim (Bit): "+lblChangeInR.Content.ToString(),
                "G Kanalındaki değişim (Bit): "+lblChangeInG.Content.ToString(),
                "B Kanalındaki değişim (Bit): "+lblChangeInB.Content.ToString()
            };
            System.IO.File.WriteAllLines(resultsFolder + "sonuclar.txt", output);
        }

        private void BtnHide_Click(object sender, RoutedEventArgs e)
        {
            Channel channel;
            if (((ComboBoxItem)cmbChannel.SelectedItem).Content.ToString() == "Otomatik")
            {
                int changeInRed = Stego.CalculateChange(sourceBitmap.RedChannel, Stego.StringToByteArray(tbInput.Text));
                int changeInGreen = Stego.CalculateChange(sourceBitmap.GreenChannel, Stego.StringToByteArray(tbInput.Text));
                int changeInBlue = Stego.CalculateChange(sourceBitmap.BlueChannel, Stego.StringToByteArray(tbInput.Text));
                int min = changeInRed;
                if (changeInGreen < min) min = changeInGreen;
                if (changeInBlue < min) min = changeInBlue;
                if (min == changeInRed) channel = Channel.R;
                else if (min == changeInGreen) channel = Channel.G;
                else channel = Channel.B;
            }
            else
                channel = (Channel)(cmbChannel.SelectedIndex - 1);
            modifiedBitmap = new StegoBitmap(Stego.Hide(sourceBitmap, tbInput.Text, channel));
            imgModified.Source = GetSource(modifiedBitmap.GetImage());
            DrawHistogram(sourceBitmap, modifiedBitmap,tbInput.Text.Length*8+200);
        }

        private void DrawHistogram(StegoBitmap sourceBitmap, StegoBitmap modifiedBitmap,int count)
        {
            DrawToCanvas(imgRS, sourceBitmap.RedChannel, System.Windows.Media.Colors.PaleVioletRed, count);
            DrawToCanvas(imgRM, modifiedBitmap.RedChannel, System.Windows.Media.Colors.DarkRed, count);

            DrawToCanvas(imgGS, sourceBitmap.GreenChannel, System.Windows.Media.Colors.LightGreen, count);
            DrawToCanvas(imgGM, modifiedBitmap.GreenChannel, System.Windows.Media.Colors.DarkGreen, count);

            DrawToCanvas(imgBS, sourceBitmap.BlueChannel, System.Windows.Media.Colors.LightBlue, count);
            DrawToCanvas(imgBM, modifiedBitmap.BlueChannel, System.Windows.Media.Colors.DarkBlue, count);
        }

        private void DrawToCanvas(Canvas img, byte[] source,System.Windows.Media.Color color1, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = img.Width / count;
                rectangle.Height = source[i] * img.Height / 255;
                rectangle.Fill = new SolidColorBrush() { Color = color1, Opacity = 1 };
                img.Children.Add(rectangle);
                Canvas.SetBottom(rectangle, 0);
                Canvas.SetLeft(rectangle, i * img.Width / count);
            }
        }

        private void BtnSaveModified_Click(object sender, RoutedEventArgs e)
        {
            System.IO.Directory.CreateDirectory(resultsFolder);
            sourceBitmap.SaveBitmap(resultsFolder + "kaynak.bmp");
            modifiedBitmap.SaveBitmap(resultsFolder + "gizli.bmp");
            SaveLabels();
            SaveHistograms();
        }

        private void SaveHistograms()
        {
            SaveCanvas(imgRS, "rs.png");
            SaveCanvas(imgRM, "rm.png");
            SaveCanvas(imgGS, "gs.png");
            SaveCanvas(imgGM, "gm.png");
            SaveCanvas(imgBS, "bs.png");
            SaveCanvas(imgBM, "bm.png");
        }

        private void SaveCanvas(Canvas canvas,string fileName)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, 300d, 300d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(canvas);
            var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, (int)canvas.Width, (int)canvas.Height));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(crop));

            using (var fs = System.IO.File.OpenWrite(resultsFolder+fileName))
            {
                pngEncoder.Save(fs);
            }
        }

        private void TbInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbInput.Text.Length > sourceBitmap.GetMaxCapacity())
                tbInput.Text = tbInput.Text.Substring(0, tbInput.Text.Length - 1);
            CalculateLabels();
        }
    }
}
