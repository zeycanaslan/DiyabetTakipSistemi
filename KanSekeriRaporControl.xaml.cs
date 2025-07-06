using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace DiyabetTakipSistemi
{
    public partial class KanSekeriRaporControl : UserControl
    {
        private int hastaID;
        private string connectionString;

        public KanSekeriRaporControl(int hastaId)
        {
            InitializeComponent();
            hastaID = hastaId;
            connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            YukleOlcumler();
        }

        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }

        private void YukleOlcumler()
        {
            var gunlukOlcumler = new Dictionary<DateTime, List<Olcum>>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ZamanDilimi, KanSekeri, VeriTarihi FROM GunlukVeriler WHERE HastaID = @HastaID ORDER BY VeriTarihi, ZamanDilimi";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HastaID", hastaID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tarih = reader.GetDateTime(2);
                            var olcum = new Olcum
                            {
                                ZamanDilimi = reader.GetString(0),
                                KanSekeri = Convert.ToDouble(reader.GetValue(1))
                            };

                            if (!gunlukOlcumler.ContainsKey(tarih))
                                gunlukOlcumler[tarih] = new List<Olcum>();

                            gunlukOlcumler[tarih].Add(olcum);
                        }
                    }
                }
            }

            List<GunlukOlcum> gosterilecekListe = gunlukOlcumler.Select(kv => new GunlukOlcum   // kv :  bir KeyValuePair<DateTime, List<Olcum>>` tipindedir.
            {
                Tarih = kv.Key,   //her bir gün için gruplandırma yapıldığında o günün tarihidir.
                Olcumler = kv.Value,  // O günün tüm ölçüm kayıtlarıdır. Yani List<Olcum> türünde bir listedir.
                Ortalama = kv.Value.Count > 0 ? kv.Value.Average(o => o.KanSekeri) : 0 //  o günkü tüm ölçümlerin kan şekeri ortalamas
            }).ToList();

            OlcumListesi.ItemsSource = gosterilecekListe;

            // Grafiksel gösterim
            Labels = gosterilecekListe.Select(g => g.Tarih.ToShortDateString()).ToList();
            var ortalamalar = gosterilecekListe.Select(g => g.Ortalama).ToList();  // grafik y ekseni

            SeriesCollection = new SeriesCollection //grafik üzerinde çizilecek verilerin toplandığı bir koleksiyondur.
            {
            new LineSeries  //Bu bir çizgidir ve içinde hangi verileri göstereceğini söylersin.
                {
                Title = "Ortalama Kan Şekeri",
                Values = new ChartValues<double>(ortalamalar),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10
                }
    };
            lineChart.Series = SeriesCollection;  // hangi çizgiler gösterilecek
            lineChart.AxisX[0].Labels = Labels;
        }

        // Veri modeli sınıfları
        // Her bir sabah/öğle/akşam gibi tek bir ölçümü temsil eder.
        public class GunlukOlcum
        {
            public DateTime Tarih { get; set; }
            public List<Olcum> Olcumler { get; set; }
            public double Ortalama { get; set; }
        }
        // Aynı tarihe ait bir günün tüm ölçümleri + o günün ortalamasını temsil eder.
        public class Olcum
        {
            public string ZamanDilimi { get; set; }
            public double KanSekeri { get; set; }
        }
    }
}
