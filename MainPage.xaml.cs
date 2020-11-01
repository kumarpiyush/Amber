using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Amber
{
    public sealed partial class MainPage : Page
    {
        const string pageExtension = ".ampg";
        const string bookName = "AmberBook";   // TODO feature : support multiple books

        int currentPage = 1;

        public MainPage()
        {
            this.InitializeComponent();
            mainCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;

            saveButton.Click += SavePage;
            loadButton.Click += LoadPage;
            nextPageButton.Click += NextPage;
            previousPageButton.Click += PrevPage;
        }

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
            var book = await KnownFolders.DocumentsLibrary.GetFolderAsync(bookName);
            var pageFile = await book.TryGetItemAsync(string.Format("{0}{1}", pageNum, pageExtension)) as StorageFile;
            if (pageFile == null)
            {
                // TODO : how to notify?
                return; // unable to load file
            }

            var readStream = await pageFile.OpenAsync(FileAccessMode.Read);
            using (var inStream = readStream.GetInputStreamAt(0))
            {
                await mainCanvas.InkPresenter.StrokeContainer.LoadAsync(inStream);
            }
            readStream.Dispose();
        }

        private async void NextPage(object sender, RoutedEventArgs e)
        {
            await SavePageTask(currentPage);
            mainCanvas.InkPresenter.StrokeContainer.Clear();
            currentPage++;
            pageNumber.Text = currentPage.ToString();
            await LoadPageTask(currentPage);
        }

        private async void PrevPage(object sender, RoutedEventArgs e)
        {
            if (currentPage == 1) return;

            await SavePageTask(currentPage);
            mainCanvas.InkPresenter.StrokeContainer.Clear();
            currentPage--;
            pageNumber.Text = currentPage.ToString();
            await LoadPageTask(currentPage);
        }
    }
}
