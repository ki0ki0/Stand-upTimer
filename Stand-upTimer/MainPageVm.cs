using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stand_upTimer
{
    class MainPageVm: BaseVm
    {
        private readonly TimeSpan timeLimit = TimeSpan.FromMinutes(2);
        private readonly TimeSpan timeWarning = TimeSpan.FromSeconds(10);

        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly DispatcherTimer updateTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(500)};
        
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();
        private readonly MediaPlaybackItem shortSound;
        private readonly MediaPlaybackItem longSound;

        private int prevRemain;
        private TimeSpan remain;

        public MainPageVm()
        {
            StartCommand = new Command(StartExecute, o => !stopwatch.IsRunning);
            NextCommand = new Command(NextExecute, o => stopwatch.IsRunning);
            StopCommand = new Command(StopExecute, o => stopwatch.IsRunning);

            updateTimer.Tick += UpdateTimerOnTick;
            Remain = timeLimit;

            var uri = new Uri("ms-winsoundevent:Notification.Looping.Alarm");
            shortSound = new MediaPlaybackItem(
                MediaSource.CreateFromUri(uri),
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(100));
            longSound = new MediaPlaybackItem(
                MediaSource.CreateFromUri(uri),
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
            Remain = timeLimit;
            updateTimer.Start();
            stopwatch.Reset();
            stopwatch.Start();
        }

        private void UpdateTimerOnTick(object sender, object o)
        {
            Remain = TimeSpan.FromSeconds(Math.Ceiling(timeLimit.Subtract(stopwatch.Elapsed).TotalSeconds));
            PlaySound();
        }

        private void PlaySound()
        {
            var remainSecs = (int)Remain.TotalSeconds;
            if (Remain < timeWarning)
            {
                if (prevRemain - remainSecs == 1)
                {
                    Sound(remainSecs == 0);
                }
            }
            prevRemain = remainSecs;
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
            updateTimer.Stop();
            stopwatch.Stop();
            StartCommand.OnCanExecuteChanged();
            NextCommand.OnCanExecuteChanged();
            ((Button)o)?.Focus(FocusState.Programmatic);
        }

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

        public Command StartCommand { get; }
        public Command NextCommand { get; }
        public Command StopCommand { get; }

        public ObservableCollection<TimeSpan> History { get; } = new ObservableCollection<TimeSpan>();
    }
}
