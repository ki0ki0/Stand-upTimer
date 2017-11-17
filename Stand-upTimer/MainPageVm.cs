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
        private int prevTick;
        private MediaPlaybackItem shortSound;
        private MediaPlaybackItem longSound;

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
            shortSound = new MediaPlaybackItem(
                MediaSource.CreateFromUri(new Uri("ms-winsoundevent:Notification.Looping.Alarm")),
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(100));
            longSound = new MediaPlaybackItem(
                MediaSource.CreateFromUri(new Uri("ms-winsoundevent:Notification.Looping.Alarm")),
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(500));
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
            Remain = TimeSpan.FromSeconds(Math.Ceiling(inteval.Subtract(stopwatch.Elapsed).TotalSeconds));
            PlaySound();
        }

        private void PlaySound()
        {
            var remainSecs = (int)Remain.TotalSeconds;
            if (Remain < warningTimeSpan)
            {
                if (prevTick - remainSecs == 1)
                {
                    Sound(remainSecs == 0);
                }
            }
            prevTick = remainSecs;
        }

        private void Sound(bool isLong)
        {
            mediaPlayer.Source = isLong ? longSound : shortSound;
            mediaPlayer.Play();
        }

        private void NextExecute(object o)
        {
            History.Add(stopwatch.Elapsed);
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
