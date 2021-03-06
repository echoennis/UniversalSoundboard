﻿using davClassLibrary.Common;
using davClassLibrary.Models;
using UniversalSoundBoard.DataAccess;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace UniversalSoundboard.Common
{
    public class TriggerAction : ITriggerAction
    {
        public void UpdateAllOfTable(int tableId)
        {
            UpdateView(tableId);
        }

        public void UpdateTableObject(TableObject tableObject, bool fileDownloaded)
        {
            if (tableObject.TableId == FileManager.PlayingSoundTableId)
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    FileManager.UpdatePlayingSoundListItemAsync(tableObject.Uuid);
                });
            }
        }

        public void DeleteTableObject(TableObject tableObject)
        {
            UpdateView(tableObject.TableId);
        }

        public void SyncFinished()
        {
            FileManager.syncFinished = true;
        }

        private void UpdateView(int tableId)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (tableId == FileManager.ImageFileTableId || tableId == FileManager.SoundFileTableId)
                {
                    // Update the sounds
                    FileManager.itemViewHolder.AllSoundsChanged = true;
                    FileManager.UpdateGridViewAsync();
                }
                else if (tableId == FileManager.CategoryTableId)
                {
                    // Update the categories
                    FileManager.CreateCategoriesListAsync();
                    FileManager.itemViewHolder.AllSoundsChanged = true;
                }
                else if (tableId == FileManager.PlayingSoundTableId)
                {
                    // Update the playing sounds
                    FileManager.CreatePlayingSoundsListAsync();
                }
            });
        }
    }
}
