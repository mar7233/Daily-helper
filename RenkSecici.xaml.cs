using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Threading.Tasks;

namespace DailyHelper;

public partial class RenkSecici : ContentPage
{
    public RenkSecici()
    {
        InitializeComponent();
        // Başlangıç değerleri (örnek bir mor ton)
        redSlider.Value = 73;
        greenSlider.Value = 60;
        blueSlider.Value = 153;
        // Rengi hemen uygula
        UpdateColor();
    }

    // Herhangi bir slider değiştiğinde renk güncellenir
    private void OnColorChanged(object sender, ValueChangedEventArgs e)
    {
        UpdateColor();
    }

    // Tüm renk hesaplamalarını ve görsel güncellemeleri yapan ana metot
    private void UpdateColor()
    {
        int r = (int)Math.Round(redSlider.Value);
        int g = (int)Math.Round(greenSlider.Value);
        int b = (int)Math.Round(blueSlider.Value);

        // Kanal değerlerini ekrana yazar (renkli yazı ile)
        redValueLabel.Text = r.ToString();
        redValueLabel.TextColor = Color.FromRgb(255, 0, 0);

        greenValueLabel.Text = g.ToString();
        greenValueLabel.TextColor = Color.FromRgb(0, 128, 0);

        blueValueLabel.Text = b.ToString();
        blueValueLabel.TextColor = Color.FromRgb(0, 0, 204);

        // Slider başlığının rengini değiştirir (destekleniyorsa)
        try { redSlider.ThumbColor = Color.FromRgb(255, 0, 0); } catch { }
        try { greenSlider.ThumbColor = Color.FromRgb(0, 128, 0); } catch { }
        try { blueSlider.ThumbColor = Color.FromRgb(0, 0, 204); } catch { }

        // HEX kodu oluşturur ve gösterir
        string hex = $"#{r:X2}{g:X2}{b:X2}";
        colorCodeLabel.Text = hex;

        // Büyük önizleme alanının arka planını seçilen renge çevirir
        previewFrame.BackgroundColor = Color.FromRgb(r, g, b);

        // HEX yazısının arka planda okunabilmesi için kontrast renk seçer
        double brightness = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
        var contrastColor = (brightness < 0.5) ? Colors.White : Colors.Black;
        colorCodeLabel.TextColor = contrastColor;
        copyButton.TextColor = contrastColor;
    }

    // Kopyala butonuna basıldığında HEX kodunu panoya kopyalar
    private async void Kopyala_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Clipboard.SetTextAsync(colorCodeLabel.Text);
            await DisplayAlert("", $"Kopyalandı: {colorCodeLabel.Text}", "Tamam");
        }
        catch
        {
            await DisplayAlert("Hata", "Renk kodu kopyalanamadı.", "Tamam");
        }
    }

    // Rastgele renk üretir ve slider’ları günceller
    private void Rastgele_Clicked(object sender, EventArgs e)
    {
        var rnd = new Random();
        redSlider.Value = rnd.Next(0, 256);
        greenSlider.Value = rnd.Next(0, 256);
        blueSlider.Value = rnd.Next(0, 256);
        UpdateColor();
    }
}