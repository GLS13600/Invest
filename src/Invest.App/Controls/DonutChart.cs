using System.Windows;
using System.Windows.Media;

namespace Invest.App.Controls;

/// <summary>Une part du donut.</summary>
public class DonutSlice
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public Color Color { get; set; }
}

/// <summary>
/// Camembert "donut" de répartition des actifs, dessiné en arcs.
/// </summary>
public class DonutChart : FrameworkElement
{
    public static readonly DependencyProperty SlicesProperty = DependencyProperty.Register(
        nameof(Slices), typeof(IReadOnlyList<DonutSlice>), typeof(DonutChart),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public IReadOnlyList<DonutSlice>? Slices
    {
        get => (IReadOnlyList<DonutSlice>?)GetValue(SlicesProperty);
        set => SetValue(SlicesProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        var slices = Slices;
        double w = ActualWidth, h = ActualHeight;
        if (slices is null || slices.Count == 0 || w <= 0 || h <= 0) return;

        double total = slices.Sum(s => s.Value);
        if (total <= 0) return;

        double size = Math.Min(w, h);
        var center = new Point(w / 2, h / 2);
        double outer = size / 2 - 4;
        double inner = outer * 0.62; // épaisseur de l'anneau

        double startAngle = -90; // départ en haut
        foreach (var slice in slices)
        {
            double sweep = slice.Value / total * 360.0;
            if (sweep <= 0) continue;
            DrawArc(dc, center, inner, outer, startAngle, startAngle + sweep, slice.Color);
            startAngle += sweep;
        }
    }

    private static void DrawArc(DrawingContext dc, Point c, double inner, double outer,
        double startDeg, double endDeg, Color color)
    {
        Point P(double radius, double deg)
        {
            double rad = deg * Math.PI / 180.0;
            return new Point(c.X + radius * Math.Cos(rad), c.Y + radius * Math.Sin(rad));
        }

        bool large = (endDeg - startDeg) > 180;
        var figure = new PathFigure { StartPoint = P(outer, startDeg), IsClosed = true };
        figure.Segments.Add(new ArcSegment(P(outer, endDeg), new Size(outer, outer), 0, large,
            SweepDirection.Clockwise, true));
        figure.Segments.Add(new LineSegment(P(inner, endDeg), true));
        figure.Segments.Add(new ArcSegment(P(inner, startDeg), new Size(inner, inner), 0, large,
            SweepDirection.Counterclockwise, true));

        var geo = new PathGeometry();
        geo.Figures.Add(figure);
        dc.DrawGeometry(new SolidColorBrush(color), null, geo);
    }
}
