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
        }

        private void BtnSaveModified_Click(object sender, RoutedEventArgs e)
        {
            System.IO.Directory.CreateDirectory(resultsFolder);
            sourceBitmap.SaveBitmap(resultsFolder + "kaynak.bmp");
            modifiedBitmap.SaveBitmap(resultsFolder + "gizli.bmp");
            SaveLabels();
        }

        private void TbInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbInput.Text.Length > sourceBitmap.GetMaxCapacity())
                tbInput.Text = tbInput.Text.Substring(0, tbInput.Text.Length - 1);
            CalculateLabels();
        }
    }
}
