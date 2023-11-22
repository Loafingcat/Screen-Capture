using System;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CaptureScreenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 주 화면의 크기 정보 읽기
                Screen scr = Screen.PrimaryScreen;
                Rectangle rect = scr.Bounds;

                Bitmap bmp = new Bitmap(rect.Width, rect.Height);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
                }

                // 이미지 표시
                BitmapImage bitmapImage = ConvertToBitmapImage(bmp);
                previewImage.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                // 예외 메시지 출력
                System.Windows.MessageBox.Show("캡처 중 예외 발생: " + ex.Message);
            }
        }

        private BitmapImage ConvertToBitmapImage(Bitmap bitmap)
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
            // previewImage의 Source 속성이 null이 아닌 경우에만 이미지 저장
            if (previewImage.Source != null)
            {
                // 이미지를 파일로 저장할 경로를 지정
                string savePath = "C:\\Users\\bj\\Desktop\\화면캡쳐테스트\\image.png";

                try
                {
                    // 이미지 저장을 위해 previewImage의 Source에서 BitmapImage 객체를 가져옴
                    BitmapImage bitmapImage = (BitmapImage)previewImage.Source;
                    // PngBitmapEncoder 클래스를 사용해서 이미지를 PNG 형식으로 저장할 준비
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    // BitmapImage를 BitmapFrame으로 변환하고 encoder에 추가
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                    // 이미지를 파일로 저장하기 위해 FileStream 객체를 사용, 파일 스트림을 열고 이미지 저장
                    using (var fileStream = new FileStream(savePath, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    // 예외 메시지 출력
                    System.Windows.MessageBox.Show("예외 발생: " + ex.Message);
                }
            }
        }
    }
}
