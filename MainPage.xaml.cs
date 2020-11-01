﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Amber
{
    public sealed partial class MainPage : Page
    {
        const string pageExtension = ".gif";
        const string bookName = "AmberBook";   // TODO feature : support multiple books

        int currentPage = 1;
        const int numLines = 50;
        const int lineWidth = 40;
        const int pageWidth = 2000;

        public MainPage()
        {
            this.InitializeComponent();
            mainCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;

            saveButton.Click += SavePage;
            loadButton.Click += LoadPage;
            nextPageButton.Click += NextPage;
            previousPageButton.Click += PrevPage;
        }

        private void DrawLines()
        {
            for (int i = 2; i < numLines; i++)
            {
                var line = new Line() { X1 = 0, Y1 = i * lineWidth, X2 = pageWidth, Y2 = i * lineWidth };
                line.StrokeThickness = 2;
                line.Stroke = new SolidColorBrush() { Color = new Windows.UI.Color() { R = 235, G = 235, B = 235, A = 120 } };
                outerCanvas.Children.Add(line);
            }
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
