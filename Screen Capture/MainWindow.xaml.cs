using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Screen_Capture
{
    public partial class MainWindow : Window
    {
        private List<BitmapImage> capturedImages = new List<BitmapImage>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CaptureScreenButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide(); //숨기는 코드

            try
            {
                Screen scr = Screen.PrimaryScreen;
                Rectangle rect = scr.Bounds;
                Bitmap bmp = new Bitmap(rect.Width, rect.Height);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
                }

                BitmapImage bitmapImage = ConvertToBitmapImage(bmp);
                previewImage.Source = bitmapImage;

                capturedImages.Add(bitmapImage);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("캡처 중 예외 발생: " + ex.Message);
            }
            this.Show();//보이게
        }

        private BitmapImage ConvertToBitmapImage(Bitmap bitmap)//캡쳐 부분에선 bitmap을 bitmapimage로 변환하여 캡쳐된 화면을 보여줌
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (capturedImages.Count > 0)
            {
                //폴더 없을 경우 폴더 생성일로 폴더 생성
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + DateTime.Now.ToString("yyyyMMdd");
                DirectoryInfo di = new DirectoryInfo(desktopPath);
                if (!di.Exists) di.Create();

                string savePath = desktopPath + @"\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";

                try
                {
                   
                    Bitmap bitmap = ConvertBitmapImageToBitmap(capturedImages[capturedImages.Count - 1]);//마지막으로 저장된 항목 찾기
                    bitmap.Save(savePath, ImageFormat.Png);
                    System.Windows.MessageBox.Show($"이미지가 성공적으로 저장되었습니다: {savePath}", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("예외 발생: " + ex.Message);
                }
            }
            else
            {
                // 저장할 이미지가 없다는 메시지 출력
                System.Windows.MessageBox.Show("저장할 이미지가 없습니다.", "경고", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private Bitmap ConvertBitmapImageToBitmap(BitmapImage bitmapImage)//저장 부분에선 bitmapimage를 다시 bitmap으로 돌려서 저장해줌
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }
    }
}