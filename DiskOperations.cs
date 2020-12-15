using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Amber
{
    public sealed partial class MainPage : Page
    {
        private async void SavePage(object sender, RoutedEventArgs e)
        {
            await SavePageTask(currentPage);
        }

        private async void LoadPage(object sender, RoutedEventArgs e)
        {
            await LoadPageTask(currentPage);
        }

        private async void FirstLoad(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add("*");
            book = await picker.PickSingleFolderAsync();

            var lastPage = (await book.GetFilesAsync())
                            .Select(f => Utils.TryCastInt(f.DisplayName))
                            .Where(f => f!=-1)
                            .OrderBy(f => -f)
                            .FirstOrDefault();
            if (lastPage <= 0) lastPage = 1;
            currentPage = lastPage;
            pageNumber.Text = currentPage.ToString();
            await LoadPageTask(currentPage);
        }

        private async Task SavePageTask(int pageNum)
        {
            var pageFile = await book.CreateFileAsync(string.Format("{0}{1}", pageNum, pageExtension), CreationCollisionOption.OpenIfExists);

            CachedFileManager.DeferUpdates(pageFile);
            var writeStream = await pageFile.OpenAsync(FileAccessMode.ReadWrite);
            using (var outStream = writeStream.GetOutputStreamAt(0))
            {
                await mainCanvas.InkPresenter.StrokeContainer.SaveAsync(outStream);
                await outStream.FlushAsync();
            }
            writeStream.Dispose();
            await CachedFileManager.CompleteUpdatesAsync(pageFile);
        }

        private async Task LoadPageTask(int pageNum)
        {
            DrawLines();

            var pageFile = await book.TryGetItemAsync(string.Format("{0}{1}", pageNum, pageExtension)) as StorageFile;
            if (pageFile == null)
            {
                // TODO : distinguish page load fail vs a new page
                return;
            }

            var readStream = await pageFile.OpenAsync(FileAccessMode.Read);
            using (var inStream = readStream.GetInputStreamAt(0))
            {
                await mainCanvas.InkPresenter.StrokeContainer.LoadAsync(inStream);
            }
            readStream.Dispose();
        }

        private void SetupBackgroundSave()
        {
            var timer = ThreadPoolTimer.CreatePeriodicTimer(async (src) =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    await semaphor.WaitAsync();
                    try
                    {
                        await SavePageTask(currentPage);
                    }
                    finally
                    {
                        semaphor.Release();
                    }
                });
            }, TimeSpan.FromSeconds(30));
        }
    }
}