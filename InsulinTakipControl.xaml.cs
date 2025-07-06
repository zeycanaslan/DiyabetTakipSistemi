using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DiyabetTakipSistemi
{
    public partial class InsulinTakipControl : UserControl
    {
        private string connectionString;
        private int hastaID;

        public InsulinTakipControl()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            dpBaslangic.SelectedDate = DateTime.Now.AddDays(-7);
            dpBitis.SelectedDate = DateTime.Now;

            grafikCanvas.SizeChanged += GrafikCanvas_SizeChanged;

        }
        private void GrafikCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (dgInsulinVerileri.ItemsSource is List<InsulinVeri> veriler)
            {
                GrafikCiz(veriler);
            }
        }

        public int HastaID
        {
            get => hastaID;
            set => hastaID = value;
        }

        private void BtnFiltrele_Click(object sender, RoutedEventArgs e)
        {
            if (hastaID == 0)
            {
                MessageBox.Show("Hasta ID atanmadı.");
                return;
            }

            if (dpBaslangic.SelectedDate == null || dpBitis.SelectedDate == null)
            {
                MessageBox.Show("Lütfen başlangıç ve bitiş tarihlerini seçin.");
                return;
            }

            DateTime baslangic = dpBaslangic.SelectedDate.Value.Date;
            DateTime bitis = dpBitis.SelectedDate.Value.Date;

            List<InsulinVeri> veriler = new List<InsulinVeri>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"SELECT VeriTarihi, ZamanDilimi, InsulinDozu
                                 FROM GunlukVeriler
                                 WHERE HastaID = @HastaID
                                 AND VeriTarihi BETWEEN @Baslangic AND @Bitis
                                 ORDER BY VeriTarihi, 
                                          CASE ZamanDilimi
                                              WHEN 'Sabah' THEN 1
                                              WHEN 'Öğle' THEN 2
                                              WHEN 'İkindi' THEN 3
                                              WHEN 'Akşam' THEN 4
                                              WHEN 'Gece' THEN 5
                                              ELSE 6
                                          END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HastaID", hastaID);
                cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                cmd.Parameters.AddWithValue("@Bitis", bitis);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DateTime tarih = Convert.ToDateTime(reader["VeriTarihi"]);
                    string zaman = reader["ZamanDilimi"].ToString();
                    string insulin = reader["InsulinDozu"].ToString();

                    veriler.Add(new InsulinVeri
                    {
                        Tarih = tarih.ToShortDateString(),
                        ZamanDilimi = zaman,
                        OnerilenInsulin = insulin
                    });
                }
            }
            if (veriler.Count == 0)
            {
                MessageBox.Show("Seçilen tarih aralığında veri bulunamadı.");
                grafikCanvas.Children.Clear();
            }
            else
            {
                dgInsulinVerileri.ItemsSource = veriler;
                GrafikCiz(veriler);
            }
        }

        private void GrafikCiz(List<InsulinVeri> veriler)
        {
            grafikCanvas.Children.Clear();

            var gruplar = veriler
                .GroupBy(v => v.Tarih)
                .Select(g => new
                {
                    Tarih = g.Key,
                    Ortalama = g.Average(v => v.OnerilenInsulinSayisal)
                })
                .ToList();

            double canvasWidth = grafikCanvas.ActualWidth;
            double canvasHeight = grafikCanvas.ActualHeight;
            double barWidth = 30;
            double spacing = 20;
            double maxDeger = gruplar.Count > 0 ? gruplar.Max(g => g.Ortalama) : 1;

            for (int i = 0; i < gruplar.Count; i++)
            {
                var g = gruplar[i];
                double barHeight = (g.Ortalama / maxDeger) * (canvasHeight - 40);

                var rect = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = Brushes.SteelBlue,
                    RadiusX = 4,
                    RadiusY = 4
                };

                Canvas.SetLeft(rect, i * (barWidth + spacing) + 20);
                Canvas.SetTop(rect, canvasHeight - barHeight - 20);
                grafikCanvas.Children.Add(rect);

                var label = new TextBlock
                {
                    Text = g.Tarih,
                    FontSize = 10
                };
                Canvas.SetLeft(label, i * (barWidth + spacing) + 10);
                Canvas.SetTop(label, canvasHeight - 15);
                grafikCanvas.Children.Add(label);
            }
        }

        public class InsulinVeri
        {
            public string Tarih { get; set; }
            public string ZamanDilimi { get; set; }
            public string OnerilenInsulin { get; set; }

            public double OnerilenInsulinSayisal
            {
                get
                {
                    double.TryParse(OnerilenInsulin, out double sonuc);
                    return sonuc;
                }
            }
        }
    }
}
