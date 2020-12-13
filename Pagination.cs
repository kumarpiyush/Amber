using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Amber
{
    public sealed partial class MainPage : Page
    {
        private async void NextPage(object sender, RoutedEventArgs e)
        {
            await semaphor.WaitAsync();
            try
            {
                await SavePageTask(currentPage);
                currentPage++;
                pageNumber.Text = currentPage.ToString();
                mainCanvas.InkPresenter.StrokeContainer.Clear();
                await LoadPageTask(currentPage);
            }
            finally
            {
                semaphor.Release();
            }
        }

        private async void PrevPage(object sender, RoutedEventArgs e)
        {
            await semaphor.WaitAsync();
            try
            {
                if (currentPage == 1) return;
                await SavePageTask(currentPage);
                currentPage--;
                pageNumber.Text = currentPage.ToString();
                mainCanvas.InkPresenter.StrokeContainer.Clear();
                await LoadPageTask(currentPage);
            }
            finally
            {
                semaphor.Release();
            }
        }
    }
}