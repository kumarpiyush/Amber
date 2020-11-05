using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Amber
{
    public sealed partial class MainPage : Page
    {
        private Polyline lasso; // keeps track of points as user moves the pen around in lasso mode
        private Rectangle selectionHighligher = null;  // rectangle to highlight what the user selected

        private Color lassoColor = new Color() { R = 255, G = 121, B = 32, A = 200 };
        const int lassoSelectionHighlighterPadding = 20;

        private void LassoPointerPressed(InkUnprocessedInput sender, PointerEventArgs args)
        {
            if (selectionHighligher != null)
            {
                outerCanvas.Children.Remove(selectionHighligher);
                selectionHighligher = null;
            }

            UnselectAllStrokes();

            lasso = new Polyline()
            {
                Stroke = new SolidColorBrush(lassoColor),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 5, 3 },
            };

            lasso.Points.Add(args.CurrentPoint.RawPosition);
            outerCanvas.Children.Add(lasso);
        }

        private void LassoPointerMoved(InkUnprocessedInput sender, PointerEventArgs args)
        {
            lasso.Points.Add(args.CurrentPoint.RawPosition);
        }

        private void LassoPointerReleased(InkUnprocessedInput sender, PointerEventArgs args)
        {
            lasso.Points.Add(args.CurrentPoint.RawPosition);
            var lassoedRegion = mainCanvas.InkPresenter.StrokeContainer.SelectWithPolyLine(lasso.Points);
            outerCanvas.Children.Remove(lasso);
            HighlightLassoSelection(lassoedRegion);
        }

        private void PenOrEraserStarted(object sender, object args)
        {
            if (selectionHighligher != null)
            {
                outerCanvas.Children.Remove(selectionHighligher);
                selectionHighligher = null;
            }

            UnselectAllStrokes();
        }

        private void UnselectAllStrokes()
        {
            foreach (var stroke in mainCanvas.InkPresenter.StrokeContainer.GetStrokes())
            {
                stroke.Selected = false;
            }
        }

        private void HighlightLassoSelection(Rect lassoedRegion)
        {
            if (!((lassoedRegion.Width == 0) || (lassoedRegion.Height == 0) || lassoedRegion.IsEmpty))
            {
                selectionHighligher = new Rectangle()
                {
                    Stroke = new SolidColorBrush(lassoColor),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection() { 5, 3 },
                    Width = lassoedRegion.Width + 2 * lassoSelectionHighlighterPadding,
                    Height = lassoedRegion.Height + 2 * lassoSelectionHighlighterPadding,
                };

                Canvas.SetLeft(selectionHighligher, lassoedRegion.X - lassoSelectionHighlighterPadding);
                Canvas.SetTop(selectionHighligher, lassoedRegion.Y - lassoSelectionHighlighterPadding);

                outerCanvas.Children.Add(selectionHighligher);
            }
        }

        private void EraseLassoSelection(object sender, RoutedEventArgs e)
        {
            if (selectionHighligher != null)
            {
                outerCanvas.Children.Remove(selectionHighligher);
                selectionHighligher = null;
            }

            mainCanvas.InkPresenter.StrokeContainer.DeleteSelected();
        }
    }
}