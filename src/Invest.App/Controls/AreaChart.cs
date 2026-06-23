using System.Windows;
using System.Windows.Media;

namespace Invest.App.Controls;

/// <summary>
/// Courbe d'aire épurée (façon Finary) : ligne lissée + dégradé vers le bas, sans grille.
/// Dessinée à la main pour éviter toute dépendance graphique externe.
/// </summary>
public class AreaChart : FrameworkElement
{
    public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
        nameof(Values), typeof(IReadOnlyList<double>), typeof(AreaChart),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty LineColorProperty = DependencyProperty.Register(
        nameof(LineColor), typeof(Color), typeof(AreaChart),
        new FrameworkPropertyMetadata(Color.FromRgb(0x1F, 0xD2, 0x86), FrameworkPropertyMetadataOptions.AffectsRender));

    public IReadOnlyList<double>? Values
    {
        get => (IReadOnlyList<double>?)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public Color LineColor
    {
        get => (Color)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        var values = Values;
        double w = ActualWidth, h = ActualHeight;
        if (values is null || values.Count < 2 || w <= 0 || h <= 0) return;

        double min = values.Min(), max = values.Max();
        double range = max - min;
        if (range < 1e-9) range = 1;

        const double pad = 6;
        double usableH = h - pad * 2;
        double stepX = w / (values.Count - 1);

        Point MapPoint(int i) => new(i * stepX, pad + usableH * (1 - (values[i] - min) / range));

        // Tracé de la ligne (segments fins -> rendu lisse avec beaucoup de points).
        var figure = new PathFigure { StartPoint = MapPoint(0), IsClosed = false };
        for (int i = 1; i < values.Count; i++)
            figure.Segments.Add(new LineSegment(MapPoint(i), true));

        var lineGeometry = new PathGeometry();
        lineGeometry.Figures.Add(figure);

        // Aire : même tracé refermé sur le bas.
        var areaFigure = new PathFigure { StartPoint = new Point(0, h), IsClosed = true };
        areaFigure.Segments.Add(new LineSegment(MapPoint(0), false));
        for (int i = 1; i < values.Count; i++)
            areaFigure.Segments.Add(new LineSegment(MapPoint(i), false));
        areaFigure.Segments.Add(new LineSegment(new Point(w, h), false));

        var areaGeometry = new PathGeometry();
        areaGeometry.Figures.Add(areaFigure);

        var fill = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1)
        };
        fill.GradientStops.Add(new GradientStop(Color.FromArgb(0x55, LineColor.R, LineColor.G, LineColor.B), 0));
        fill.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, LineColor.R, LineColor.G, LineColor.B), 1));
        fill.Freeze();

        dc.DrawGeometry(fill, null, areaGeometry);
        dc.DrawGeometry(null, new Pen(new SolidColorBrush(LineColor), 2.5)
        {
            LineJoin = PenLineJoin.Round,
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        }, lineGeometry);
    }
}
