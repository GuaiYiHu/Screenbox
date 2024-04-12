#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Screenbox.Core.Enums;
using Screenbox.Core.Messages;
using Screenbox.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Screenbox.Core.ViewModels
{
    public sealed partial class CommonViewModel : ObservableRecipient,
        IRecipient<PropertyChangedMessage<NavigationViewDisplayMode>>,
        IRecipient<PropertyChangedMessage<PlayerVisibilityState>>
    {
        public Dictionary<Type, string> NavigationStates { get; }

        public bool IsAdvancedModeEnabled => _settingsService.AdvancedMode;

        [ObservableProperty] private NavigationViewDisplayMode _navigationViewDisplayMode;
        [ObservableProperty] private Thickness _scrollBarMargin;
        [ObservableProperty] private Thickness _footerBottomPaddingMargin;
        [ObservableProperty] private double _footerBottomPaddingHeight;

        private readonly INavigationService _navigationService;
        private readonly IFilesService _filesService;
        private readonly IResourceService _resourceService;
        private readonly ISettingsService _settingsService;
        private readonly Dictionary<string, double> _scrollingStates;

        public CommonViewModel(INavigationService navigationService,
            IFilesService filesService,
            IResourceService resourceService,
            ISettingsService settingsService)
        {
            _navigationService = navigationService;
            _filesService = filesService;
            _resourceService = resourceService;
            _settingsService = settingsService;
            _navigationViewDisplayMode = Messenger.Send<NavigationViewDisplayModeRequestMessage>();
            NavigationStates = new Dictionary<Type, string>();
            _scrollingStates = new Dictionary<string, double>();

            // Activate the view model's messenger
            IsActive = true;
        }

        public void Receive(PropertyChangedMessage<NavigationViewDisplayMode> message)
        {
            this.NavigationViewDisplayMode = message.NewValue;
        }

        public void Receive(PropertyChangedMessage<PlayerVisibilityState> message)
        {
            ScrollBarMargin = message.NewValue == PlayerVisibilityState.Hidden
                ? new Thickness(0)
                : (Thickness)Application.Current.Resources["ContentPageScrollBarMargin"];

            FooterBottomPaddingMargin = message.NewValue == PlayerVisibilityState.Hidden
                ? new Thickness(0)
                : (Thickness)Application.Current.Resources["ContentPageBottomMargin"];

            FooterBottomPaddingHeight = message.NewValue == PlayerVisibilityState.Hidden
                ? 0
                : (double)Application.Current.Resources["ContentPageBottomPaddingHeight"];
        }

        public void SaveScrollingState(double verticalOffset, string pageTypeName, int backStackDepth)
        {
            _scrollingStates[pageTypeName + backStackDepth] = verticalOffset;
        }

        public bool TryGetScrollingState(string pageTypeName, int backStackDepth, out double verticalOffset)
        {
            return _scrollingStates.TryGetValue(pageTypeName + backStackDepth, out verticalOffset);
        }

        [RelayCommand]
        private void OpenAlbum(AlbumViewModel? album)
        {
            if (album == null) return;
            _navigationService.Navigate(typeof(AlbumDetailsPageViewModel),
                new NavigationMetadata(typeof(MusicPageViewModel), album));
        }

        [RelayCommand]
        private void OpenArtist(ArtistViewModel? artist)
        {
            if (artist == null) return;
            _navigationService.Navigate(typeof(ArtistDetailsPageViewModel),
                new NavigationMetadata(typeof(MusicPageViewModel), artist));
        }

        public List<IStorageItem> GetItemsAfterMatch(IReadOnlyList<IStorageItem>? items, StorageFile file)
        {
            if (items == null)
            {
                return new List<IStorageItem> { file };
            }
            int matchIndex = -1;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is StorageFile storageFile && storageFile.Path == file.Path)
                {
                    matchIndex = i;
                    break;
                }
            }

            if (matchIndex == -1)
            {
                // 如果没有找到匹配的文件，则返回空列表
                return new List<IStorageItem> { file };
            }
            else
            {
                // 返回匹配项及其后续的所有项
                return items.Skip(matchIndex).ToList();
            }
        }

        [RelayCommand]
        private async Task OpenFilesAsync()
        {
            try
            {
                IReadOnlyList<StorageFile>? files = await _filesService.PickMultipleFilesAsync();
                if (files == null || files.Count == 0) return;
                if (files.Count == 1)
                {
                    StorageFile file = files[0];
                    string parentFolderPath = System.IO.Path.GetDirectoryName(file.Path);
                    if (!string.IsNullOrEmpty(parentFolderPath))
                    {
                        StorageFolder? folder = await StorageFolder.GetFolderFromPathAsync(parentFolderPath);
                        if (folder == null)
                        {
                            Messenger.Send(new PlayMediaMessage(files));
                            return;
                        }
                        IReadOnlyList<IStorageItem> items = await _filesService.GetSupportedItems(folder).GetItemsAsync();
                        IStorageFile[] newFiles = items.OfType<IStorageFile>().ToArray();
                        if (newFiles.Length == 0) 
                        {
                            Messenger.Send(new PlayMediaMessage(files));
                        } else
                        {
                            List<IStorageItem> newItems = GetItemsAfterMatch(newFiles, file);
                            Messenger.Send(new PlayMediaMessage(newItems));
                        }
                    } else
                    {
                        Messenger.Send(new PlayMediaMessage(files));
                    }
                }
                else
                {
                    Messenger.Send(new PlayMediaMessage(files));
                }
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorMessage(
                    _resourceService.GetString(ResourceName.FailedToOpenFilesNotificationTitle), e.Message));
            }
        }
    }
}
