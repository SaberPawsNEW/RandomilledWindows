using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ChilledWindows.Properties;

namespace ChilledWindows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Type typeFromProgID = Type.GetTypeFromProgID("Shell.Application");
            object target = Activator.CreateInstance(typeFromProgID);
            typeFromProgID.InvokeMember("MinimizeAll", BindingFlags.InvokeMethod, null, target, null);
            Thread.Sleep(300);

            this.screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            this.screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            Bitmap bitmap = new Bitmap(this.screenWidth, this.screenHeight);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }

            this.InitializeComponent();
            base.WindowState = WindowState.Normal;
            base.WindowStyle = WindowStyle.None;
            base.Topmost = true;
            base.WindowState = WindowState.Maximized;

            ImageSource source = this.BitmapToImageSource(bitmap);
            this.firstBg.Source = source;
            this.bg2.Source = source;
            this.bg3.Source = source;

            this.firstBg.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            this.fTransformGroup.Children.Add(this.fFlipTrans);
            this.fTransformGroup.Children.Add(this.fRotateTrans);
            this.firstBg.RenderTransform = this.fTransformGroup;

            this.bg2.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            this.bg2.RenderTransform = this.FlipTrans1;

            this.bg3.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            this.bg3.RenderTransform = this.FlipTrans2;

            this.twoGrid.RenderTransformOrigin = new System.Windows.Point(0.0, 0.0);
            this.gTransGroup.Children.Add(this.gTransTransform);
            this.gTransGroup.Children.Add(this.gScaleTransform);
            this.twoGrid.RenderTransform = this.gTransGroup;

            File.WriteAllBytes("chilledwindows.mp4", ChilledWindows.Properties.Resources.Chilled_Windows);
            this.mediaElement.Source = new Uri("chilledwindows.mp4", UriKind.Relative);

            this.dt.Tick += this.Dt_Tick;
            this.dt.Interval = TimeSpan.FromTicks(1);
            this.dt.Start();
        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            this.frameIndex = (int)Math.Floor(this.mediaElement.Position.TotalMilliseconds / 33.33333);
            this.label.Content = "Frame:" + this.frameIndex;

            if (this.frameIndex == 438)
            {
                this.refreshFirstFlips = false;
                this.firstBg.Visibility = Visibility.Hidden;
                this.twoGrid.Visibility = Visibility.Visible;
            }
            if (this.frameIndex == 585)
            {
                this.refreshSecondFlips = false;
            }

            // --- HIGH INTENSITY RUBBER-BANDING SHRINK ANIMATION ---
            if (this.frameIndex == 622 && !hasShrunk)
            {
                hasShrunk = true;
                this.bg.Visibility = Visibility.Hidden;

                double originalW = (double)this.screenWidth * 0.13817330210772832;
                double originalH = (double)this.screenHeight * 0.3541666666666667;

                double finalTargetWidth = originalW * 3.0;
                double finalTargetHeight = originalH * 3.0;

                double maxValidX = this.screenWidth - finalTargetWidth;
                double maxValidY = this.screenHeight - finalTargetHeight;

                double targetX = maxValidX > 0 ? rand.NextDouble() * maxValidX : 0;
                double targetY = maxValidY > 0 ? rand.NextDouble() * maxValidY : 0;

                DoubleAnimationUsingKeyFrames glitchX = new DoubleAnimationUsingKeyFrames();
                DoubleAnimationUsingKeyFrames glitchY = new DoubleAnimationUsingKeyFrames();
                DoubleAnimationUsingKeyFrames glitchScaleX = new DoubleAnimationUsingKeyFrames();
                DoubleAnimationUsingKeyFrames glitchScaleY = new DoubleAnimationUsingKeyFrames();

                // Increased keyframe steps to pack more erratic jumps into the exact same 500ms time block
                int totalSteps = 45;
                double totalDurationMs = 500.0;

                // Initial frame state boundaries
                glitchX.KeyFrames.Add(new LinearDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                glitchY.KeyFrames.Add(new LinearDoubleKeyFrame(0.1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                glitchScaleX.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                glitchScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));

                for (int i = 1; i < totalSteps; i++)
                {
                    double normalProgress = (double)i / totalSteps;
                    double currentTimeMs = normalProgress * totalDurationMs;
                    double glitchyProgress;

                    // Generate a random roll to determine type of glitch injection
                    double glitchChance = rand.NextDouble();

                    if (glitchChance < 0.15)
                    {
                        // Hard Rubber Band: Instantly snap completely back to the original starting values
                        glitchyProgress = 0.0;
                    }
                    else if (glitchChance < 0.25)
                    {
                        // Desync Leap: Teleport heavily forward into the final animation stage ahead of schedule
                        glitchyProgress = Math.Min(1.0, normalProgress + 0.4);
                    }
                    else
                    {
                        // Standard Heavy Jitter: Massively offset timeline progress forwards or backwards
                        glitchyProgress = normalProgress + ((rand.NextDouble() * 0.6) - 0.3);
                        glitchyProgress = Math.Max(0.0, Math.Min(1.0, glitchyProgress)); // Clamp safety bounds
                    }

                    // Linear matrix updates driven by the warped timeline progression index
                    double currentX = 0.9 + (targetX - 0.9) * glitchyProgress;
                    double currentY = 0.1 + (targetY - 0.1) * glitchyProgress;
                    double currentScaleX = 1.0 - (0.7 * glitchyProgress);
                    double currentScaleY = 1.0 - (0.7 * glitchyProgress);

                    glitchX.KeyFrames.Add(new LinearDoubleKeyFrame(currentX, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(currentTimeMs))));
                    glitchY.KeyFrames.Add(new LinearDoubleKeyFrame(currentY, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(currentTimeMs))));
                    glitchScaleX.KeyFrames.Add(new LinearDoubleKeyFrame(currentScaleX, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(currentTimeMs))));
                    glitchScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(currentScaleY, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(currentTimeMs))));
                }

                // Hard-lock the final keyframe at precisely 500ms to guarantee termination alignment 
                glitchX.KeyFrames.Add(new LinearDoubleKeyFrame(targetX, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(totalDurationMs))));
                glitchY.KeyFrames.Add(new LinearDoubleKeyFrame(targetY, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(totalDurationMs))));
                glitchScaleX.KeyFrames.Add(new LinearDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(totalDurationMs))));
                glitchScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(totalDurationMs))));

                this.gTransTransform.BeginAnimation(TranslateTransform.XProperty, glitchX);
                this.gTransTransform.BeginAnimation(TranslateTransform.YProperty, glitchY);
                this.gScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, glitchScaleX);
                this.gScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, glitchScaleY);
            }

            if (this.frameIndex == 665)
            {
                this.twoGrid.Visibility = Visibility.Hidden;
            }
            if (this.frameIndex == 1260)
            {
                File.Delete("chilledwindows.mp4");
                Application.Current.Shutdown();
            }

            // Only starts rotating when frame index hits or passes 286
            if (this.frameIndex >= 286)
            {
                this.fRotateTrans.Angle = (rand.NextDouble() * 720.0) - 360.0;
            }

            if (this.refreshFirstFlips)
            {
                if (rand.Next(0, 2) == 0)
                {
                    this.fFlipTrans.ScaleX = (this.fFlipTrans.ScaleX == -1.0) ? 1.0 : -1.0;
                }
            }
            else if (this.refreshSecondFlips)
            {
                if (rand.Next(0, 2) == 0)
                {
                    this.FlipTrans1.ScaleX = (this.FlipTrans1.ScaleX == -1.0) ? 1.0 : -1.0;
                }
                if (rand.Next(0, 2) == 0)
                {
                    this.FlipTrans2.ScaleX = (this.FlipTrans2.ScaleX == -1.0) ? 1.0 : -1.0;
                }
            }
        }
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            BitmapImage result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Bmp);
                memoryStream.Position = 0L;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                result = bitmapImage;
            }
            return result;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                this.fFlipTrans.ScaleX = (this.fFlipTrans.ScaleX == -1.0) ? 1.0 : -1.0;
            }
        }
        private TransformGroup fTransformGroup = new TransformGroup();
        private TransformGroup gTransGroup = new TransformGroup();
        private TranslateTransform gTransTransform = new TranslateTransform();
        private ScaleTransform gScaleTransform = new ScaleTransform();
        private ScaleTransform fFlipTrans = new ScaleTransform();
        private ScaleTransform FlipTrans1 = new ScaleTransform();
        private ScaleTransform FlipTrans2 = new ScaleTransform();
        private RotateTransform fRotateTrans = new RotateTransform();
        private int screenWidth;
        private int screenHeight;
        private DispatcherTimer dt = new DispatcherTimer();
        private Random rand = new Random();
        private bool hasShrunk = false;
        private int flipIndex;
        private int flipIndex1;
        private int flipIndex2;
        private int frameIndex;
        private bool refreshFirstFlips = true;
        private bool refreshSecondFlips = true;
    }
}