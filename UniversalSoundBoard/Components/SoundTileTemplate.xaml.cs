﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UniversalSoundBoard.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.UI.Animations;
using UniversalSoundBoard.DataAccess;
using UniversalSoundBoard.Common;

namespace UniversalSoundBoard.Components
{
    public sealed partial class SoundTileTemplate : UserControl
    {
        public Sound Sound { get { return DataContext as Sound; } }
        int moreButtonClicked = 0;
        MenuFlyout OptionsFlyout;
        MenuFlyoutItem SetFavouriteFlyout;
        MenuFlyoutSubItem CategoriesFlyoutSubItem;

        
        public SoundTileTemplate()
        {
            InitializeComponent();
            Loaded += SoundTileTemplate_Loaded;
            DataContextChanged += (s, e) => Bindings.Update(); // <-- only working with x:Bind !!!
            SetDarkThemeLayout();
            SetDataContext();
            CreateFlyout();
        }
        
        void SoundTileTemplate_Loaded(object sender, RoutedEventArgs e)
        {
            CreateCategoriesFlyout();
        }
        
        private void SetDataContext()
        {
            ContentRoot.DataContext = (App.Current as App)._itemViewHolder;
        }
        
        private void SetDarkThemeLayout()
        {
            if((App.Current as App).RequestedTheme == ApplicationTheme.Dark)
            {
                ContentRoot.Background = new SolidColorBrush(Colors.Black);
            }
        }
        
        private void SoundTileOptionsSetFavourite_Click(object sender, RoutedEventArgs e)
        {
            bool newFav = !Sound.Favourite;

            // Update all lists containing sounds with the new favourite value
            List<ObservableCollection<Sound>> soundLists = new List<ObservableCollection<Sound>>
            {
                (App.Current as App)._itemViewHolder.sounds,
                (App.Current as App)._itemViewHolder.allSounds,
                (App.Current as App)._itemViewHolder.favouriteSounds
            };

            foreach (ObservableCollection<Sound> soundList in soundLists)
            {
                var sounds = soundList.Where(s => s.Uuid == Sound.Uuid);
                if (sounds.Count() > 0)
                {
                    sounds.First().Favourite = newFav;
                }
            }

            if (newFav)
            {
                // Add to favourites
                (App.Current as App)._itemViewHolder.favouriteSounds.Add(Sound);
            }
            else
            {
                // Remove sound from favourites
                (App.Current as App)._itemViewHolder.favouriteSounds.Remove(Sound);
            }

            FavouriteSymbol.Visibility = newFav ? Visibility.Visible : Visibility.Collapsed;
            SetFavouritesMenuItemText();
            FileManager.SetSoundAsFavourite(Sound.Uuid, newFav);
        }
        
        private async void SoundTileOptionsSetImage_Click(object sender, RoutedEventArgs e)
        {
            Sound sound = Sound;
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                (App.Current as App)._itemViewHolder.progressRingIsActive = true;
                await FileManager.AddImage(sound.Uuid, file);
                (App.Current as App)._itemViewHolder.allSoundsChanged = true;
                (App.Current as App)._itemViewHolder.progressRingIsActive = false;
                await FileManager.UpdateGridView();
            }
        }
        
        private async void SoundTileOptionsDelete_Click(object sender, RoutedEventArgs e)
        {
            var DeleteSoundContentDialog = ContentDialogs.CreateDeleteSoundContentDialog(Sound.Name);
            DeleteSoundContentDialog.PrimaryButtonClick += DeleteSoundContentDialog_PrimaryButtonClick;
            await DeleteSoundContentDialog.ShowAsync();
        }
        
        private async void DeleteSoundContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await FileManager.DeleteSound(Sound.Uuid);
            // UpdateGridView nicht in deleteSound, weil es auch in einer Schleife aufgerufen wird (löschen mehrerer Sounds)
            await FileManager.UpdateGridView();
        }
        
        private async void SoundTileOptionsRename_Click(object sender, RoutedEventArgs e)
        {
            var RenameSoundContentDialog = ContentDialogs.CreateRenameSoundContentDialog(Sound);
            RenameSoundContentDialog.PrimaryButtonClick += RenameSoundContentDialog_PrimaryButtonClick;
            await RenameSoundContentDialog.ShowAsync();
        }
        
        private async void RenameSoundContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Save new name
            if(ContentDialogs.RenameSoundTextBox.Text != Sound.Name)
            {
                FileManager.RenameSound(Sound.Uuid, ContentDialogs.RenameSoundTextBox.Text);
                await FileManager.UpdateGridView();
            }
        }
        
        private void CreateCategoriesFlyout()
        {
            if (moreButtonClicked == 0)
            {
                // Add some more invisible MenuFlyoutItems
                for (int i = 0; i < (App.Current as App)._itemViewHolder.categories.Count + 10; i++)
                {
                    ToggleMenuFlyoutItem item = new ToggleMenuFlyoutItem { Visibility = Visibility.Collapsed };
                    item.Click += CategoryToggleMenuItem_Click;
                    CategoriesFlyoutSubItem.Items.Add(item);
                }
            }

            foreach (MenuFlyoutItem item in CategoriesFlyoutSubItem.Items)
            {   // Make each item invisible
                item.Visibility = Visibility.Collapsed;
            }

            for (int n = 1; n < (App.Current as App)._itemViewHolder.categories.Count; n++)
            {
                if (CategoriesFlyoutSubItem.Items.ElementAt(n - 1) != null)
                {   // If the element is already there, set the new text
                    Category cat = (App.Current as App)._itemViewHolder.categories.ElementAt(n);
                    ((MenuFlyoutItem)CategoriesFlyoutSubItem.Items.ElementAt(n - 1)).Text = cat.Name;
                    ((MenuFlyoutItem)CategoriesFlyoutSubItem.Items.ElementAt(n - 1)).Tag = cat.Uuid;
                    ((MenuFlyoutItem)CategoriesFlyoutSubItem.Items.ElementAt(n - 1)).Visibility = Visibility.Visible;
                }
                else
                {
                    var item = new ToggleMenuFlyoutItem();
                    item.Click += CategoryToggleMenuItem_Click;
                    item.Text = (App.Current as App)._itemViewHolder.categories.ElementAt(n).Name;
                    item.Tag = (App.Current as App)._itemViewHolder.categories.ElementAt(n).Uuid;
                    CategoriesFlyoutSubItem.Items.Add(item);
                }
            }
        }
        
        private void CategoryToggleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var categoryObject = (ToggleMenuFlyoutItem) sender;
            string categoryUuid = categoryObject.Tag.ToString();
            FileManager.SetCategoryOfSound(Sound.Uuid, categoryUuid);
            
            UnselectAllItemsOfCategoriesFlyoutSubItem();
            categoryObject.IsChecked = true;
        }
        
        private void SelectRightCategory()
        {
            UnselectAllItemsOfCategoriesFlyoutSubItem();
            foreach (ToggleMenuFlyoutItem item in CategoriesFlyoutSubItem.Items)
            {
                if(Sound.Category != null && item.Tag != null)
                {
                    if (item.Tag.ToString() == Sound.Category.Uuid)
                    {
                        item.IsChecked = true;
                    }
                }
            }
        }
        
        private void UnselectAllItemsOfCategoriesFlyoutSubItem()
        {
            // Clear MenuItems and select selected item
            for (int i = 0; i < CategoriesFlyoutSubItem.Items.Count; i++)
            {
                (CategoriesFlyoutSubItem.Items[i] as ToggleMenuFlyoutItem).IsChecked = false;
            }
        }
        
        private void SetFavouritesMenuItemText()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            if (Sound.Favourite)
                SetFavouriteFlyout.Text = loader.GetString("SoundTile-UnsetFavourite");
            else
                SetFavouriteFlyout.Text = loader.GetString("SoundTile-SetFavourite");
        }
        
        private void CreateFlyout()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            OptionsFlyout = new MenuFlyout();
            OptionsFlyout.Opened += Flyout_Opened;
            CategoriesFlyoutSubItem = new MenuFlyoutSubItem { Text = loader.GetString("SoundTile-Category") };
            SetFavouriteFlyout = new MenuFlyoutItem();
            SetFavouriteFlyout.Click += SoundTileOptionsSetFavourite_Click;
            MenuFlyoutItem ShareFlyoutItem = new MenuFlyoutItem { Text = loader.GetString("Share") };
            ShareFlyoutItem.Click += ShareFlyoutItem_Click;
            MenuFlyoutSeparator separator = new MenuFlyoutSeparator();
            MenuFlyoutItem SetImageFlyout = new MenuFlyoutItem { Text = loader.GetString("SoundTile-ChangeImage") };
            SetImageFlyout.Click += SoundTileOptionsSetImage_Click;
            MenuFlyoutItem RenameFlyout = new MenuFlyoutItem { Text = loader.GetString("SoundTile-Rename") };
            RenameFlyout.Click += SoundTileOptionsRename_Click;
            MenuFlyoutItem DeleteFlyout = new MenuFlyoutItem { Text = loader.GetString("SoundTile-Delete") };
            DeleteFlyout.Click += SoundTileOptionsDelete_Click;

            OptionsFlyout.Items.Add(CategoriesFlyoutSubItem);
            OptionsFlyout.Items.Add(SetFavouriteFlyout);
            OptionsFlyout.Items.Add(ShareFlyoutItem);
            OptionsFlyout.Items.Add(separator);
            OptionsFlyout.Items.Add(SetImageFlyout);
            OptionsFlyout.Items.Add(RenameFlyout);
            OptionsFlyout.Items.Add(DeleteFlyout);
        }
        
        private void ShareFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }
        
        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            List<StorageFile> sounds = new List<StorageFile>();

            // Copy file with better name into temp folder and share it
            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile tempFile = await Sound.AudioFile.CopyAsync(tempFolder, Sound.Name + Sound.AudioFile.FileType, NameCollisionOption.ReplaceExisting);
            sounds.Add(tempFile);
            
            DataRequest request = args.Request;
            request.Data.SetStorageItems(sounds);

            request.Data.Properties.Title = loader.GetString("ShareDialog-Title");
            request.Data.Properties.Description = sounds.First().Name;
            deferral.Complete();
        }
        
        private void Flyout_Opened(object sender, object e)
        {
            CreateCategoriesFlyout();
            SelectRightCategory();
            SetFavouritesMenuItemText();

            moreButtonClicked++;
        }
        
        private void ContentRoot_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            OptionsFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }
        
        private void ContentRoot_Holding(object sender, HoldingRoutedEventArgs e)
        {
            OptionsFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }
        
        private void ContentRoot_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            SoundTileImage.Scale(1.1f, 1.1f, Convert.ToInt32(SoundTileImage.ActualWidth / 2), Convert.ToInt32(SoundTileImage.ActualHeight / 2), 2000, 0, EasingType.Quintic).Start();
        }
        
        private void ContentRoot_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            SoundTileImage.Scale(1, 1, Convert.ToInt32(SoundTileImage.ActualWidth / 2), Convert.ToInt32(SoundTileImage.ActualHeight / 2), 1000, 0, EasingType.Quintic).Start();
        }
    }
}