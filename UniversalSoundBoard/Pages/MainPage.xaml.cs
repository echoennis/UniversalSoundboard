﻿using System.Linq;
using UniversalSoundBoard.Models;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using UniversalSoundBoard.DataAccess;

namespace UniversalSoundBoard.Pages
{
    public sealed partial class MainPage : Page
    {
        int sideBarCollapsedMaxWidth = FileManager.sideBarCollapsedMaxWidth;

        
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }
        
        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetDataContext();
            InitializeLocalSettings();
            (App.Current as App)._itemViewHolder.page = typeof(SoundPage);
            SideBar.MenuItemsSource = (App.Current as App)._itemViewHolder.categories;
            
            CustomiseTitleBar();
            await FileManager.ShowAllSounds();
        }
        
        private void SetDataContext()
        {
            WindowTitleTextBox.DataContext = (App.Current as App)._itemViewHolder;
            SideBar.DataContext = (App.Current as App)._itemViewHolder;
        }
        
        private void InitializeLocalSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values["playingSoundsListVisible"] == null)
            {
                localSettings.Values["playingSoundsListVisible"] = FileManager.playingSoundsListVisible;
                (App.Current as App)._itemViewHolder.playingSoundsListVisibility = FileManager.playingSoundsListVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                (App.Current as App)._itemViewHolder.playingSoundsListVisibility = (bool)localSettings.Values["playingSoundsListVisible"] ? Visibility.Visible : Visibility.Collapsed;
            }

            if (localSettings.Values["playOneSoundAtOnce"] == null)
            {
                localSettings.Values["playOneSoundAtOnce"] = FileManager.playOneSoundAtOnce;
                (App.Current as App)._itemViewHolder.playOneSoundAtOnce = FileManager.playOneSoundAtOnce;
            }
            else
            {
                (App.Current as App)._itemViewHolder.playOneSoundAtOnce = (bool)localSettings.Values["playOneSoundAtOnce"];
            }

            if (localSettings.Values["liveTile"] == null)
            {
                localSettings.Values["liveTile"] = FileManager.liveTile;
            }

            if (localSettings.Values["showCategoryIcon"] == null)
            {
                localSettings.Values["showCategoryIcon"] = FileManager.showCategoryIcon;
                (App.Current as App)._itemViewHolder.showCategoryIcon = FileManager.showCategoryIcon;
            }
            else
            {
                (App.Current as App)._itemViewHolder.showCategoryIcon = (bool)localSettings.Values["showCategoryIcon"];
            }

            if (localSettings.Values["showSoundsPivot"] == null)
            {
                localSettings.Values["showSoundsPivot"] = FileManager.showSoundsPivot;
                (App.Current as App)._itemViewHolder.showSoundsPivot = FileManager.showSoundsPivot;
            }
            else
            {
                (App.Current as App)._itemViewHolder.showSoundsPivot = (bool)localSettings.Values["showSoundsPivot"];
            }
        }
        
        private void CustomiseTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = ((App.Current as App).RequestedTheme == ApplicationTheme.Dark) ? Colors.White : Colors.Black;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
        
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            FileManager.GoBack();
            e.Handled = true;
        }
        
        private async void SideBar_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            (App.Current as App)._itemViewHolder.selectedSounds.Clear();

            FileManager.ResetSearchArea();

            // Display all Sounds with the selected category
            if (args.IsSettingsInvoked == true)
            {
                (App.Current as App)._itemViewHolder.page = typeof(SettingsPage);
                (App.Current as App)._itemViewHolder.title = (new Windows.ApplicationModel.Resources.ResourceLoader()).GetString("Settings-Title");
                (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Collapsed;
                (App.Current as App)._itemViewHolder.playAllButtonVisibility = Visibility.Collapsed;
                FileManager.SetBackButtonVisibility(true);
            }
            else
            {
                // Find the selected category in the categories list and set selectedCategory
                var category = (Category)args.InvokedItem;
                for(int i = 0; i < (App.Current as App)._itemViewHolder.categories.Count(); i++)
                {
                    if ((App.Current as App)._itemViewHolder.categories[i].Uuid == category.Uuid)
                    {
                        (App.Current as App)._itemViewHolder.selectedCategory = i;
                    }
                }

                if ((App.Current as App)._itemViewHolder.selectedCategory == 0)
                {
                    await FileManager.ShowAllSounds();
                }
                else
                {
                    await FileManager.ShowCategory(category.Uuid);
                }
            }
        }
    }
}