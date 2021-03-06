﻿using UniversalSoundBoard.Common;
using UniversalSoundBoard.Models;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UniversalSoundBoard.Components
{
    public sealed partial class SoundItemTemplate : UserControl
    {
        public Sound Sound { get { return DataContext as Sound; } }
        
        public SoundItemTemplate()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();

            if(App.Current.RequestedTheme == ApplicationTheme.Dark)
                RemoveSoundButton.Background = new SolidColorBrush(Colors.Black);
            else
                RemoveSoundButton.Background = new SolidColorBrush(Colors.White);
        }

        private void RemoveSoundButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogs.SoundsList.Remove(Sound);

            if(ContentDialogs.SoundsList.Count == 0)
            {
                if (ContentDialogs.PlaySoundsSuccessivelyContentDialog != null)
                    ContentDialogs.PlaySoundsSuccessivelyContentDialog.IsPrimaryButtonEnabled = false;
                else if (ContentDialogs.ExportSoundsContentDialog != null)
                    ContentDialogs.ExportSoundsContentDialog.IsPrimaryButtonEnabled = false;
            }
        }
    }
}
