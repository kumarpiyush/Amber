using System;
using System.Threading.Tasks;
using Windows.Storage;
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

        private async Task SavePageTask(int pageNum)
        {
            var book = await KnownFolders.DocumentsLibrary.CreateFolderAsync(bookName, CreationCollisionOption.OpenIfExists);
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

            var book = await KnownFolders.DocumentsLibrary.CreateFolderAsync(bookName, CreationCollisionOption.OpenIfExists);
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
    }
}