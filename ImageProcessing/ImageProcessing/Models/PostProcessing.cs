using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using ImageProcLibOpenGL;

namespace ImageProcessing.Models
{

    public delegate void ProcessingFinishedHandler(System.Drawing.Bitmap wBmp);
    public delegate void ProcessingErrorHandler(string err);

    public class PostProcessing
    {
        EventWaitHandle _wh = new AutoResetEvent(false);
        Thread _worker;
        readonly object _locker = new object();

        Queue<System.Drawing.Bitmap> imgQ = new Queue<System.Drawing.Bitmap>();
        bool exit = false;

        ImageProcLibOpenGL.Effects.Effects effect = ImageProcLibOpenGL.Effects.Effects.NO_EFFECT;

        IQOpenGLRenderer renderer = new IQOpenGLRenderer(1, 1);

        public event ProcessingFinishedHandler processingFinished;
        public event ProcessingErrorHandler processingError;
 
        public PostProcessing()
        {
            _worker = new Thread(Processing);
            _worker.IsBackground = true;
            _worker.Start();
        }

        void RaiseProcessingFinishedEvent(System.Drawing.Bitmap wBmp)
        {
            processingFinished?.Invoke(wBmp);
        }

        void RaiseProcessingErrorEvent(string err)
        {
            processingError?.Invoke(err);
        }

        public void AddProcessingFinishedSubscriber(ProcessingFinishedHandler h)
        {
            processingFinished += h;
        }

        public void AddProcessingErrorSubscriber(ProcessingErrorHandler h)
        {
            processingError += h;
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
                    try
                    {
                        Console.WriteLine("Performing effect: ");
                        renderer.setBitmap(img);
                        renderer.setEffect(effect, (float)App.appSettings.ABFTheta);
                        renderer.DrawFrame();
                        RaiseProcessingFinishedEvent(renderer.m_BmpDst);
                    } catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        RaiseProcessingErrorEvent(ex.Message);
                    }
                }
                else
                {
                    _wh.WaitOne();
                }
            }

            Console.WriteLine("post processing thread exits");
        }

        public void enqueueImage(System.Drawing.Bitmap bmp, ImageProcLibOpenGL.Effects.Effects effect)
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
    }
}
