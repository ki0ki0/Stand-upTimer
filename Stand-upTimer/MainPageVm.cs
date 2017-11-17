using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.HockeyApp;

namespace Stand_upTimer
{
    class MainPageVm: BaseVm
    {
#if DEBUG
        private readonly TimeSpan timeLimit = TimeSpan.FromSeconds(10);
        private readonly TimeSpan timeWarning = TimeSpan.FromSeconds(5);
#else
        private readonly TimeSpan timeLimit = TimeSpan.FromMinutes(2);
        private readonly TimeSpan timeWarning = TimeSpan.FromSeconds(10);
#endif

        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly DispatcherTimer updateTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(100)};
        
        private readonly MediaPlayer mediaPlayer1 = new MediaPlayer();
        private readonly MediaPlayer mediaPlayer2 = new MediaPlayer();

        private int prevRemain;
        private TimeSpan remain;
        private bool isExceeded;

        public MainPageVm()
        {
            TimeLimit = timeLimit;
            StartCommand = new Command(StartExecute, o => !stopwatch.IsRunning);
            NextCommand = new Command(NextExecute, o => stopwatch.IsRunning);
            StopCommand = new Command(StopExecute, o => stopwatch.IsRunning);

            updateTimer.Tick += UpdateTimerOnTick;
            remain = TimeLimit;

            var uri = new Uri("ms-winsoundevent:Notification.Looping.Alarm");
            mediaPlayer1.Source = new MediaPlaybackItem(
                MediaSource.CreateFromUri(uri),
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(100));
            mediaPlayer2.Source = new MediaPlaybackItem(
                MediaSource.CreateFromUri(uri),
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(500));

            HockeyClient.Current.TrackEvent("Run");

#if DEBUG
            Next();
#endif
        }

        private void StartExecute(object o)
        {
            HockeyClient.Current.TrackEvent("Start");
            History.Clear();
            Next();
            NextCommand.OnCanExecuteChanged();
            StopCommand.OnCanExecuteChanged();
            ((Button)o)?.Focus(FocusState.Programmatic);
        }

        private void Next()
        {
            Remain = TimeLimit;
            updateTimer.Start();
            stopwatch.Reset();
            stopwatch.Start();
        }

        private void UpdateTimerOnTick(object sender, object o)
        {
            Remain = TimeSpan.FromSeconds(Math.Ceiling(TimeLimit.Subtract(stopwatch.Elapsed).TotalSeconds));
            IsExceeded = Remain < TimeSpan.Zero;
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
            var mediaPlayer = isLong ? mediaPlayer2 : mediaPlayer1;
            mediaPlayer.Play();
        }

        private void NextExecute(object o)
        {
            HockeyClient.Current.TrackEvent(
                "Next",
                new Dictionary<string, string>() { ["Elapsed"] = stopwatch.Elapsed.ToString("c") });
            History.Add(new Record(stopwatch.Elapsed, stopwatch.Elapsed > timeLimit));
            Next();
        }

        private void StopExecute(object o)
        {
            HockeyClient.Current.TrackEvent(
                "Stop",
                new Dictionary<string, string>()
                {
                    ["Elapsed"] = stopwatch.Elapsed.ToString("c"),
                    ["Count"] = History.Count.ToString(CultureInfo.InvariantCulture)
                });
            updateTimer.Stop();
            stopwatch.Stop();
            StartCommand.OnCanExecuteChanged();
            NextCommand.OnCanExecuteChanged();
            ((Button)o)?.Focus(FocusState.Programmatic);
        }

        public TimeSpan TimeLimit { get; }

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


        public bool IsExceeded
        {
            get => isExceeded;
            private set
            {
                if (value.Equals(isExceeded)) return;
                isExceeded = value;
                OnPropertyChanged();
            }
        }

        public Command StartCommand { get; }
        public Command NextCommand { get; }
        public Command StopCommand { get; }

        public ObservableCollection<Record> History { get; } = new ObservableCollection<Record>();
    }
}
