using SkiaSharp;
using AutoDispatch.Web.Common;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Логотипы новой компании dispatch по Legal Name (V1 Web/Helpers/ImageGenerator, System.Drawing → SkiaSharp):
///     малый 64x64 — одна или две буквы, большой 500x100 — название; фон случайный, общий для обоих логотипов экземпляра </summary>
public class LogoGenerator {
    private const string PngPrefix = "data:image/png;base64,";

    /// <summary>
    ///     Arial Bold V1 → Liberation Sans (OFL, метрики Arial) из Resources/Fonts, полужирный — Embolden: в образе aspnet шрифтов нет,
    ///     и FromFamilyName("Arial") там давал логотип без текста (аудит C2). Та же замена, что для RTF→PDF (TelerikPdfHelper) </summary>
    public static readonly SKTypeface Typeface = SKTypeface.FromFile(Path.Combine(AppSettings.FontsPath, "LiberationSans-Regular.ttf"));

    private readonly Random _rand = new Random();
    private SKColor? _backColor;

    private SKColor BackColor => _backColor ??= new SKColor((byte)_rand.Next(0, 255), (byte)_rand.Next(0, 255), (byte)_rand.Next(0, 255));

    public string GetSmallLogoPng(string name) {
        string title;
        float fontSize;
        float y = 0;
        if (_rand.Next(2) == 0) {
            // one letter
            title = name.Substring(0, 1);
            fontSize = 56;
        } else {
            // two letter
            title = name.Substring(0, 2);
            if (name.Contains(' ')) {
                var arr = name.Split(' ');
                title = arr[0].Substring(0, 1) + arr[1].Substring(0, 1);
            }
            fontSize = 38;
            y = 10;
        }
        return Draw(64, 64, title, fontSize, y);
    }

    public string GetBigLogoPng(string name) {
        var height = 100;
        var fontSize = 750 / name.Length;
        var y = (height - fontSize) / 2;
        return Draw(500, height, name, fontSize, y);
    }

    /// <summary>
    ///     y — верх строки, как Graphics.DrawString V1; Skia рисует от базовой линии </summary>
    private string Draw(int width, int height, string text, float fontSize, float y) {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(BackColor);
        using var font = new SKFont(Typeface, fontSize) { Embolden = true };
        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawText(text, 0, y - font.Metrics.Ascent, font, paint);
        using var image = SKImage.FromBitmap(bitmap);
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);
        return PngPrefix + Convert.ToBase64String(png.ToArray());
    }
}
