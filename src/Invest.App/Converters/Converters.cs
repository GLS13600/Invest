using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Invest.App.Converters;

/// <summary>Nombre négatif -> rouge, positif/zéro -> vert. Pour les variations.</summary>
public class SignToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        decimal d = System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        var key = d < 0 ? "RedBrush" : "GreenBrush";
        return Application.Current.Resources[key] ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

/// <summary>Formate un decimal en euros (ex: "2 527,65 €").</summary>
public class CurrencyConverter : IValueConverter
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null) return "—";
        decimal d = System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        return d.ToString("N2", Fr) + " €";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

/// <summary>Comme CurrencyConverter mais préfixe "+" pour les valeurs positives.</summary>
public class SignedCurrencyConverter : IValueConverter
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        decimal d = System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        string sign = d > 0 ? "+" : "";
        return sign + d.ToString("N2", Fr) + " €";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

/// <summary>Formate un ratio (0.083) en pourcentage signé ("+8,30 %").</summary>
public class PercentConverter : IValueConverter
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double d = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        string sign = d > 0 ? "+" : "";
        return sign + (d * 100).ToString("N2", Fr) + " %";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
