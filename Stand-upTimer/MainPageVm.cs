using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stand_upTimer
{
    class MainPageVm: BaseVm
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private TimeSpan remain;
        private readonly TimeSpan inteval = TimeSpan.FromMinutes(2);
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly TimeSpan timerInterval = TimeSpan.FromMilliseconds(100);
        private readonly TimeSpan warningTimeSpan = TimeSpan.FromSeconds(10);
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();
        private double last;

        public TimeSpan Remain
        {
            get => remain;
            private set
            {
                if (value.Equals(remain)) return;
                remain = value;
                OnPropertyChanged();
            }
        }

        public MainPageVm()
        {
            History = new ObservableCollection<TimeSpan>();
            StartCommand = new Command(StartExecute, o => !stopwatch.IsRunning);
            NextCommand = new Command(NextExecute, o => stopwatch.IsRunning);
            StopCommand = new Command(StopExecute, o => stopwatch.IsRunning);

            timer.Interval = timerInterval;
            timer.Tick += TimerOnTick;
            Remain = inteval;
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-winsoundevent:Notification.Looping.Alarm"));
        }

        private void StartExecute(object o)
        {
            History.Clear();
            Next();
            NextCommand.OnCanExecuteChanged();
            StopCommand.OnCanExecuteChanged();
            ((Button)o)?.Focus(FocusState.Programmatic);
        }

        private void Next()
        {
            Remain = inteval;
            timer.Start();
            stopwatch.Reset();
            stopwatch.Start();
        }

        private void TimerOnTick(object sender, object o)
        {
            Remain = inteval.Subtract(ElapsedSeconds());
            if (Remain < warningTimeSpan)
            {
                if (ElapsedSeconds().TotalSeconds - last >= 1)
                {
                    last = ElapsedSeconds().TotalSeconds;
                    if (Remain < timerInterval && Remain > TimeSpan.Zero)
                    {
                        Sound(true);
                    }
                    else
                    {
                        Sound();
                    }
                }
            }
            else
            {
                last = ElapsedSeconds().TotalSeconds;
            }
            { }
        }

        private TimeSpan ElapsedSeconds()
        {
            return TimeSpan.FromSeconds(Math.Ceiling(stopwatch.Elapsed.TotalSeconds));
        }

        private void Sound(bool longSound = false)
        {
            mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
            mediaPlayer.Play();
            Task.Delay(longSound ? 500 : 100).ContinueWith(task => mediaPlayer.Pause());
        }

        private void NextExecute(object o)
        {
            History.Add(ElapsedSeconds());
            Next();
        }

        private void StopExecute(object o)
        {
            timer.Stop();
            stopwatch.Stop();
            StartCommand.OnCanExecuteChanged();
            NextCommand.OnCanExecuteChanged();
            ((Button)o)?.Focus(FocusState.Programmatic);
        }

        public Command StartCommand { get; }
        public Command NextCommand { get; }
        public Command StopCommand { get; }

        public ObservableCollection<TimeSpan> History { get; }
    }
}
