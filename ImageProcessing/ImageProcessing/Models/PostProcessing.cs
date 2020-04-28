using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using ImageProcessing.Pipeline;

namespace ImageProcessing.Models
{
    public enum Effects
    {
        NO_EFFECT,
        ABF
    }

    public delegate void ProcessingFinishedHandler(WriteableBitmap wBmp);

    public class PostProcessing
    {
        EventWaitHandle _wh = new AutoResetEvent(false);
        Thread _worker;
        readonly object _locker = new object();

        Queue<System.Drawing.Bitmap> imgQ = new Queue<System.Drawing.Bitmap>();
        bool exit = false;

        Effects effect = Effects.NO_EFFECT;

        WriteableBitmap postProcImage;

        IQOpenGLRenderer renderer = new IQOpenGLRenderer(1, 1);

        public event ProcessingFinishedHandler processingFinished;
 
        public PostProcessing()
        {
            _worker = new Thread(Processing);
            _worker.IsBackground = true;
            _worker.Start();
        }

        void RaiseProcessingFinishedEvent(WriteableBitmap wBmp)
        {
            processingFinished?.Invoke(wBmp);
        }

        public void AddProcessingFinishedSubscriber(ProcessingFinishedHandler h)
        {
            processingFinished += h;
        }

        void Processing()
        {
            Console.WriteLine("post processing thread is running...");

            // initial gpu
            renderer.init();

            while (!exit)
            {
                System.Drawing.Bitmap img = null;
                lock (_locker)
                {
                    if (imgQ.Count > 0)
                    {
                        img = imgQ.Dequeue();
                    }
                }
                if (img != null)
                {
                    Console.WriteLine("Performing effect: ");
                    renderer.setBitmap(img);
                    renderer.setEffect(effect);
                    renderer.DrawFrame();
                    RaiseProcessingFinishedEvent(new WriteableBitmap(ToBitmapImage(renderer.m_BmpDst)));
                }
                else
                {
                    _wh.WaitOne();
                }
            }

            Console.WriteLine("post processing thread exits");
        }

        public void enqueueImage(System.Drawing.Bitmap bmp, Effects effect)
        {
            lock (_locker)
            {
                imgQ.Enqueue(bmp);
            }
            this.effect = effect;
            _wh.Set();
        }

        public void exitProcessing()
        {
            exit = true;
            _wh.Set();
            _worker.Join();         // Wait for the consumer's thread to finish.
            _wh.Close();
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
    }
}
