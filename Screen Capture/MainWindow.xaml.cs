using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Net;
using System.Diagnostics;

namespace Screen_Capture
{
    public partial class MainWindow : Window
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private IntPtr hookID = IntPtr.Zero;
        private List<BitmapImage> capturedImages = new List<BitmapImage>();
        private bool captureModeActive = false; // 캡쳐모드 활성화 여부를 나타내는 변수

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]

        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.Dll")]
        static extern int PostMessage(IntPtr hWnd, UInt32 msg, int wParam, int lParam);

        private const UInt32 WM_CLOSE = 0x0010;


        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private LowLevelMouseProc hookCallback;

        public MainWindow()
        {
            InitializeComponent();
            hookCallback = new LowLevelMouseProc(MouseHookCallback);
        }

        private void CaptureMode_Click(object sender, RoutedEventArgs e)
        {
            if (!captureModeActive)
            {
                captureModeActive = true;
                ShowAutoClosingMessageBox("캡쳐모드로 전환합니다", "알림");
                SetCaptureMode();
            }
            else
            {
                captureModeActive = false;
                ShowAutoClosingMessageBox("캡쳐모드를 종료합니다", "알림");
                UnhookWindowsHookEx(hookID);
            }
        }

        private void SetCaptureMode()
        {
            hookID = SetHook(hookCallback);
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (ProcessModule curModule = Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                // Left mouse button down event
                CaptureScreen();
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        private void CaptureScreen()
        {
            this.Hide(); // Hide the window during capture

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

                // 캡쳐모드에서만 바로 저장
                if (captureModeActive)
                {
                    SaveCapturedImage();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("캡처 중 예외 발생: " + ex.Message);
            }

            this.Show(); // Show the window after capture
        }

        private void SaveCapturedImage()
        {
            if (capturedImages.Count > 0)
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + DateTime.Now.ToString("yyyyMMdd");
                DirectoryInfo di = new DirectoryInfo(desktopPath);
                if (!di.Exists) di.Create();

                string savePath = desktopPath + @"\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";

                try
                {
                    Bitmap bitmap = ConvertBitmapImageToBitmap(capturedImages[capturedImages.Count - 1]);
                    bitmap.Save(savePath, ImageFormat.Png);
                    System.Windows.MessageBox.Show($"이미지가 성공적으로 저장되었습니다: {savePath}", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("예외 발생: " + ex.Message);
                }
            }
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

        private Bitmap ConvertBitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UnhookWindowsHookEx(hookID);
            base.OnClosing(e);
        }

        //캡쳐모드 전환시 
        public void ShowAutoClosingMessageBox(string message, string caption)
        {
            var timer = new System.Timers.Timer(1500) { AutoReset = false };
            timer.Elapsed += delegate
            {
                IntPtr hWnd = FindWindowByCaption(IntPtr.Zero, caption);
                if (hWnd.ToInt32() != 0) PostMessage(hWnd, WM_CLOSE, 0, 0);
            };
            timer.Enabled = true;
            System.Windows.MessageBox.Show(message, caption);
        }
    }
}