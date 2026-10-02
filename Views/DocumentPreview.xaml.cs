using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ACTA.Views.Controls;

public partial class DocumentPreviewControl : UserControl
{
    private const double ZoomStep = 0.2;
    private ViewerMode _viewerMode = ViewerMode.Pan;
    private ScrollViewer? _scrollViewer;
    private Point _dragStartPoint;
    private double _dragStartHorizontalOffset;
    private double _dragStartVerticalOffset;
    private bool _isDragging;

    private enum ViewerMode
    {
        Pan,
        Selection
    }

    public DocumentPreviewControl()
    {
        InitializeComponent();
        Loaded += DocumentPreviewControl_Loaded;
    }

    private void DocumentPreviewControl_Loaded(object sender, RoutedEventArgs e)
    {
        PdfViewer.ApplyTemplate();
        _scrollViewer = PdfViewer.Template.FindName("PART_Scroll", PdfViewer) as ScrollViewer;
        SetViewerMode(ViewerMode.Pan);
        UpdateZoomText();
    }

    public void LoadDocument(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("La ruta del documento no puede estar vacía.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("No se encontró el documento PDF.", filePath);
        }

        if (!string.Equals(Path.GetExtension(filePath), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException("La vista previa solo admite documentos PDF.");
        }

        PdfViewer.Source = filePath;
    }

    public void CloseDocument()
    {
        PdfViewer.Source = null;
    }

    private void DocumentPreviewControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Control -> Ancho: {ActualWidth:F2}px | Alto: {ActualHeight:F2}px");
    }





    // ---------------------------------------------------------
    // MODO DE VISUALIZACIÓN
    // ---------------------------------------------------------

    private void PanButton_Click(object sender, RoutedEventArgs e)
    {
        SetViewerMode(ViewerMode.Pan);
    }

    private void SelectionButton_Click(object sender, RoutedEventArgs e)
    {
        SetViewerMode(ViewerMode.Selection);
    }

    private void SetViewerMode(ViewerMode mode)
    {
        _viewerMode = mode;
        PanButton.IsChecked = mode == ViewerMode.Pan;
        SelectionButton.IsChecked = mode == ViewerMode.Selection;
        PdfViewer.Cursor = mode == ViewerMode.Pan ? Cursors.Hand : Cursors.IBeam;
    }

    // ---------------------------------------------------------
    // ZOOM
    // ---------------------------------------------------------

    private void ZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        SetZoom(PdfViewer.Zoom + ZoomStep);
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        SetZoom(PdfViewer.Zoom - ZoomStep);
    }

    private void SetZoom(double zoom)
    {
        PdfViewer.FitWidth = false;
        PdfViewer.Zoom = Math.Clamp(zoom, PdfViewer.ZoomMin, PdfViewer.ZoomMax);
        UpdateZoomText();
    }

    private void UpdateZoomText()
    {
        ZoomText.Text = $"{PdfViewer.Zoom * 100:0} %";
    }

    // ---------------------------------------------------------
    // AJUSTAR AL ANCHO
    // ---------------------------------------------------------

    private void FitWidthButton_Click(object sender, RoutedEventArgs e)
    {
        FitDocumentToWidth();
    }

    private void FitDocumentToWidth()
    {
        if (_scrollViewer is null || PdfViewer.Document is null || PdfViewer.Document.PageCount == 0)
        {
            return;
        }

        double viewportWidth = _scrollViewer.ViewportWidth;

        if (viewportWidth <= 0)
        {
            return;
        }

        double pageWidth = PdfViewer.Document.Pages[0].Size.Width;

        if (pageWidth <= 0)
        {
            return;
        }

        double zoom = viewportWidth / pageWidth;

        PdfViewer.FitWidth = false;

        PdfViewer.Zoom = Math.Clamp(zoom, PdfViewer.ZoomMin, PdfViewer.ZoomMax);

        UpdateZoomText();
    }

    // ---------------------------------------------------------
    // CTRL + WHEEL
    // ---------------------------------------------------------

    private void DocumentPreviewControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
        {
            return;
        }

        e.Handled = true;
        double newZoom = e.Delta > 0 ? PdfViewer.Zoom + ZoomStep : PdfViewer.Zoom - ZoomStep;
        SetZoom(newZoom);
    }

    private void DocumentPreview_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_viewerMode != ViewerMode.Pan || _scrollViewer is null)
        {
            return;
        }

        // Evita activar el PAN si el clic fue sobre la toolbar.
        if (IsMouseOverToolbar(e))
        {
            return;
        }

        _isDragging = true;

        _dragStartPoint = e.GetPosition(this);

        _dragStartHorizontalOffset = _scrollViewer.HorizontalOffset;

        _dragStartVerticalOffset = _scrollViewer.VerticalOffset;

        CaptureMouse();

        PdfViewer.Cursor = Cursors.SizeAll;

        // MUY IMPORTANTE:
        // impedimos que PDFViewer reciba el clic y comience
        // su mecanismo interno de selección de texto.
        e.Handled = true;
    }

    private void DocumentPreview_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _viewerMode != ViewerMode.Pan || _scrollViewer is null)
        {
            return;
        }

        Point currentPoint = e.GetPosition(this);

        Vector delta = currentPoint - _dragStartPoint;

        _scrollViewer.ScrollToHorizontalOffset(_dragStartHorizontalOffset - delta.X);

        _scrollViewer.ScrollToVerticalOffset(_dragStartVerticalOffset - delta.Y);

        e.Handled = true;       
    }

    private void DocumentPreview_PreviewMouseLeftButtonUp(object sender,MouseButtonEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;

        ReleaseMouseCapture();

        PdfViewer.Cursor = Cursors.Hand;

        e.Handled = true;
    }

    private bool IsMouseOverToolbar(MouseEventArgs e)
    {
        Point position = e.GetPosition(ViewerToolbar);

        return position.X >= 0 &&
               position.Y >= 0 &&
               position.X <= ViewerToolbar.ActualWidth &&
               position.Y <= ViewerToolbar.ActualHeight;
    }

    // ---------------------------------------------------------
    // HERRAMIENTA MANO
    // ---------------------------------------------------------

    private void PdfViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_viewerMode != ViewerMode.Pan || _scrollViewer is null)
        {
            return;
        }

        _isDragging = true;
        _dragStartPoint = e.GetPosition(this);
        _dragStartHorizontalOffset = _scrollViewer.HorizontalOffset;
        _dragStartVerticalOffset = _scrollViewer.VerticalOffset;
        PdfViewer.CaptureMouse();
        PdfViewer.Cursor = Cursors.SizeAll;
        e.Handled = true;
    }

    private void PdfViewer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _scrollViewer is null)
        {
            return;
        }

        Point currentPoint = e.GetPosition(this);
        Vector delta = currentPoint - _dragStartPoint;
        _scrollViewer.ScrollToHorizontalOffset(_dragStartHorizontalOffset - delta.X);
        _scrollViewer.ScrollToVerticalOffset(_dragStartVerticalOffset - delta.Y);
        e.Handled = true;
    }

    private void PdfViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        EndPan();
        e.Handled = true;
    }

    private void EndPan()
    {
        _isDragging = false;
        PdfViewer.ReleaseMouseCapture();

        if (_viewerMode == ViewerMode.Pan)
        {
            PdfViewer.Cursor = Cursors.Hand;
        }
    }
}
