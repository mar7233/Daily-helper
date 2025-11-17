using System;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace DailyHelper;

public partial class KrediHesaplama : ContentPage
{
    // Hangi giriş alanının aktif olduğunu belirler
    enum ActiveField { None, Tutar, Faiz, Vade }
    ActiveField active = ActiveField.None;

    // Ham veri (kullanıcının girdiği sayıların string hali)
    string rawTutar = "0";
    string rawFaiz = "0";
    string rawVade = "1";

    public KrediHesaplama()
    {
        InitializeComponent();
        UpdateDisplays(); // Sayfa açıldığında değerleri göster
    }

    // Ekrandaki Label’ları günceller
    void UpdateDisplays()
    {
        if (double.TryParse(rawTutar, NumberStyles.Any, CultureInfo.InvariantCulture, out double t))
            tutarLabel.Text = t.ToString("N2", CultureInfo.CurrentCulture);

        if (double.TryParse(rawFaiz, NumberStyles.Any, CultureInfo.InvariantCulture, out double f))
            faizLabel.Text = f.ToString("N2", CultureInfo.CurrentCulture);

        if (int.TryParse(rawVade, out int v))
        {
            vadeLabel.Text = v.ToString();
            // Slider’ı senkronize eder (tekrar tetiklenmesin diye event geçici kaldırılır)
            vadeSlider.ValueChanged -= VadeSlider_ValueChanged;
            vadeSlider.Value = Math.Clamp(v, (int)vadeSlider.Minimum, (int)vadeSlider.Maximum);
            vadeSlider.ValueChanged += VadeSlider_ValueChanged;
        }
    }

    // Tutar alanına tıklandığında tuş takımını açar
    private void Tutar_Tapped(object sender, EventArgs e)
    {
        active = ActiveField.Tutar;
        KeypadGrid.IsVisible = true;
    }

    // Faiz alanına tıklandığında tuş takımını açar
    private void Faiz_Tapped(object sender, EventArgs e)
    {
        active = ActiveField.Faiz;
        KeypadGrid.IsVisible = true;
    }

    // Vade alanına tıklandığında tuş takımını açar
    private void Vade_Tapped(object sender, EventArgs e)
    {
        active = ActiveField.Vade;
        KeypadGrid.IsVisible = true;
    }

    // Rakam tuşlarına basıldığında aktif alana ekler
    private void Key_Clicked(object sender, EventArgs e)
    {
        if (sender is Button b)
        {
            string ch = b.Text;
            switch (active)
            {
                case ActiveField.Tutar: rawTutar = Append(rawTutar, ch); break;
                case ActiveField.Faiz: rawFaiz = Append(rawFaiz, ch); break;
                case ActiveField.Vade: rawVade = Append(rawVade, ch, true); break;
            }
            UpdateDisplays();
        }
    }

    // Sayı ekleme yardımcı metodu (baştaki sıfırları temizler)
    private string Append(string txt, string add, bool isInt = false)
    {
        if (txt == "0") txt = "";
        txt += add;
        if (!txt.Contains(".")) txt = txt.TrimStart('0');
        if (txt == "") txt = isInt ? "1" : "0";
        return txt;
    }

    // 00 tuşu
    private void DoubleZero_Clicked(object sender, EventArgs e)
    {
        switch (active)
        {
            case ActiveField.Tutar: rawTutar = Append(rawTutar, "00"); break;
            case ActiveField.Faiz: rawFaiz = Append(rawFaiz, "00"); break;
            case ActiveField.Vade: rawVade = Append(rawVade, "00", true); break;
        }
        UpdateDisplays();
    }

    // Ondalık nokta tuşu
    private void Dot_Clicked(object sender, EventArgs e)
    {
        if (active == ActiveField.Tutar && !rawTutar.Contains(".")) rawTutar += ".";
        if (active == ActiveField.Faiz && !rawFaiz.Contains(".")) rawFaiz += ".";
        UpdateDisplays();
    }

    // Geri silme tuşu
    private void Backspace_Clicked(object sender, EventArgs e)
    {
        switch (active)
        {
            case ActiveField.Tutar:
                rawTutar = rawTutar.Length > 1 ? rawTutar[..^1] : "0";
                break;
            case ActiveField.Faiz:
                rawFaiz = rawFaiz.Length > 1 ? rawFaiz[..^1] : "0";
                break;
            case ActiveField.Vade:
                rawVade = rawVade.Length > 1 ? rawVade[..^1] : "1";
                break;
        }
        UpdateDisplays();
    }

    // Aktif alanı sıfırlar
    private void Clear_Clicked(object sender, EventArgs e)
    {
        if (active == ActiveField.Tutar) rawTutar = "0";
        if (active == ActiveField.Faiz) rawFaiz = "0";
        if (active == ActiveField.Vade) rawVade = "1";
        UpdateDisplays();
    }

    // Tuş takımını kapatır (onay)
    private void Accept_Clicked(object sender, EventArgs e)
    {
        KeypadGrid.IsVisible = false;
        active = ActiveField.None;
    }

    // Tuş takımını kapatır (kapat butonu)
    private void HideKeypad_Clicked(object sender, EventArgs e)
    {
        KeypadGrid.IsVisible = false;
        active = ActiveField.None;
    }

    // Vade slider’ı değiştiğinde değeri günceller
    private void VadeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        rawVade = ((int)Math.Round(e.NewValue)).ToString();
        UpdateDisplays();
    }

    // Seçilen kredi türüne göre KKDF ve BSMV oranlarını verir
    private void GetTaxesForSelectedType(out double kkdfRate, out double bsmvRate)
    {
        kkdfRate = 0; bsmvRate = 0;
        if (krediTuruPicker == null || krediTuruPicker.SelectedIndex < 0) return;

        switch (krediTuruPicker.SelectedIndex)
        {
            case 0: kkdfRate = 0.15; bsmvRate = 0.10; break; // İhtiyaç
            case 1: kkdfRate = 0.15; bsmvRate = 0.05; break; // Taşıt
            case 2: kkdfRate = 0.00; bsmvRate = 0.00; break; // Konut
            case 3: kkdfRate = 0.00; bsmvRate = 0.05; break; // Ticari
        }
    }

    // Hesapla butonuna basıldığında çalışır
    private void Hesapla_Clicked(object sender, EventArgs e)
    {
        // Girişlerin geçerli olup olmadığını kontrol eder
        if (!double.TryParse(tutarLabel.Text, out double tutar) ||
            !double.TryParse(faizLabel.Text, out double faizPercent) ||
            !int.TryParse(vadeLabel.Text, out int vade))
        {
            DisplayAlert("Uyarı", "Geçersiz giriş.", "Tamam");
            return;
        }

        GetTaxesForSelectedType(out double kkdf, out double bsmv);
        double aylikTaksit = CalculateMonthlyInstallment(tutar, faizPercent, vade, kkdf, bsmv);
        double toplamOdeme = aylikTaksit * vade;
        double toplamFaiz = toplamOdeme - tutar;

        // Sonuçları ekrana yazar
        aylikLabel.Text = aylikTaksit.ToString("N2");
        toplamLabel.Text = toplamOdeme.ToString("N2");
        faizToplamLabel.Text = toplamFaiz.ToString("N2");
    }

    // Aylık taksit hesaplama formülü (KKDF ve BSMV dahil)
    private double CalculateMonthlyInstallment(double tutar, double faizPercent, int vade,
        double kkdf, double bsmv)
    {
        double r = faizPercent / 100.0;                    // Aylık faiz oranı
        double fullRate = r * (1 + kkdf + bsmv);           // Vergilerle birlikte
        double pow = Math.Pow(1 + fullRate, vade);
        return tutar * (pow * fullRate) / (pow - 1);       // PMT formülü
    }
}