using System;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace DailyHelper;

public partial class VKIHesaplama : ContentPage
{
    // Hangi alanın düzenlendiğini takip eder
    enum ActiveField { None, Kilo, Boy }
    ActiveField active = ActiveField.None;

    // Kullanıcının girdiği ham değerler (string olarak tutulur)
    string rawKilo = "00.0";
    string rawBoy = "00.0";

    public VKIHesaplama()
    {
        InitializeComponent();
        // Slider’ların başlangıç değerlerini ayarlar
        if (kiloSlider != null) kiloSlider.Value = 30;
        if (boySlider != null) boySlider.Value = 120;
        UpdateDisplays();
    }

    // Kilo slider’ı hareket edince çalışır
    private void KiloSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        rawKilo = e.NewValue.ToString("F1", CultureInfo.InvariantCulture);
        UpdateDisplays();
    }

    // Boy slider’ı hareket edince çalışır
    private void BoySlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        rawBoy = e.NewValue.ToString("F1", CultureInfo.InvariantCulture);
        UpdateDisplays();
    }

    // Kilo değerine dokunulduğunda tuş takımını açar
    private void KiloLabel_Tapped(object sender, EventArgs e)
    {
        active = ActiveField.Kilo;
        KeypadGrid.IsVisible = true;
    }

    // Boy değerine dokunulduğunda tuş takımını açar
    private void BoyLabel_Tapped(object sender, EventArgs e)
    {
        active = ActiveField.Boy;
        KeypadGrid.IsVisible = true;
    }

    // Rakam tuşlarına basıldığında aktif alana ekler
    private void Key_Clicked(object sender, EventArgs e)
    {
        if (!(sender is Button b)) return;
        string ch = b.Text;
        switch (active)
        {
            case ActiveField.Kilo:
                AppendToRaw(ref rawKilo, ch);
                break;
            case ActiveField.Boy:
                AppendToRaw(ref rawBoy, ch);
                break;
        }
        UpdateDisplays();
    }

    // 00 tuşu
    private void DoubleZero_Clicked(object sender, EventArgs e)
    {
        switch (active)
        {
            case ActiveField.Kilo: rawKilo = ConcatAndClean(rawKilo, "00"); break;
            case ActiveField.Boy: rawBoy = ConcatAndClean(rawBoy, "00"); break;
        }
        UpdateDisplays();
    }

    // Ondalık nokta tuşu
    private void Dot_Clicked(object sender, EventArgs e)
    {
        switch (active)
        {
            case ActiveField.Kilo:
                if (!rawKilo.Contains(".")) rawKilo += ".";
                break;
            case ActiveField.Boy:
                if (!rawBoy.Contains(".")) rawBoy += ".";
                break;
        }
        UpdateDisplays();
    }

    // Geri silme tuşu
    private void Backspace_Clicked(object sender, EventArgs e)
    {
        switch (active)
        {
            case ActiveField.Kilo:
                if (rawKilo.Length > 0) rawKilo = rawKilo.Substring(0, rawKilo.Length - 1);
                if (rawKilo == "" || rawKilo == ".") rawKilo = "0";
                break;
            case ActiveField.Boy:
                if (rawBoy.Length > 0) rawBoy = rawBoy.Substring(0, rawBoy.Length - 1);
                if (rawBoy == "" || rawBoy == ".") rawBoy = "0";
                break;
        }
        UpdateDisplays();
    }

    // Aktif alanı sıfırlar
    private void Clear_Clicked(object sender, EventArgs e)
    {
        switch (active)
        {
            case ActiveField.Kilo: rawKilo = "0"; break;
            case ActiveField.Boy: rawBoy = "0"; break;
        }
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

    // Sayı eklerken baştaki sıfırları temizler
    private void AppendToRaw(ref string target, string ch)
    {
        if (target == "0") target = "";
        target = ConcatAndClean(target, ch);
    }

    // İki string’i birleştirir ve gereksiz sıfırları temizler
    private string ConcatAndClean(string current, string add)
    {
        string result = current + add;
        if (!result.Contains("."))
        {
            result = result.TrimStart('0');
            if (result == "") result = "0";
        }
        return result;
    }

    // Ekranı ve VKI sonucunu günceller
    private void UpdateDisplays()
    {
        // Ham değerleri double’a çevirir
        if (!double.TryParse(rawKilo, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double kilo))
            kilo = 30.0;
        if (!double.TryParse(rawBoy, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double boyCm))
            boyCm = 120.0;

        // Slider sınırları içinde tutar
        kilo = Math.Clamp(kilo, kiloSlider.Minimum, kiloSlider.Maximum);
        boyCm = Math.Clamp(boyCm, boySlider.Minimum, boySlider.Maximum);

        // Slider’ları günceller (tekrar tetiklenmesin diye event geçici kaldırılır)
        kiloSlider.ValueChanged -= KiloSlider_ValueChanged;
        kiloSlider.Value = kilo;
        kiloSlider.ValueChanged += KiloSlider_ValueChanged;

        boySlider.ValueChanged -= BoySlider_ValueChanged;
        boySlider.Value = boyCm;
        boySlider.ValueChanged += BoySlider_ValueChanged;

        // Değerleri ekrana yazar (1 ondalık basamak)
        kiloValueLabel.Text = kilo.ToString("F1", CultureInfo.CurrentCulture);
        boyValueLabel.Text = boyCm.ToString("F1", CultureInfo.CurrentCulture);

        // VKI hesaplama: kilo / (boy_m²)
        double heightMeters = boyCm / 100.0;
        double vki = 0;
        if (heightMeters > 0) vki = kilo / (heightMeters * heightMeters);
        vkiValueLabel.Text = vki.ToString("F2", CultureInfo.CurrentCulture);

        // VKI kategorisini ve rengini belirler
        string category;
        string colorKey = "AccentOrange";

        if (vki < 16.0) { category = "İleri Düzeyde Zayıf"; colorKey = "AccentOrange"; }
        else if (vki < 17.0) { category = "Orta Düzeyde Zayıf"; colorKey = "AccentOrange"; }
        else if (vki < 18.5) { category = "Hafif Düzeyde Zayıf"; colorKey = "AccentOrange"; }
        else if (vki < 25.0) { category = "Normal Kilolu"; colorKey = "NormalGreen"; }
        else if (vki < 30.0) { category = "Hafif Şişman / Fazla Kilolu"; colorKey = "AccentOrange"; }
        else if (vki < 35.0) { category = "1. Derecede Obez"; colorKey = "WarningRed"; }
        else if (vki < 40.0) { category = "2. Derecede Obez"; colorKey = "WarningRed"; }
        else { category = "3. Derecede Obez / Morbid Obez"; colorKey = "WarningRed"; }

        vkiCategoryLabel.Text = category;

        // Kategori rengine göre yazı rengini değiştirir
        if (Resources.ContainsKey(colorKey) && Resources[colorKey] is Color c)
        {
            vkiCategoryLabel.TextColor = c;
            vkiValueLabel.TextColor = c;
        }
        else
        {
            vkiCategoryLabel.TextColor = Colors.Black;
            vkiValueLabel.TextColor = Colors.Black;
        }
    }
}