﻿using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Screenbox.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Screenbox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayerPage : Page
    {
        internal PlayerPageViewModel ViewModel => (PlayerPageViewModel)DataContext;

        public PlayerPage()
        {
            DataContext = App.Services.GetRequiredService<PlayerPageViewModel>();
            this.InitializeComponent();
            RegisterSeekBarPointerHandlers();
            FocusVideoViewOnEvents();
            Window.Current.SetTitleBar(TitleBarElement);
        }

        public void FocusVideoView()
        {
            VideoView.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            VideoView.ViewModel.OpenCommand.Execute(e.Parameter);
        }

        private void FocusVideoViewOnEvents()
        {
            Loaded += (_, _) => FocusVideoView();
            PageStates.CurrentStateChanged += (_, args) =>
            {
                if (args.NewState.Name == "PlayerVisible")
                    FocusVideoView();
            };
        }

        private void RegisterSeekBarPointerHandlers()
        {
            void PointerReleasedEventHandler(object s, PointerRoutedEventArgs e)
            {
                FocusVideoView();
            }

            SeekBar.AddHandler(PointerReleasedEvent, (PointerEventHandler)PointerReleasedEventHandler, true);
            SeekBar.AddHandler(PointerCanceledEvent, (PointerEventHandler)PointerReleasedEventHandler, true);
        }

        private void PlaybackSpeedItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (RadioMenuFlyoutItem)sender;
            ViewModel.SetPlaybackSpeed(item.Text);
        }

        private Symbol GetPlayPauseSymbol(bool isPlaying) => isPlaying ? Symbol.Pause : Symbol.Play;

        private Symbol GetFullscreenToggleSymbol(bool isFullscreen) => isFullscreen ? Symbol.BackToWindow : Symbol.FullScreen;

        private string GetHeightAsVec3(Size viewSize) => $"0,{viewSize.Height},0";
    }
}
