using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using LiveCharts;
using System.Collections.Generic;
using LiveCharts.Wpf;
using System.Windows.Media;
using System.Configuration;

namespace DiyabetTakipSistemi
{
    public partial class SecilenHastayaAitVeriler : Window
    {
        private int hastaID;
        // Chart veri serileri
        public SeriesCollection Series { get; set; }
        public List<string> Tarihler { get; set; }
        public SeriesCollection EgzersizSeries { get; set; }
        public SeriesCollection DiyetSeries { get; set; }

        public SecilenHastayaAitVeriler(int hastaID)
        {
            InitializeComponent();
            this.hastaID = hastaID;
            VerileriGetir();

            // Grafik bağlama
            GrafikChart.Series = Series;
            GrafikChart.AxisX.Add(new Axis
            {
                Title = "Tarih",
                Labels = Tarihler
            });
            GrafikChart.AxisY.Add(new Axis
            {
                Title = "Kan Şekeri",
                LabelFormatter = value => value.ToString()
            });
        }
        private void VerileriGetir()
        {
            string connStr = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            using (SqlConnection connect = new SqlConnection(connStr))
            {
                connect.Open();

                string sorgu = @"
            SELECT gv.VeriTarihi, gv.KanSekeri, gd.DiyetDurumu, gd.EgzersizDurumu
            FROM GunlukVeriler gv
            LEFT JOIN GunlukDurumlar gd 
                ON gv.HastaID = gd.HastaID AND gv.VeriTarihi = gd.Tarih
            WHERE gv.HastaID = @hastaID
            ORDER BY gv.VeriTarihi";
                SqlCommand cmd = new SqlCommand(sorgu, connect);
                cmd.Parameters.AddWithValue("@hastaID", hastaID);
                SqlDataReader rdr = cmd.ExecuteReader();

                Tarihler = new List<string>();
                var diyetEgzersiz = new ChartValues<double>();
                var sadeceDiyet = new ChartValues<double>();
                var sadeceEgzersiz = new ChartValues<double>();
                var hicbiri = new ChartValues<double>();

                while (rdr.Read())
                {
                    var tarih = ((DateTime)rdr["VeriTarihi"]).ToString("dd.MM.yyyy");
                    Tarihler.Add(tarih);

                    double seker = rdr["KanSekeri"] != DBNull.Value ? Convert.ToDouble(rdr["KanSekeri"]) : 0;
                    string diyetStr = rdr["DiyetDurumu"] != DBNull.Value ? rdr["DiyetDurumu"].ToString() : "";
                    string egzersizStr = rdr["EgzersizDurumu"] != DBNull.Value ? rdr["EgzersizDurumu"].ToString() : "";

                    bool diyet = diyetStr == "Uygulandı" || diyetStr == "Yapıldı";
                    bool egzersiz = egzersizStr == "Yapıldı";

                    if (diyet && egzersiz)
                        diyetEgzersiz.Add(seker);
                    else if (diyet)
                        sadeceDiyet.Add(seker);
                    else if (egzersiz)
                        sadeceEgzersiz.Add(seker);
                    else
                        hicbiri.Add(seker);
                }

                rdr.Close();

                Series = new SeriesCollection
        {
            new LineSeries { Title = "Diyet + Egzersiz", Values = diyetEgzersiz, Stroke = Brushes.Green, Fill = Brushes.Transparent },
            new LineSeries { Title = "Sadece Diyet", Values = sadeceDiyet, Stroke = Brushes.Blue, Fill = Brushes.Transparent },
            new LineSeries { Title = "Sadece Egzersiz", Values = sadeceEgzersiz, Stroke = Brushes.Orange, Fill = Brushes.Transparent },
            new LineSeries { Title = "Hiçbiri", Values = hicbiri, Stroke = Brushes.Red, Fill = Brushes.Transparent }
        };

                byte[] resim = GetHastaResmi(hastaID);
                imgHastaProfil.Source = ByteArrayToImage(resim);

                using (SqlConnection connectt = new SqlConnection(connStr)) // Yine aynı connection string
                {
                    connectt.Open();

                    // Günlük Veriler
                    SqlDataAdapter da = new SqlDataAdapter("SELECT VeriTarihi, KanSekeri FROM GunlukVeriler WHERE HastaID = @hastaID", connectt);
                    da.SelectCommand.Parameters.AddWithValue("@hastaID", hastaID);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgGunlukVeriler.ItemsSource = dt.DefaultView;

                    // Diyet Planı
                    // Diyet Planları – Tümünü getir
                    SqlCommand cmdDiyet = new SqlCommand("SELECT PlanTarihi, DiyetTuru, Aciklama FROM DiyetPlanlari WHERE HastaID = @hastaID ORDER BY PlanTarihi DESC", connectt);
                    cmdDiyet.Parameters.AddWithValue("@hastaID", hastaID);
                    SqlDataReader rdrDiyet = cmdDiyet.ExecuteReader();
                    var diyetListesi = new List<string>();
                    while (rdrDiyet.Read())
                    {
                        diyetListesi.Add($"{rdrDiyet["PlanTarihi"]:dd.MM.yyyy} - {rdrDiyet["DiyetTuru"]} - {rdrDiyet["Aciklama"]}");
                    }
                    rdrDiyet.Close();
                    txtDiyet.Text = string.Join("\n", diyetListesi); // Hepsini alt alta yaz

                    // Egzersiz Planları – Tümünü getir
                    SqlCommand cmdEgzersiz = new SqlCommand("SELECT PlanTarihi, EgzersizTuru, Aciklama FROM EgzersizPlanlari WHERE HastaID = @hastaID ORDER BY PlanTarihi DESC", connectt);
                    cmdEgzersiz.Parameters.AddWithValue("@hastaID", hastaID);
                    SqlDataReader rdrEgzersiz = cmdEgzersiz.ExecuteReader();
                    var egzersizListesi = new List<string>();
                    while (rdrEgzersiz.Read())
                    {
                        egzersizListesi.Add($"{rdrEgzersiz["PlanTarihi"]:dd.MM.yyyy} - {rdrEgzersiz["EgzersizTuru"]} - {rdrEgzersiz["Aciklama"]}");
                    }
                    rdrEgzersiz.Close();
                    txtEgzersiz.Text = string.Join("\n", egzersizListesi);

                    // Belirtiler
                    using (SqlCommand cmdBelirti = new SqlCommand("SELECT Belirti FROM Belirtiler WHERE HastaID = @hastaID ORDER BY BelirtiTarihi DESC", connectt))
                    {
                        cmdBelirti.Parameters.AddWithValue("@hastaID", hastaID);
                        using (SqlDataReader rdrBelirti = cmdBelirti.ExecuteReader())
                        {
                            var tumBelirtiler = new List<string>();
                            while (rdrBelirti.Read())
                            {
                                tumBelirtiler.Add(rdrBelirti["Belirti"].ToString());
                            }
                            txtBelirti.Text = string.Join("\n", tumBelirtiler); // Belirtileri alt alta yazdır
                        }


                        // Uyarı ve Doktora Mesajı
                        SqlCommand cmdUyariMesaj = new SqlCommand(@"
SELECT VeriTarihi, KanSekeri, Uyari, DoktorMesaji 
FROM GunlukVeriler 
WHERE HastaID = @hastaID 
ORDER BY VeriTarihi DESC", connectt);

                        cmdUyariMesaj.Parameters.AddWithValue("@hastaID", hastaID);

                        SqlDataReader rdrUyariMesaj = cmdUyariMesaj.ExecuteReader();

                        var uyarilar = new List<string>();
                        var mesajlar = new List<string>();

                        while (rdrUyariMesaj.Read())
                        {
                            string tarih = Convert.ToDateTime(rdrUyariMesaj["VeriTarihi"]).ToString("dd.MM.yyyy");
                            string uyari = rdrUyariMesaj["Uyari"] != DBNull.Value ? rdrUyariMesaj["Uyari"].ToString() : "";
                            string mesaj = rdrUyariMesaj["DoktorMesaji"] != DBNull.Value ? rdrUyariMesaj["DoktorMesaji"].ToString() : "";

                            if (!string.IsNullOrWhiteSpace(uyari))
                                uyarilar.Add($"{tarih}: {uyari}");

                            if (!string.IsNullOrWhiteSpace(mesaj))
                                mesajlar.Add($"{tarih}: {mesaj}");
                        }

                        rdrUyariMesaj.Close();

                        txtUyari.Text = string.Join("\n", uyarilar);
                        txtDoktorMesaj.Text = string.Join("\n", mesajlar);

                    }
                }
            }
        }

        private byte[] GetHastaResmi(int hastaID)
        {
            byte[] resim = null;
            string connectionString = @"veritabani bağlantisi";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ProfilResmi FROM Kullanicilar WHERE KullaniciID = @id", conn);
                cmd.Parameters.AddWithValue("@id", hastaID);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        resim = reader["ProfilResmi"] as byte[];
                }
            }
            return resim;
        }

        private BitmapImage ByteArrayToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            using (var ms = new MemoryStream(imageData))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }
}
