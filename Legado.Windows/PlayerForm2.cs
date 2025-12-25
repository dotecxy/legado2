using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace Legado.Windows
{
    public partial class PlayerForm2 : Form
    {
        const string url = "http://audiopay.cos.tx.xmcdn.com/download/1.0.0/storages/926f-audiopay/CF/59/CKwRIW4EiiP7AB5QAQCy25Xh.m4a?buy_key=aed65595bbd6d943057c57973f8b5b93&sign=3f56f3f61ced55b51f2b028468988aad&timestamp=1766673692517000&token=1251&duration=1800";
        NativeAudioService service = new NativeAudioService();
        Timer timer;


        public PlayerForm2()
        {
            InitializeComponent();

            service.PlayFinished += Service_PlayFinished;
            timer = new();
            timer.Interval = 300;
            timer.Elapsed += Timer_Elapsed;
            service.IsPlayingChanged += Service_IsPlayingChanged;
        }

        private void Service_IsPlayingChanged(object? sender, bool e)
        {
            if (e)
            {
                timer.Stop(); timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        bool setProgress = false;
        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var p = service.CurrentPositionMillisecond / (double)service.CurrentDurationMillisecond;
            p *= 100;
            label2.Invoke(() =>
            {
                label2.Text = TimeSpan.FromMilliseconds(service.CurrentDurationMillisecond).ToString("mm':'ss");
                label1.Text = TimeSpan.FromMilliseconds(service.CurrentPositionMillisecond).ToString("mm':'ss");
                if (setProgress) return;
                trackBar1.Value = (int)Math.Clamp(p, 0, 100);
            });
        }

        private void Service_PlayFinished(object? sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (service.IsPlaying)
            {
                service.PauseAsync();
                return;
            }
            service.PlayAsync(service.CurrentPositionMillisecond);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await service.InitializeAsync(url);
        }

        private void trackBar1_MouseDown(object sender, MouseEventArgs e)
        {
            setProgress = true;
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {

            var v = e.X / (double)trackBar1.Width;
            var seekValue = v * (double)service.CurrentDurationMillisecond ;
            service.SetCurrentTime(seekValue );

            setProgress = false;
        }
    }
}
