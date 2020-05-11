using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImageProcessing.Views;
using ImageProcessing.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Runtime.InteropServices;
using ImageProcessorLib;
using Microsoft.Win32;
using ImageProcLibOpenGL.Effects;

namespace ImageProcessing.ViewModels
{
    public enum ContrastType
    {
        RMS,
        Weber,
        Michelson
    }

    public enum ProcMethod
    {
        Canny,
        AbsDiff,
        GrabCut,
        BGSub,
        FancyColor,
        Glare,
        ImageProcLib,
        SemiInverted,
        Gamma,
        WhiteDiamond,
        DarkChannel,
        CLAHE
    }

    public enum ResultStatus
    {
        SUCCESS = 0,
        ERROR
    }

    public class Result
    {
        public ResultStatus Status;
        public string Message;
        public object Value;
        public Result(ResultStatus status, string message, object value = null)
        {
            Status = status;
            Message = message;
            Value = value;
        }
    }

    public class ControlViewModel : ViewModelBase
    {
        public ControlViewModel()
        {
            base.DisplayName = "ControlViewModel";

            CommandSettings = new RelayCommand(param => Settings());
            CommandClearLineMarkers = new RelayCommand(param => RemoveLineMarkers());
            CommandOpenFile = new RelayCommand(param => OpenFile());
            CommandGPUProcessing = new RelayCommand(param => GPUProcessing());

            //Method = "FancyColor";
            ProcMethods = new ObservableCollection<string>() { "Canny", "AbsDiff", "GrabCut", "BGSub", "FancyColor",
                "Glare", "ImageProcLib", "SemiInverted", "Gamma", "WhiteDiamond", "DarkChannel", "CLAHE" };
            Method = ProcMethods[0];
            UsingGamma = true;

            postProcessing = new PostProcessing();
            postProcessing.AddProcessingFinishedSubscriber(new ProcessingFinishedHandler(PostProcessingFinished));
        }

        PostProcessing postProcessing;
        ImageAnalyzer_Pearl imageAnalyzer = new ImageAnalyzer_Pearl();
        //ImageAnalyzer imageAnalyzer = new ImageAnalyzer_FancyColor();

        System.Drawing.Bitmap bmB = null;
        System.Drawing.Bitmap bmF = null;
        System.Drawing.Bitmap dstBmp = null;

        Uri uri;
        string name;

        #region property
        public RelayCommand CommandSettings { get; set; }
        public RelayCommand CommandClearLineMarkers { get; set; }
        public RelayCommand CommandOpenFile { get; set; }
        public RelayCommand CommandGPUProcessing { get; set; }

        WriteableBitmap _imageOrig;
        public WriteableBitmap ImageOrig
        {
            get
            {
                return _imageOrig;
            }
        }

        WriteableBitmap _imageProc;
        public WriteableBitmap ImageProc
        {
            get
            {
                return _imageProc;
            }
            set
            {
                _imageProc = value;
                OnPropertyChanged("ImageProc");
            }
        }

        double width;
        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                if (width == value)
                {
                    return;
                }

                width = value;
                OnPropertyChanged("Width");
            }
        }

        double height;
        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                if (height == value)
                {
                    return;
                }

                height = value;
                OnPropertyChanged("Height");
            }
        }

        public ObservableCollection<string> ProcMethods { get; }

        string _method = string.Empty;
        public string Method
        {
            get
            {
                return _method;
            }
            set
            {
                if (_method.Equals(value))
                {
                    return;
                }
                _method = value;
                OnPropertyChanged("Method");
                if (bmF != null)
                {
                    ProcImageThread(_method);
                }
            }
        }

        bool _foregroundLineMarker = false;
        public bool ForegroundLineMarker
        {
            get
            {
                return _foregroundLineMarker;
            }
            set
            {
                _foregroundLineMarker = value;
                OnPropertyChanged("ForegroundLineMarker");
            }
        }

        bool _manualMask = false;
        public bool ManualMask
        {
            get
            {
                return _manualMask;
            }
            set
            {
                _manualMask = value;
                OnPropertyChanged("ManualMask");
            }
        }

        bool _usingGamma;
        public bool UsingGamma
        {
            get
            {
                return _usingGamma;
            }
            set
            {
                _usingGamma = value;
                OnPropertyChanged("UsingGamma");
                if(Method == Enum.GetName(typeof(ProcMethod), ProcMethod.Gamma))
                {
                    AdjustSettings();
                }
            }
        }

        bool _doingPostProc;
        public bool DoingPostProc
        {
            get
            {
                return _doingPostProc;
            }
            set
            {
                _doingPostProc = value;
                OnPropertyChanged("DoingPostProc");
                if(value == true)
                {
                    postProcessing.enqueueImage(dstBmp, ImageProcLibOpenGL.Effects.Effects.ABF);
                } else
                {
                    ProcImageThread(Method);
                }
            }
        }
        #endregion property

        System.Windows.Point currentPoint;
        List<Line> LineMarkers = new List<Line>();
        System.Drawing.Bitmap bmpMask;

        public void PostProcessingFinished(System.Drawing.Bitmap bmp)
        {
            WriteableBitmap wBmp = new WriteableBitmap(ToBitmapImage(bmp));
            wBmp.Freeze();
            App.Current.Dispatcher.Invoke(()=> ImageProc = wBmp);
        }

        void GPUProcessing()
        {
            //ImageProc = new WriteableBitmap(ToBitmapImage(bmF));
            postProcessing.enqueueImage(bmF, ImageProcLibOpenGL.Effects.Effects.ABF);
            
        }

        public void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                currentPoint = e.GetPosition((Canvas)sender);
        }

        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point pos = e.GetPosition((Canvas)sender);
                MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                Line line = DrawCanvas.NewLine(currentPoint.X, currentPoint.Y, pos.X, pos.Y, ForegroundLineMarker, mv.CanvasDraw);
                currentPoint = pos;
                LineMarkers.Add(line);
            }
        }

        void RemoveLineMarkers(int index = 0)
        {
            while (index < LineMarkers.Count)
            {
                MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mv.CanvasDraw.Children.Remove(LineMarkers[LineMarkers.Count - 1]);
                LineMarkers.RemoveAt(LineMarkers.Count - 1);
            }
        }

        void Settings()
        {
            Settings settings = new Settings();
            SettingsViewModel settingsVM = new SettingsViewModel();
            settings.DataContext = settingsVM;
            settingsVM.AddChangeSettingsSubscriber(new ChangeSettingsHandler(AdjustSettings));
            settings.ShowDialog();
        }

        void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp|All files (*.*)|*.*";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.InitialDirectory = @"C:\opencv";
            if (openFileDialog.ShowDialog() == true)
            {
                uri = new Uri(openFileDialog.FileName);

                name = System.IO.Path.GetFileNameWithoutExtension(uri.OriginalString);
                BitmapImage bitmapF = new BitmapImage(uri);
                _imageOrig = new WriteableBitmap(bitmapF);
                OnPropertyChanged("ImageOrig");

                Width = bitmapF.Width;
                Height = bitmapF.Height;

                bmF = BitmapImage2Bitmap(bitmapF);

                ProcImageThread(_method);
            }
        }

        public void AdjustSettings()
        {
            ProcImageThread(Method);
        }

        public void loadImage()
        {
            //uri = new Uri("..\\..\\..\\Images\\FancyWhite_0976.png", UriKind.Relative); // FancyWhite_0976, FancyWhite_110206685931-3, FacnyWhite_110207770032, 110206930819
            //uri = new Uri("..\\..\\..\\Images\\101.png", UriKind.Relative);
            uri = new Uri("..\\..\\..\\Images\\0000962424_CV041_StoneImage37_2018-04-02_223615.png", UriKind.Relative); //1110106267546_color30_StoneImage36_2016-04-06_140258
            uri = new Uri("..\\..\\..\\Images\\1110106267546_color30_StoneImage36_2016-04-06_140258.png", UriKind.Relative);
            uri = new Uri("..\\..\\..\\Images\\0000014138_CV022_StoneImage37_2017-03-20_122908.png", UriKind.Relative);
            //uri = new Uri("..\\..\\..\\Images\\FancyWhite_110206685931-3.png", UriKind.Relative);

            name = System.IO.Path.GetFileNameWithoutExtension(uri.OriginalString);
            BitmapImage bitmapF = new BitmapImage(uri);
            BitmapImage bitmapB = new BitmapImage(new Uri("..\\..\\..\\Images\\white_B_0.png", UriKind.Relative));
            _imageOrig = new WriteableBitmap(bitmapF);
            OnPropertyChanged("ImageOrig");

            Width = bitmapF.Width;
            Height = bitmapF.Height;

            bmF = BitmapImage2Bitmap(bitmapF);
            bmB = BitmapImage2Bitmap(bitmapB);

            //ProcImage(ref bmF, out System.Drawing.Bitmap dst);
            ProcImage5(ref bmF, ref bmB, out System.Drawing.Bitmap dst);

            _imageProc = new WriteableBitmap(ToBitmapImage(dst));
            OnPropertyChanged("ImageProc");
            //        Cv2.Dilate(iplImage, iplImage, new Mat());

            //        Image1.Source = iplImage.ToWriteableBitmap(PixelFormats.Bgr24);
        }

        void ProcImageThread(string method)
        {
            MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            WriteableBitmap tmp = SaveAsWriteableBitmap(mv.CanvasDraw);
            bmpMask = BitmapFromWriteableBitmap(tmp);
            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //{
            //    bmpMask.Save(@"C:\temp\manualmask.bmp");
            //});
            //bmpMask.Save(@"C:\temp\manualmask.bmp");

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(ProcImage_doWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ProcImage_completed);
            bw.RunWorkerAsync(method);
        }

        void ProcImage_doWork(object sender, DoWorkEventArgs e)
        {
            string method = (string)e.Argument;
            if (bmF == null)
            {
                e.Result = new Result(ResultStatus.ERROR, "Source image does not exist.");
                return;
            }
            if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.Canny))
            {
                ProcImage5(ref bmF, ref bmB, out System.Drawing.Bitmap dst);

                e.Result = new Result(ResultStatus.SUCCESS, null, dst);
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.AbsDiff))
            {
                ProcImage2(ref bmF, ref bmB, out System.Drawing.Bitmap dst);

                e.Result = new Result(ResultStatus.SUCCESS, null, dst);
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.GrabCut))
            {
                ProcImage(ref bmF, ref bmpMask, out System.Drawing.Bitmap dst);

                e.Result = new Result(ResultStatus.SUCCESS, null, dst);
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.BGSub))
            {
                ProcImage3(ref bmF, ref bmB, out System.Drawing.Bitmap dst);

                e.Result = new Result(ResultStatus.SUCCESS, null, dst);
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.FancyColor))
            {
                //fancy
                CreateObjectMask(ref bmF, out System.Drawing.Bitmap dst);

                // pearl 
                //CreateObjectMask1(ref bmF, ref bmB, out System.Drawing.Bitmap dst);

                e.Result = new Result(ResultStatus.SUCCESS, null, dst);
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.Glare))
            {
                GlareDetection(ref bmF, out System.Drawing.Bitmap dst);

                e.Result = new Result(ResultStatus.SUCCESS, null, dst);
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.ImageProcLib))
            {
                int clusters = App.appSettings.Clusters;
                CreateMask(ref bmF, ref bmB, clusters, out System.Drawing.Bitmap dst);

                e.Result = new Result(ResultStatus.SUCCESS, null, dst);
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.SemiInverted))
            {
                HazeSemiInverted(ref bmF, out System.Drawing.Bitmap dst);

                e.Result = new Result(ResultStatus.SUCCESS, null, dst);
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.DarkChannel))
            {
                DarkChannelPrior(ref bmF, out System.Drawing.Bitmap dst);

                e.Result = new Result(ResultStatus.SUCCESS, null, dst);
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.CLAHE))
            {
                ContrastLimitHE(ref bmF, out System.Drawing.Bitmap dst);

                e.Result = new Result(ResultStatus.SUCCESS, null, dst);
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.Gamma))
            {
                double gamma = App.appSettings.LevelGamma;
                double levelMax = App.appSettings.LevelMax;
                double levelMin = App.appSettings.LevelMin;
                System.Drawing.Bitmap tmp = null;
                if (UsingGamma)
                {
                    GammaAdjustment(ref bmF, gamma, levelMin, levelMax, out tmp);
                }
                else
                {
                    tmp = bmF;
                }
                if (ManualMask)
                {
                    int clusters = App.appSettings.Clusters;
                    CreateMask(ref tmp, ref bmB, clusters, out System.Drawing.Bitmap dst);
                    e.Result = new Result(ResultStatus.SUCCESS, null, dst);
                } else
                {
                    e.Result = new Result(ResultStatus.SUCCESS, null, tmp);
                }
            }
            else if (method == Enum.GetName(typeof(ProcMethod), ProcMethod.WhiteDiamond))
            {
                WhiteDiamondAnalysis();

                e.Result = new Result(ResultStatus.SUCCESS, null, bmF);
            }
        }

        void DarkChannelPrior(ref System.Drawing.Bitmap src, out System.Drawing.Bitmap dst)
        {
            dst = ImageProcessorLib.ImageProcessing.DarkChannelPrior(src);
        }

        void ContrastLimitHE(ref System.Drawing.Bitmap src, out System.Drawing.Bitmap dst)
        {
            double clipLimit = App.appSettings.ClipLimit;
            dst = ImageProcessorLib.ImageProcessing.ContrastLimitHE(src, clipLimit);
        }

        void WhiteDiamondAnalysis()
        {
            //string dir = @"C:\opencv\whitecolor\Fancy White";
            //string[] files = System.IO.Directory.GetFiles(dir, "*.bmp");
            bool white = true;
            string dir = @"C:\opencv\nonwhite";
            string[] files = System.IO.Directory.GetFiles(dir, "*stoneImage*.jpg", SearchOption.AllDirectories);
            if (white)
            {
                dir = @"C:\opencv\whitecolor\Images";
                files = System.IO.Directory.GetFiles(dir, "*.jpg", SearchOption.AllDirectories);
            }
            Console.WriteLine("total images: {0}", files.Length);
            List<System.Drawing.Bitmap> srcList = new List<System.Drawing.Bitmap>();
            List<System.Drawing.Bitmap> bgList = new List<System.Drawing.Bitmap>();
            List<string> nameList = new List<string>();
            int m = files.Length / 64;
            int mod = files.Length - 64 * m;

            long timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string fileName = "nonwhitediamond_darkchannel_hue_diff_" + timestamp.ToString() + ".csv";
            if (white)
            {
                fileName = "whitediamond_darkchannel_hue_diff_" + timestamp.ToString() + ".csv";
            }
            var filePath = dir + @"\" + fileName;

            bool testing = true;
            if (testing)
            {
                dir = @"C:\temp";
                //files = new string[1]{ "C:\\opencv\\Test\\ImageProcessing\\ImageProcessing\\Images\\FancyWhite_110206685931-3.png"};
                string fullPath = System.IO.Path.GetFullPath(uri.OriginalString);
                files = new string[1] { fullPath };
                fileName = "testing_" + timestamp.ToString() + ".csv";

                m = files.Length / 64;
                mod = files.Length - 64 * m;
            }

            bool usingGamma = true;

            // hsl data saving for debugging
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                var line = string.Format("{0},{1},{2},{3},{4},{5},{6}", "index", "mean", "std", "percentile", "name", "mean hue", "std hue");
                writer.WriteLine(line);
                for (int j = 0; j < m; j++)
                {
                    int offset = j * 64;
                    for (int i = 0; i < 64; i++)
                    {
                        BitmapImage bitmapF = new BitmapImage(new Uri(files[i + offset]));
                        System.Drawing.Bitmap bmp = BitmapImage2Bitmap(bitmapF);
                        double gamma = App.appSettings.LevelGamma;
                        double levelMax = App.appSettings.LevelMax;
                        double levelMin = App.appSettings.LevelMin;
                        System.Drawing.Bitmap tmp = null;

                        GammaAdjustment(ref bmp, gamma, levelMin, levelMax, out tmp);
                        if (usingGamma)
                        {
                            srcList.Add(tmp);
                        } else
                        {
                            srcList.Add(bmp);
                        }
                        bgList.Add(bmp);
                        nameList.Add(System.IO.Path.GetFileName(files[i + offset]));
                    }
                    imageAnalyzer.WhiteDiamondAnalysis(ref srcList, ref bgList, ref nameList, out List<Tuple<double, double, double>> mspList, out List<Tuple<double, double, double>> mspHueList);
                    for (int i = 0; i < mspList.Count(); i++)
                    {
                        if (mspHueList != null && mspHueList.Count > 0)
                        {
                            line = string.Format("{0},{1},{2},{3},{4},{5},{6}", i + offset, mspList[i].Item1, mspList[i].Item2, mspList[i].Item3, files[i + offset], mspHueList[i].Item1, mspHueList[i].Item2);
                            Console.WriteLine(line);
                            writer.WriteLine(line);
                        }
                        else
                        {
                            line = string.Format("{0},{1},{2},{3},{4}", i + offset, mspList[i].Item1, mspList[i].Item2, mspList[i].Item3, files[i + offset]);
                            Console.WriteLine(line);
                            writer.WriteLine(line);
                        }
                    }
                    srcList.Clear();
                    bgList.Clear();
                    nameList.Clear();
                }
                {
                    int offset = m * 64;
                    for (int i = 0; i < mod; i++)
                    {
                        BitmapImage bitmapF = new BitmapImage(new Uri(files[i + offset]));
                        System.Drawing.Bitmap bmp = BitmapImage2Bitmap(bitmapF);
                        double gamma = App.appSettings.LevelGamma;
                        double levelMax = App.appSettings.LevelMax;
                        double levelMin = App.appSettings.LevelMin;
                        System.Drawing.Bitmap tmp = null;

                        GammaAdjustment(ref bmp, gamma, levelMin, levelMax, out tmp);

                        if (usingGamma)
                        {
                            srcList.Add(tmp);
                        }
                        else
                        {
                            srcList.Add(bmp);
                        }
                        bgList.Add(bmp);
                        nameList.Add(System.IO.Path.GetFileName(files[i + offset]));
                    }
                    imageAnalyzer.WhiteDiamondAnalysis(ref srcList, ref bgList, ref nameList, out List<Tuple<double, double, double>> mspList, out List<Tuple<double, double, double>> mspHueList);
                    for (int i = 0; i < mspList.Count(); i++)
                    {
                        if (mspHueList != null && mspHueList.Count > 0)
                        {
                            line = string.Format("{0},{1},{2},{3},{4},{5},{6}", i + offset, mspList[i].Item1, mspList[i].Item2, mspList[i].Item3, files[i + offset], mspHueList[i].Item1, mspHueList[i].Item2);
                            Console.WriteLine(line);
                            writer.WriteLine(line);
                        }
                        else
                        { 
                            line = string.Format("{0},{1},{2},{3},{4}", i + offset, mspList[i].Item1, mspList[i].Item2, mspList[i].Item3, files[i + offset]);
                            Console.WriteLine(line);
                            writer.WriteLine(line);
                        }
                    }
                    srcList.Clear();
                    bgList.Clear();
                    nameList.Clear();
                }
            }
        }

        void GammaAdjustment(ref System.Drawing.Bitmap src, double gamma, double fmin, double fmax, out System.Drawing.Bitmap dst)
        {
            dst = null;
            ImageProcessorLib.ImageProcessing.LevelAdjustment2(ref src, out dst, gamma, fmin, fmax);
        }

        void HazeSemiInverted(ref System.Drawing.Bitmap src, out System.Drawing.Bitmap dst)
        {
            dst = null;
            dst = ImageProcessorLib.ImageProcessing.SemiInverted(src);
        }

        void CreateMask(ref System.Drawing.Bitmap src, ref System.Drawing.Bitmap bg, int clusters, out System.Drawing.Bitmap dst)
        {
            List<System.Drawing.Bitmap> srcList = new List<System.Drawing.Bitmap>() { src };
            List<System.Drawing.Bitmap> bgList = new List<System.Drawing.Bitmap>() { bg };
            List<System.Drawing.Rectangle> maskList = null;
            imageAnalyzer.Get_Description(ref srcList, ref bgList, ref maskList, clusters, out List<System.Drawing.Bitmap> clusterImgList, 
                out List<List<Tuple<double, double, double, double>>> descriptions, out List<string> comments);
            if (clusterImgList.Count > 0)
            {
                dst = clusterImgList[0];
            } else
            {
                dst = src;
            }
        }

        void ProcImage_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (((Result)e.Result).Status == ResultStatus.SUCCESS)
            {
                System.Drawing.Bitmap res = (System.Drawing.Bitmap)((Result)e.Result).Value;
                dstBmp = res;
                _imageProc = new WriteableBitmap(ToBitmapImage(res));
                OnPropertyChanged("ImageProc");
                if (DoingPostProc)
                {
                    postProcessing.enqueueImage(dstBmp, ImageProcLibOpenGL.Effects.Effects.ABF);
                }
            }
        }

        private void ProcImage(ref System.Drawing.Bitmap src, ref System.Drawing.Bitmap maskRef, out System.Drawing.Bitmap dst)
        {
            dst = null;
            Mat srcImg = BitmapConverter.ToMat(src);
            Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGRA2BGR);

            Mat srcMask = BitmapConverter.ToMat(maskRef);
            Cv2.CvtColor(srcMask, srcMask, ColorConversionCodes.BGRA2BGR);
            Cv2.ImWrite(@"C:\opencv\ImageProcessing\ImageProcessing\Images\manualmask.jpg", srcMask);
            Mat mask = new Mat(new OpenCvSharp.Size(src.Width, src.Height), MatType.CV_8UC1, 0);

            for (int i = 0; i < srcMask.Cols; i++)
            {
                for (int j = 0; j < srcMask.Rows; j++)
                {
                    if( i > srcMask.Cols / 4 && i < srcMask.Cols / 4 * 3 && j > srcMask.Rows / 4 && j < srcMask.Rows / 4 * 3)
                    {
                        mask.Set<byte>(j, i, 3);
                    } else
                    {
                        mask.Set<byte>(j, i, 0);
                    }

                    Vec3b intensity = srcMask.Get<Vec3b>(j, i);
                    if(intensity.Item1 > 40)
                    {
                        mask.Set<byte>(j, i, 1);
                    }

                    if (intensity.Item2 > 40)
                    {
                        mask.Set<byte>(j, i, 0);
                    }
                }
            }

            //dilate process 
            //Cv2.Dilate(srcImg, dstImg, new Mat());

            Mat bgdModel = new Mat();
            Mat fgdModel = new Mat();

            OpenCvSharp.Rect r = new OpenCvSharp.Rect(500, 500, (int)Width - 1000, (int)Height - 1000);
            Cv2.GrabCut(srcImg, mask, r, bgdModel, fgdModel, 3, GrabCutModes.InitWithMask);

            //for (int i = mask.Cols / 2 - 50; i < mask.Cols / 2 + 50; i++)
            //{
            //    for (int j = mask.Rows / 2 - 25; j < mask.Rows / 2 + 75; j++)
            //    {
            //        mask.Set<byte>(j, i, 1);
            //    }
            //}

            //Cv2.GrabCut(srcImg, mask, r, bgdModel, fgdModel, 1, GrabCutModes.InitWithMask);

            for (int i = 0; i < mask.Cols; i++)
            {
                for (int j = 0; j < mask.Rows; j++)
                {
                    byte e = mask.Get<byte>(j, i);
                    if (e == 0 | e == 2)
                    {
                        mask.Set<byte>(j, i, 0);
                    }
                    else if(e == 1)
                    {
                        mask.Set<byte>(j, i, 255);
                    } else
                    {
                        mask.Set<byte>(j, i, 127);
                    }
                }
            }
            Mat res = srcImg.Clone();

            dst = BitmapConverter.ToBitmap(mask);
        }

        private void ProcImage0(ref System.Drawing.Bitmap src, out System.Drawing.Bitmap dst)
        {
            dst = null;
            Mat srcImg = BitmapConverter.ToMat(src);
            Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGRA2BGR);
            Mat mask = new Mat(new OpenCvSharp.Size(src.Width, src.Height), MatType.CV_8UC1, 0);

            Mat edge = new Mat();
            Cv2.Canny(srcImg, mask, 50, 150);

            Cv2.Threshold(mask, mask, 50, 1, ThresholdTypes.Binary);

            //mask = edge.Clone();

            Mat bgdModel = new Mat();
            Mat fgdModel = new Mat();

            OpenCvSharp.Rect r = new OpenCvSharp.Rect(500, 500, (int)Width - 1000, (int)Height - 1000);
            Cv2.GrabCut(srcImg, mask, r, bgdModel, fgdModel, 1, GrabCutModes.InitWithMask);

            for (int i = 0; i < mask.Cols; i++)
            {
                for (int j = 0; j < mask.Rows; j++)
                {
                    byte e = mask.Get<byte>(j, i);
                    if (e == 0 | e == 2)
                    {
                        mask.Set<byte>(j, i, 0);
                    }
                    else
                    {
                        mask.Set<byte>(j, i, 255);
                    }
                }
            }


            dst = BitmapConverter.ToBitmap(mask);
        }

        private void ProcImage2(ref System.Drawing.Bitmap src, ref System.Drawing.Bitmap srcB, out System.Drawing.Bitmap dst)
        {
            dst = null;
            Mat srcImg = BitmapConverter.ToMat(src);
            Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGRA2BGR);

            Mat srcImgB = BitmapConverter.ToMat(srcB);
            Cv2.CvtColor(srcImgB, srcImgB, ColorConversionCodes.BGRA2BGR);

            Mat mask = new Mat();
            Cv2.Absdiff(srcImg, srcImgB, mask);
            Cv2.ImWrite(@"C:\opencv\ImageProcessing\ImageProcessing\Images\absdiff.jpg", mask);
            dst = BitmapConverter.ToBitmap(mask);
        }

        private void ProcImage3(ref System.Drawing.Bitmap src, ref System.Drawing.Bitmap srcB, out System.Drawing.Bitmap dst)
        {
            dst = null;
            Mat srcImg = BitmapConverter.ToMat(src);
            Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGRA2BGR);

            Mat srcImgB = BitmapConverter.ToMat(srcB);
            Cv2.CvtColor(srcImgB, srcImgB, ColorConversionCodes.BGRA2BGR);

            Mat mask = new Mat();
            double threshold = App.appSettings.DarkAreaThreshold;
            BackgroundSubtractor backSub = BackgroundSubtractorMOG2.Create(1, threshold, true);
            //BackgroundSubtractor backSub = BackgroundSubtractorMOG.Create(1, 5, 0.7, 0);
            //BackgroundSubtractor backSub = BackgroundSubtractorGMG.Create(1, 0.5);
            backSub.Apply(srcImgB, mask, 1);
            backSub.Apply(srcImg, mask, 0);

            //Cv2.Threshold(mask, mask, 180, 255, ThresholdTypes.Binary);

            //var element = Cv2.GetStructuringElement(
            //                    MorphShapes.Rect,
            //                    new OpenCvSharp.Size(2 * 2 + 1, 2 * 2 + 1),
            //                    new OpenCvSharp.Point(2, 2));

            //Mat tmp = new Mat();
            //Cv2.MorphologyEx(mask, tmp, MorphTypes.Close, element, null, App.appSettings.Iterations);

            //Cv2.MorphologyEx(tmp, mask, MorphTypes.Open, element, null, App.appSettings.Iterations2);
            //Cv2.Erode(mask, tmp, element);

            dst = BitmapConverter.ToBitmap(mask);
        }

        private void ProcImage4(ref System.Drawing.Bitmap src, ref System.Drawing.Bitmap srcB, out System.Drawing.Bitmap dst)
        {
            dst = null;
            Mat srcImg = BitmapConverter.ToMat(src);
            Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGRA2BGR);

            Mat srcImgB = BitmapConverter.ToMat(srcB);
            Cv2.CvtColor(srcImgB, srcImgB, ColorConversionCodes.BGRA2BGR);

            Mat mask = new Mat();

            Cv2.Absdiff(srcImg, srcImgB, mask);

            Mat tmp2 = new Mat();

            Cv2.Threshold(mask, tmp2, 35, 255, ThresholdTypes.Binary);

            Cv2.BitwiseAnd(srcImg, tmp2, mask);

            dst = BitmapConverter.ToBitmap(mask);
        }

        private void ProcImage5(ref System.Drawing.Bitmap src, ref System.Drawing.Bitmap srcB, out System.Drawing.Bitmap dst)
        {
            dst = null;
            Mat tmp = new Mat();
            var element = Cv2.GetStructuringElement(
                                MorphShapes.Rect,
                                new OpenCvSharp.Size(2 * 2 + 1, 2 * 2 + 1),
                                new OpenCvSharp.Point(2, 2));

            Mat srcImg = BitmapConverter.ToMat(src);
            Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGRA2BGR);
            Cv2.GaussianBlur(srcImg, tmp, new OpenCvSharp.Size(7, 7), 0);
            srcImg = tmp;
            Mat edge = new Mat();
            double threshold1 = App.appSettings.DarkAreaThreshold;
            double threshold2 = App.appSettings.BrightAreaThreshold;
            Cv2.Canny(srcImg, edge, threshold1, threshold2);
            tmp = edge;
            ////Cv2.MorphologyEx(edge, tmp, MorphTypes.Open, element);

            //Cv2.MorphologyEx(edge, tmp, MorphTypes.Close, element, null, 25);

            //Cv2.MorphologyEx(tmp, edge, MorphTypes.Open, element, null, 3);

            //Cv2.MorphologyEx(edge, tmp, MorphTypes.Close, element, null, 10);

            //Mat srcImgB = BitmapConverter.ToMat(srcB);
            //Cv2.CvtColor(srcImgB, srcImgB, ColorConversionCodes.BGRA2BGR);
            //Mat edgeB = new Mat();
            //Cv2.Canny(srcImgB, edgeB, 16, 50);

            //Mat mask = new Mat();

            //Cv2.Absdiff(edge, edgeB, mask);

            dst = BitmapConverter.ToBitmap(tmp);
        }

        private void CreateObjectMask(ref System.Drawing.Bitmap image, out System.Drawing.Bitmap mask)
        {
            double kThresh = 125;
            double hThresh = 95;
            kThresh = App.appSettings.DarkAreaThreshold;
            hThresh = App.appSettings.BrightAreaThreshold;
            double canny1 = 25;
            double canny2 = 75;

            mask = null;
            Mat tmp = new Mat();
            Mat src = BitmapConverter.ToMat(image);
            Cv2.CvtColor(src, src, ColorConversionCodes.BGRA2BGR);
            //Cv2.GaussianBlur(src, tmp, new OpenCvSharp.Size(7, 7), 0);
            Cv2.BilateralFilter(src, tmp, 11, 75, 75);
            src = tmp;

            Mat src_kirsch = BitmapConverter.ToMat(image.KirschFilter());

            Mat kirsch_gray = new Mat();
            Cv2.CvtColor(src_kirsch, kirsch_gray, ColorConversionCodes.RGB2GRAY);

            Cv2.ImWrite(@"C:\temp\kirsch_gray.jpg", kirsch_gray);
            Mat kirsch_threshold = new Mat();
            Cv2.Threshold(kirsch_gray, kirsch_threshold, kThresh, 255, ThresholdTypes.Binary);
            Cv2.ImWrite(@"C:\temp\kirsch_threshold.jpg", kirsch_threshold);

            Mat[] contours;
            List<OpenCvSharp.Point> hierarchy;
            List<Mat> hulls;
            Mat morph_element = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(2, 2), new OpenCvSharp.Point(1, 1));

            Mat tmpMat = kirsch_threshold.MorphologyEx(MorphTypes.Open, morph_element);
            Cv2.ImWrite(@"C:\temp\kirsch_threshold_open.jpg", tmpMat);

            kirsch_threshold = tmpMat;//.MorphologyEx(MorphTypes.Open, morph_element);
            //Cv2.ImWrite(@"C:\temp\kirsch_threshold_open2.jpg", tmpMat);
            #region morphology

            Mat kirsch_threshold_copy = new Mat();
            kirsch_threshold.CopyTo(kirsch_threshold_copy);

            int hullCount = 0, numLoops = 0;
            do
            {
                numLoops++;

                Mat kirsch_morph = kirsch_threshold_copy.MorphologyEx(MorphTypes.Gradient, morph_element);
                Cv2.ImWrite(@"C:\temp\kirsch_morph" + numLoops + ".jpg", kirsch_morph);
                hierarchy = new List<OpenCvSharp.Point>();
                Cv2.FindContours(kirsch_morph, out contours, OutputArray.Create(hierarchy),
                    RetrievalModes.External, ContourApproximationModes.ApproxSimple, new OpenCvSharp.Point(0, 0));

                hulls = new List<Mat>();
                for (int j = 0; j < contours.Length; j++)
                {
                    Mat hull = new Mat();
                    Cv2.ConvexHull(contours[j], hull);
                    hulls.Add(hull);
                }

                Mat drawing = Mat.Zeros(src.Size(), MatType.CV_8UC1);
                Cv2.DrawContours(drawing, hulls, -1, Scalar.White);

                Cv2.ImWrite(@"C:\temp\drawing" + numLoops + ".jpg", drawing);
                if (hulls.Count != hullCount && numLoops < 100)
                {
                    hullCount = hulls.Count;
                    kirsch_threshold_copy = drawing;
                }
                else
                {
                    break;
                }

            } while (true);

            #endregion

            if (numLoops >= 100)
            {
                throw new Exception("Could not find hull");
            }

            #region bestHull
            //try and filter out dust near to stone
            double largestArea = hulls.Max(m => Cv2.ContourArea(m));
            var bestHulls = hulls.Where(m => Cv2.ContourArea(m) == largestArea).ToList();

            Mat hulls_mask = Mat.Zeros(src.Size(), MatType.CV_8UC1);
            Cv2.DrawContours(hulls_mask, bestHulls, -1, Scalar.White, -1);
            Cv2.ImWrite(@"C:\temp\hulls_mask.jpg", hulls_mask);

            //hulls_mask is the convex hull of outline, now look for clefts
            Cv2.Threshold(kirsch_gray, kirsch_threshold, hThresh, 255, ThresholdTypes.Binary);
            Mat kirsch_mask = Mat.Zeros(kirsch_threshold.Size(), kirsch_threshold.Type());
            kirsch_threshold.CopyTo(kirsch_mask, hulls_mask);
            Cv2.ImWrite(@"C:\temp\kirsch_mask.jpg", kirsch_mask);
            Mat kirsch_mask_canny = new Mat();
            Cv2.Canny(kirsch_mask, kirsch_mask_canny, canny1, canny2, 3);
            Cv2.ImWrite(@"C:\temp\kirsch_mask_canny.jpg", kirsch_mask_canny);
            morph_element = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5), new OpenCvSharp.Point(2, 2));
            Mat kirsch_filled = new Mat();
            Cv2.Dilate(kirsch_mask_canny, kirsch_filled, morph_element);
            Cv2.Dilate(kirsch_filled, kirsch_filled, morph_element);
            Cv2.Erode(kirsch_filled, kirsch_filled, morph_element);
            Cv2.Erode(kirsch_filled, kirsch_filled, morph_element);
            Cv2.ImWrite(@"C:\temp\kirsch_filled.jpg", kirsch_filled);
            hierarchy = new List<OpenCvSharp.Point>(); ;
            Cv2.FindContours(kirsch_filled, out contours, OutputArray.Create(hierarchy),
                    RetrievalModes.External, ContourApproximationModes.ApproxSimple, new OpenCvSharp.Point(0, 0));

            #endregion

            hulls_mask = Mat.Zeros(src.Size(), MatType.CV_8UC1);
            Cv2.DrawContours(hulls_mask, contours, -1, Scalar.White, -1);
            Cv2.ImWrite(@"C:\temp\hulls_mask_final.jpg", hulls_mask);
            Cv2.Erode(hulls_mask, hulls_mask, morph_element);
            Cv2.Erode(hulls_mask, hulls_mask, morph_element);

            Mat fImage = new Mat();
            src.CopyTo(fImage, hulls_mask);
            Cv2.ImWrite(@"C:\temp\fImage.png", fImage);
            mask = BitmapConverter.ToBitmap(hulls_mask);

            // contrast calculation
            //double contrast, L, a, b, H, C;
            //CalculateContrast(ref src, ref hulls_mask, out L, out a, out b, out contrast, out H, out C, ContrastType.RMS);

            //Console.WriteLine("contrast: {0}, L: {1}, a: {2}, b: {3}, C: {4}, H: {5}", contrast, L, a, b, C, H);

            //hulls_mask = null;

            //CalcHistogram(ref src, ref hulls_mask, out mask);
        }

        private void CreateObjectMask1(ref System.Drawing.Bitmap image, ref System.Drawing.Bitmap bg, out System.Drawing.Bitmap mask)
        {
            mask = null;
            int clusters = App.appSettings.Clusters;
            List<System.Drawing.Bitmap> srcList = new List<System.Drawing.Bitmap>() { image };
            List<System.Drawing.Bitmap> bgList = new List<System.Drawing.Bitmap>() { bg };
            List<System.Drawing.Rectangle> maskList = null;
            imageAnalyzer.Get_Description(ref srcList, ref bgList, ref maskList, clusters, out List<System.Drawing.Bitmap> clusterImgList,
                out List<List<Tuple<double, double, double, double>>> descriptions, out List<string> comments, true);
            if (clusterImgList.Count > 0)
            {
                mask = clusterImgList[0];
            }
            else
            {
                mask = image;
            }
        }

        private void CalculateContrast(ref Mat src, ref Mat maskSrc, out double L, out double a, out double b, out double contrast, out double H, out double C, ContrastType type)
        {
            contrast = -1;
            L = a = b = C = H = 0;
            if(type == ContrastType.RMS)
            {
                Mat img = src;
                Mat imgLab = new Mat(new OpenCvSharp.Size(img.Width, img.Height), MatType.CV_8UC3, 0);

                Cv2.CvtColor(img, imgLab, ColorConversionCodes.LBGR2Lab);

                Mat mask = maskSrc;

                Scalar mean, std;
                Cv2.MeanStdDev(imgLab, out mean, out std, mask);

                L = mean.Val0 * 100 / 255;
                a = mean.Val1 - 128;
                b = mean.Val2 - 128;
                contrast = std.Val0 / 255;
                H = calc_H(ref a, ref b);
                C = calc_C(ref a, ref b);

            } else if(type == ContrastType.Weber)
            {

            } else if(type == ContrastType.Michelson)
            {

            }
        }

        private void GlareDetection(ref System.Drawing.Bitmap image, out System.Drawing.Bitmap mask)
        {
            mask = null;

            Mat hsv = new Mat();
            Mat src = BitmapConverter.ToMat(image);
            Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);
            Mat[] planes;
            Cv2.Split(hsv, out planes);
            Cv2.ImWrite(@"C:\temp\hImage.png", planes[0]);
            Cv2.ImWrite(@"C:\temp\sImage.png", planes[1]);
            Cv2.ImWrite(@"C:\temp\vImage.png", planes[2]);
            mask = BitmapConverter.ToBitmap(planes[2]);

            Mat nonSat = new Mat(new OpenCvSharp.Size(src.Width, src.Height), MatType.CV_8UC1, 0);
            //nonSat = Cv2.Threshold(planes[1], nonSat, 180, 255, ThresholdTypes.Binary);
            for (int i = 0; i < planes[1].Cols; i++)
            {
                for(int j = 0; j < planes[1].Rows; j++)
                {
                    byte e = planes[1].Get<byte>(j, i);
                    byte v = planes[2].Get<byte>(j, i);
                    if(e < 150 && v > 220)
                    {
                        nonSat.Set<byte>(j, i, 255);
                    } else
                    {
                        nonSat.Set<byte>(j, i, 0);
                    }
                }
            }

            var disk = Cv2.GetStructuringElement(
                                MorphShapes.Rect,
                                new OpenCvSharp.Size(5, 5),
                                new OpenCvSharp.Point(2, 2));

            Mat m = new Mat();
            Cv2.Erode(nonSat, m, disk);
            Cv2.ImWrite(@"C:\temp\maskImage.png", nonSat);

            Mat dst = new Mat();
            src.CopyTo(dst, m);
            mask = BitmapConverter.ToBitmap(dst);
        }

        private void CalcHistogram(ref Mat src, ref Mat mask, out System.Drawing.Bitmap dst, bool usingGamma = true)
        {
            dst = null;

            Mat hsv = new Mat();
            Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);
            Mat[] planes;
            Cv2.Split(hsv, out planes);
            Cv2.ImWrite(@"C:\temp\hhist.png", planes[0]);
            Cv2.ImWrite(@"C:\temp\sHist.png", planes[1]);
            Cv2.ImWrite(@"C:\temp\vHist.png", planes[2]);

            dst = BitmapConverter.ToBitmap(planes[2]);

            Mat hist = new Mat();
            int[] hdims = { 180, 256 }; // Histogram size for each dimension
            Rangef[] ranges = { new Rangef(0, 180), new Rangef(0, 256) }; // min/max 
            Cv2.CalcHist(new Mat[] { hsv }, new int[] { 0, 1 }, mask, hist, 2, hdims, ranges);

            double minVal, maxVal;
            Cv2.MinMaxLoc(hist, out minVal, out maxVal);

            double gamma = App.appSettings.Gamma;

            Mat histImage = new Mat(new OpenCvSharp.Size(hist.Width, hist.Height), MatType.CV_8UC3, 0);
            byte intensity;
            for(int i = 0; i < hist.Cols; i++)
            {
                for(int j = 0; j < hist.Rows; j++)
                {
                    float value = hist.Get<float>(j, i);
                    Vec3b color = new Vec3b();
                    color.Item0 = (byte)j;
                    color.Item1 = (byte)i;
                    if (usingGamma)
                    {
                        intensity = (byte)(Math.Pow(value / maxVal, gamma) * 255.0);
                    }
                    else
                    {
                        intensity = (byte)(value / maxVal * 255);
                    }
                    color.Item2 = intensity;
                    //histImage.Set<byte>(j, i, intensity);
                    histImage.Set<Vec3b>(j, i, color);
                }
            }
            Mat histColor = new Mat();
            Cv2.CvtColor(histImage, histColor, ColorConversionCodes.HSV2BGR);
            Cv2.ImWrite(@"C:\temp\2dhist" + "_" + name + ".png", histColor);
            dst = BitmapConverter.ToBitmap(histColor);
        }

        public double calc_C(ref double a, ref double b)
        {
            return Math.Sqrt(a * a + b * b);
        }

        public double calc_H(ref double a, ref double b)
        {
            return Math.Atan2(b, a) * 180 / Math.PI;
        }

        private System.Drawing.Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage, null, null, null));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new System.Drawing.Bitmap(bitmap);
            }
        }

        private BitmapImage ToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public WriteableBitmap SaveAsWriteableBitmap(Canvas surface)
        {
            if (surface == null) return null;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            System.Windows.Size size = new System.Windows.Size(surface.ActualWidth, surface.ActualHeight);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new System.Windows.Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
              (int)size.Width,
              (int)size.Height,
              96d,
              96d,
              PixelFormats.Pbgra32);
            renderBitmap.Render(surface);


            //Restore previously saved layout
            surface.LayoutTransform = transform;

            //create and return a new WriteableBitmap using the RenderTargetBitmap
            return new WriteableBitmap(renderBitmap);
        }

        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        #region dispose
        private bool _disposed = false;

        protected override void OnDispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose 
                    postProcessing.exitProcessing();
                }
                _disposed = true;
            }
        }
        #endregion
    }

    static class ExtBitmap
    {
        static System.Drawing.Bitmap ConvolutionFilter(this System.Drawing.Bitmap sourceBitmap,
                                            double[,] xFilterMatrix,
                                            double[,] yFilterMatrix,
                                                  double factor = 1,
                                                       int bias = 0,
                                             bool grayscale = false)
        {
            System.Drawing.Imaging.BitmapData sourceData = sourceBitmap.LockBits(new System.Drawing.Rectangle(0, 0,
                                     sourceBitmap.Width, sourceBitmap.Height),
                                                       System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                  System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            if (grayscale == true)
            {
                float rgb = 0;

                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;

                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }

            double blueX = 0.0;
            double greenX = 0.0;
            double redX = 0.0;

            double blueY = 0.0;
            double greenY = 0.0;
            double redY = 0.0;

            double blueTotal = 0.0;
            double greenTotal = 0.0;
            double redTotal = 0.0;

            int filterOffset = 1;
            int calcOffset = 0;

            int byteOffset = 0;

            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blueX = greenX = redX = 0;
                    blueY = greenY = redY = 0;

                    blueTotal = greenTotal = redTotal = 0.0;

                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;

                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset +
                                         (filterX * 4) +
                                         (filterY * sourceData.Stride);

                            blueX += (double)(pixelBuffer[calcOffset]) *
                                      xFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];

                            greenX += (double)(pixelBuffer[calcOffset + 1]) *
                                      xFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];

                            redX += (double)(pixelBuffer[calcOffset + 2]) *
                                      xFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];

                            blueY += (double)(pixelBuffer[calcOffset]) *
                                      yFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];

                            greenY += (double)(pixelBuffer[calcOffset + 1]) *
                                      yFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];

                            redY += (double)(pixelBuffer[calcOffset + 2]) *
                                      yFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];
                        }
                    }

                    blueTotal = Math.Sqrt((blueX * blueX) + (blueY * blueY));
                    greenTotal = Math.Sqrt((greenX * greenX) + (greenY * greenY));
                    redTotal = Math.Sqrt((redX * redX) + (redY * redY));

                    if (blueTotal > 255)
                    { blueTotal = 255; }
                    else if (blueTotal < 0)
                    { blueTotal = 0; }

                    if (greenTotal > 255)
                    { greenTotal = 255; }
                    else if (greenTotal < 0)
                    { greenTotal = 0; }

                    if (redTotal > 255)
                    { redTotal = 255; }
                    else if (redTotal < 0)
                    { redTotal = 0; }

                    resultBuffer[byteOffset] = (byte)(blueTotal);
                    resultBuffer[byteOffset + 1] = (byte)(greenTotal);
                    resultBuffer[byteOffset + 2] = (byte)(redTotal);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            System.Drawing.Bitmap resultBitmap = new System.Drawing.Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            System.Drawing.Imaging.BitmapData resultData = resultBitmap.LockBits(new System.Drawing.Rectangle(0, 0,
                                     resultBitmap.Width, resultBitmap.Height),
                                                      System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                                  System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

        public static System.Drawing.Bitmap KirschFilter(this System.Drawing.Bitmap sourceBitmap,
                                          bool grayscale = true)
        {
            System.Drawing.Bitmap resultBitmap = ExtBitmap.ConvolutionFilter(sourceBitmap,
                                                Matrix.Kirsch3x3Horizontal,
                                                  Matrix.Kirsch3x3Vertical,
                                                        1.0, 0, grayscale);

            return resultBitmap;
        }

        static class Matrix
        {
            public static double[,] Laplacian3x3
            {
                get
                {
                    return new double[,]
                    { { -1, -1, -1,  },
                  { -1,  8, -1,  },
                  { -1, -1, -1,  }, };
                }
            }

            public static double[,] Laplacian5x5
            {
                get
                {
                    return new double[,]
                    { { -1, -1, -1, -1, -1, },
                  { -1, -1, -1, -1, -1, },
                  { -1, -1, 24, -1, -1, },
                  { -1, -1, -1, -1, -1, },
                  { -1, -1, -1, -1, -1  }, };
                }
            }

            public static double[,] LaplacianOfGaussian
            {
                get
                {
                    return new double[,]
                    { {  0,   0, -1,  0,  0 },
                  {  0,  -1, -2, -1,  0 },
                  { -1,  -2, 16, -2, -1 },
                  {  0,  -1, -2, -1,  0 },
                  {  0,   0, -1,  0,  0 }, };
                }
            }

            public static double[,] Gaussian3x3
            {
                get
                {
                    return new double[,]
                    { { 1, 2, 1, },
                  { 2, 4, 2, },
                  { 1, 2, 1, }, };
                }
            }

            public static double[,] Gaussian5x5Type1
            {
                get
                {
                    return new double[,]
                    { { 2, 04, 05, 04, 2 },
                  { 4, 09, 12, 09, 4 },
                  { 5, 12, 15, 12, 5 },
                  { 4, 09, 12, 09, 4 },
                  { 2, 04, 05, 04, 2 }, };
                }
            }

            public static double[,] Gaussian5x5Type2
            {
                get
                {
                    return new double[,]
                    { {  1,   4,  6,  4,  1 },
                  {  4,  16, 24, 16,  4 },
                  {  6,  24, 36, 24,  6 },
                  {  4,  16, 24, 16,  4 },
                  {  1,   4,  6,  4,  1 }, };
                }
            }

            public static double[,] Sobel3x3Horizontal
            {
                get
                {
                    return new double[,]
                    { { -1,  0,  1, },
                  { -2,  0,  2, },
                  { -1,  0,  1, }, };
                }
            }

            public static double[,] Sobel3x3Vertical
            {
                get
                {
                    return new double[,]
                    { {  1,  2,  1, },
                  {  0,  0,  0, },
                  { -1, -2, -1, }, };
                }
            }

            public static double[,] Prewitt3x3Horizontal
            {
                get
                {
                    return new double[,]
                    { { -1,  0,  1, },
                  { -1,  0,  1, },
                  { -1,  0,  1, }, };
                }
            }

            public static double[,] Prewitt3x3Vertical
            {
                get
                {
                    return new double[,]
                    { {  1,  1,  1, },
                  {  0,  0,  0, },
                  { -1, -1, -1, }, };
                }
            }


            public static double[,] Kirsch3x3Horizontal
            {
                get
                {
                    return new double[,]
                    { {  5,  5,  5, },
                  { -3,  0, -3, },
                  { -3, -3, -3, }, };
                }
            }

            public static double[,] Kirsch3x3Vertical
            {
                get
                {
                    return new double[,]
                    { {  5, -3, -3, },
                  {  5,  0, -3, },
                  {  5, -3, -3, }, };
                }
            }
        }
    }
}
