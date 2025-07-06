using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DiyabetTakipSistemi
{
    public partial class EgzersizDiyetTakibiControl : UserControl
    {
        private int hastaID;
        private string connectionString;

        public EgzersizDiyetTakibiControl(int hastaId)
        {
            InitializeComponent();
            hastaID = hastaId;
            connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            YuzdeleriHesapla();
            EgzersizleriGetir();
            DiyetleriGetir();
            BelirtileriGetir();
        }

        // Model sınıfları
        public class EgzersizModel
        {
            public string EgzersizTuru { get; set; }
            public DateTime Tarih { get; set; }
        }
        public class DiyetModel
        {
            public string DiyetTuru { get; set; }
            public DateTime Tarih { get; set; }
        }
        public class BelirtiModel
        {
            public string BelirtiAdi { get; set; }
            public DateTime Tarih { get; set; }
        }
        private void EgzersizleriGetir()
        {
            var liste = new List<EgzersizModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT EgzersizTuru, PlanTarihi FROM EgzersizPlanlari WHERE HastaID = @HastaID ORDER BY PlanTarihi DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HastaID", hastaID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        liste.Add(new EgzersizModel
                        {
                            EgzersizTuru = reader.GetString(0),
                            Tarih = reader.GetDateTime(1)
                        });
                    }
                }
            }
            lstEgzersiz.ItemsSource = liste;
        }

        private void DiyetleriGetir()
        {
            var liste = new List<DiyetModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT DiyetTuru, PlanTarihi FROM DiyetPlanlari WHERE HastaID = @HastaID ORDER BY PlanTarihi DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HastaID", hastaID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        liste.Add(new DiyetModel
                        {
                            DiyetTuru = reader.GetString(0),
                            Tarih = reader.GetDateTime(1)
                        });
                    }
                }
            }
            lstDiyet.ItemsSource = liste;
        }
        private void BelirtileriGetir()
        {
            var liste = new List<BelirtiModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Belirti, BelirtiTarihi FROM Belirtiler WHERE HastaID = @HastaID ORDER BY BelirtiTarihi DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HastaID", hastaID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        liste.Add(new BelirtiModel
                        {
                            BelirtiAdi = reader.GetString(0),
                            Tarih = reader.GetDateTime(1)
                        });
                    }
                }
            }
            lstBelirti.ItemsSource = liste;
        }
        private void YuzdeleriHesapla()
        {
            int toplamGun = 0;
            int egzersizYapilanGun = 0;
            int diyetUygulananGun = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT EgzersizDurumu, DiyetDurumu FROM GunlukDurumlar WHERE HastaID = @HastaID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HastaID", hastaID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            toplamGun++;

                            string egzersiz = reader.GetString(0);
                            string diyet = reader.GetString(1);

                            if (egzersiz == "Yapıldı") egzersizYapilanGun++;
                            if (diyet == "Uygulandı") diyetUygulananGun++;
                        }
                    }
                }
            }

            double egzersizYuzdesi = toplamGun > 0 ? (egzersizYapilanGun * 100.0 / toplamGun) : 0;
            double diyetYuzdesi = toplamGun > 0 ? (diyetUygulananGun * 100.0 / toplamGun) : 0;

            txtEgzersiz.Text = $"Egzersiz Uyumu: %{egzersizYuzdesi:F2}";
            txtDiyet.Text = $"Diyet Uyumu: %{diyetYuzdesi:F2}";

            GrafikCiz(egzersizYuzdesi, diyetYuzdesi);
        }
        private void GrafikCiz(double egzersizYuzde, double diyetYuzde)
        {
            grafikCanvas.Children.Clear();

            double barWidth = 100;
            double maxBarHeight = 120;

            // Egzersiz Barı
            var egzersizBar = new Rectangle
            {
                Width = barWidth,
                Height = maxBarHeight * egzersizYuzde / 100,
                Fill = Brushes.SeaGreen,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(egzersizBar, 50);
            Canvas.SetTop(egzersizBar, maxBarHeight - egzersizBar.Height + 20);
            grafikCanvas.Children.Add(egzersizBar);

            // Diyet Barı
            var diyetBar = new Rectangle
            {
                Width = barWidth,
                Height = maxBarHeight * diyetYuzde / 100,
                Fill = Brushes.IndianRed,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(diyetBar, 200);
            Canvas.SetTop(diyetBar, maxBarHeight - diyetBar.Height + 20);
            grafikCanvas.Children.Add(diyetBar);

            // Etiketler
            var egzersizLabel = new TextBlock
            {
                Text = "Egzersiz",
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = Brushes.SeaGreen,
                FontSize = 14
            };
            Canvas.SetLeft(egzersizLabel, 50);
            Canvas.SetTop(egzersizLabel, maxBarHeight + 25);
            grafikCanvas.Children.Add(egzersizLabel);

            var diyetLabel = new TextBlock
            {
                Text = "Diyet",
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = Brushes.IndianRed,
                FontSize = 14
            };
            Canvas.SetLeft(diyetLabel, 200);
            Canvas.SetTop(diyetLabel, maxBarHeight + 25);
            grafikCanvas.Children.Add(diyetLabel);
        }
    }
}
