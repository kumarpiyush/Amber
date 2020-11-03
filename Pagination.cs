using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Amber
{
    public sealed partial class MainPage : Page
    {
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