using Windows.UI.Input.Inking;
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

            // lasso selection event handlers
            mainCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;
            mainCanvas.InkPresenter.UnprocessedInput.PointerPressed += LassoPointerPressed;
            mainCanvas.InkPresenter.UnprocessedInput.PointerMoved += LassoPointerMoved;
            mainCanvas.InkPresenter.UnprocessedInput.PointerReleased += LassoPointerReleased;
            mainCanvas.InkPresenter.StrokeInput.StrokeStarted += PenOrEraserStarted;
            mainCanvas.InkPresenter.StrokesErased += PenOrEraserStarted;
            eraseSelection.Click += EraseLassoSelection;

            // save/load events
            saveButton.Click += SavePage;
            loadButton.Click += LoadPage;

            // page navigation events
            nextPageButton.Click += NextPage;
            previousPageButton.Click += PrevPage;

            // color picker events
            redButton.Click += (a, b) => SetPenColor(7);
            blueButton.Click += (a, b) => SetPenColor(16);
            blackButton.Click += (a, b) => SetPenColor(0);
            greenButton.Click += (a, b) => SetPenColor(14);

            SetupBackgroundSave();
        }

        private void SetPenColor(int colorIndex)
        {
            if (writingTools.ActiveTool is InkToolbarBallpointPenButton)
            {
                (writingTools.ActiveTool as InkToolbarBallpointPenButton).SelectedBrushIndex = colorIndex;
            }
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
    }
}
