﻿using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using ModernVLC.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ModernVLC.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayerPage : Page
    {
        public PlayerPage()
        {
            this.InitializeComponent();
            RegisterEventHandlers();
            ConfigureTitleBar();
        }

        private void RegisterEventHandlers()
        {
            PointerEventHandler pointerPressedEventHandler = (s, e) => ViewModel.SetInteracting(true);
            PointerEventHandler pointerReleasedEventHandler = (s, e) => ViewModel.SetInteracting(false);
            SeekBar.AddHandler(PointerPressedEvent, pointerPressedEventHandler, true);
            SeekBar.AddHandler(PointerReleasedEvent, pointerReleasedEventHandler, true);
            SeekBar.AddHandler(PointerCanceledEvent, pointerReleasedEventHandler, true);
        }

        private void ConfigureTitleBar()
        {
            Window.Current.SetTitleBar(TitleBarElement);
            var coreApp = CoreApplication.GetCurrentView();
            coreApp.TitleBar.ExtendViewIntoTitleBar = true;

            var view = ApplicationView.GetForCurrentView();
            view.TitleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            view.TitleBar.InactiveBackgroundColor = Windows.UI.Colors.Transparent;
        }

        private void SeekBar_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //var position = e.GetCurrentPoint(SeekBar).Position;
            //var ratio = position.X / SeekBar.ActualWidth;
            //long potentialValue = (long)(ratio * SeekBar.Maximum);
            //SeekBarToolTip.Content = $"{potentialValue}";
            //SeekBarToolTip.HorizontalOffset = position.X;
        }

        public void FocusVideoView()
        {
            VideoView.Focus(FocusState.Programmatic);
        }

        private void VideoView_ProcessKeyboardAccelerators(UIElement sender, ProcessKeyboardAcceleratorEventArgs args)
        {
            long seekAmount = 0;
            int volumeChange = 0;
            int direction = 0;

            switch (args.Key)
            {
                case VirtualKey.Left:
                    direction = -1;
                    break;
                case VirtualKey.Right:
                    direction = 1;
                    break;
                case VirtualKey.Up:
                    volumeChange = 10;
                    break;
                case VirtualKey.Down:
                    volumeChange = -10;
                    break;
            }

            switch (args.Modifiers)
            {
                case VirtualKeyModifiers.Control:
                    seekAmount = 10000;
                    break;
                case VirtualKeyModifiers.Shift:
                    seekAmount = 1000;
                    break;
                case VirtualKeyModifiers.None:
                    seekAmount = 5000;
                    break;
            }

            if (seekAmount * direction != 0)
            {
                ViewModel.SeekingCommand.Execute(seekAmount * direction);
            }

            if (volumeChange != 0)
            {
                ViewModel.MediaPlayer.ObservableVolume += volumeChange;
            }
        }

        private Symbol GetPlayPauseSymbol(bool isPlaying) => isPlaying ? Symbol.Pause : Symbol.Play;

        private Symbol GetMuteToggleSymbol(bool isMute) => isMute ? Symbol.Mute : Symbol.Volume;

        private Symbol GetFullscreenToggleSymbol(bool isFullscreen) => isFullscreen ? Symbol.BackToWindow : Symbol.FullScreen;
    }
}
