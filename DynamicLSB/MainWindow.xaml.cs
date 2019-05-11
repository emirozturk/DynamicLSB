using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        OpenFileDialog ofd = new OpenFileDialog();
        StegoBitmap sourceBitmap;
        StegoBitmap modifiedBitmap;
        string resultsFolder = "/Sonuclar/";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLoadSource_Click(object sender, RoutedEventArgs e)
        {
            ofd.Filter = "Jpeg Dosyası|*.jpg|Png Dosyası|*.png|Bmp Dosyası|*.bmp";
            ofd.FileOk += Ofd_FileOk;
            ofd.ShowDialog();
        }

        private void Ofd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sourceBitmap = new StegoBitmap(ofd.FileName);
            imgSource.Source = sourceBitmap.GetImage();
            if (Stego.HasHiddenValue(sourceBitmap))
                tbInput.Text = Stego.GetHiddenValue(sourceBitmap);
            else
                CalculateLabels();
        }

        private void CalculateLabels()
        {
            lblFileSize.Content = sourceBitmap.GetFileSize();
            lblMaxCapacity.Content = sourceBitmap.GetMaxCapacity();
            lblUsedCapacity.Content = tbInput.Text.Length;
            lblRemainingCapacity.Content = (sourceBitmap.GetMaxCapacity() - tbInput.Text.Length);
            lblTotalWrittenBits.Content = tbInput.Text.Length * 8;
            lblChangeInR.Content = Stego.CalculateChange(sourceBitmap.GetRinBytes(), Stego.StringToByteArray(tbInput.Text));
            lblChangeInR.Content = Stego.CalculateChange(sourceBitmap.GetGinBytes(), Stego.StringToByteArray(tbInput.Text));
            lblChangeInR.Content = Stego.CalculateChange(sourceBitmap.GetBinBytes(), Stego.StringToByteArray(tbInput.Text));
        }

        private void SaveLabels()
        {
            List<string> output = new List<string>();
            output.Add(lblFileSize.Content.ToString());
            output.Add(lblMaxCapacity.Content.ToString());
            output.Add(lblUsedCapacity.Content.ToString());
            output.Add(lblRemainingCapacity.Content.ToString());
            output.Add(lblTotalWrittenBits.Content.ToString());
            output.Add(lblChangeInR.Content.ToString());
            output.Add(lblChangeInR.Content.ToString());
            output.Add(lblChangeInR.Content.ToString());
            System.IO.File.WriteAllLines(resultsFolder + "sonuclar.txt", output);
        }

        private void BtnHide_Click(object sender, RoutedEventArgs e)
        {
            Channel channel;
            if (cmbChannel.SelectedValue.ToString() == "Otomatik")
            {
                int changeInRed = Stego.CalculateChange(sourceBitmap.GetRinBytes(), Stego.StringToByteArray(tbInput.Text));
                int changeInGreen = Stego.CalculateChange(sourceBitmap.GetGinBytes(), Stego.StringToByteArray(tbInput.Text));
                int changeInBlue = Stego.CalculateChange(sourceBitmap.GetBinBytes(), Stego.StringToByteArray(tbInput.Text));
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
            imgModified.Source = modifiedBitmap.GetImage();
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
