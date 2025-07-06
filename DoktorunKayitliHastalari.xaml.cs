using System.Data.SqlClient;
using System;
using System.Windows;
using System.Configuration;
using System.Collections.Generic;

namespace DiyabetTakipSistemi
{
    public partial class DoktorunKayitliHastalari : Window
        {
            private int doktorID;
            public DoktorunKayitliHastalari(int doktorID)
            {
                InitializeComponent();
                this.doktorID = doktorID;
                HastalariYukle();
                BelirtileriYukle();
        }
        // hasta bilgilerini combobox'ta göstermek için kullanılır
        public class ComboBoxItem
            {
                public string Text { get; set; }
                public string Value { get; set; }
                public override string ToString() => Text;
            }
        private void HastalariYukle(string belirti = "", int? minKan = null, int? maxKan = null)
        {
            hastalari_listele.Items.Clear();

            string connStr = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
SELECT DISTINCT k.KullaniciID, k.Ad + ' ' + k.Soyad AS HastaAd
FROM Hastalar h
JOIN Kullanicilar k ON h.HastaID = k.KullaniciID
LEFT JOIN Belirtiler b ON h.HastaID = b.HastaID
LEFT JOIN KanSekeriOlcumleri kso ON h.HastaID = kso.HastaID
WHERE h.DoktorID = @doktorID
  AND (@belirti = '' OR b.Belirti LIKE '%' + @belirti + '%')
  AND (@minKan IS NULL OR kso.OlcumDegeri >= @minKan)
  AND (@maxKan IS NULL OR kso.OlcumDegeri <= @maxKan)", conn);

                cmd.Parameters.AddWithValue("@doktorID", doktorID);
                cmd.Parameters.AddWithValue("@belirti", belirti ?? "");
                cmd.Parameters.AddWithValue("@minKan", minKan.HasValue ? (object)minKan.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@maxKan", maxKan.HasValue ? (object)maxKan.Value : DBNull.Value);

                SqlDataReader reader = cmd.ExecuteReader(); 
                while (reader.Read())
                {
                    hastalari_listele.Items.Add(new ComboBoxItem
                    {
                        Text = reader["HastaAd"].ToString(),
                        Value = reader["KullaniciID"].ToString()
                    });
                }
            }
        }
        private void lstHastalar_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (hastalari_listele.SelectedItem != null)
            {
                int hastaID = Convert.ToInt32(((ComboBoxItem)hastalari_listele.SelectedItem).Value);
                SecilenHastayaAitVeriler detayWindow = new SecilenHastayaAitVeriler(hastaID);
                detayWindow.ShowDialog();

                // Seçim sonrası tekrar null'a çekmek istersen 
                hastalari_listele.SelectedItem = null;
            }
        }
        //belirtilere göre filtreleme
        private void BelirtileriYukle()
        {
            cmbBelirtiler.Items.Clear();
            cmbBelirtiler.Items.Add("Tümü"); // Varsayılan

            List<string> belirtiler = new List<string>
                {
                    "poliüri",
                    "polifaji",
                    "polidipsi",
                    "nöropati",
                    "kilo kaybı",
                    "yorgunluk",
                    "yaraların yavaş iyileşmesi",
                    "bulanık görme"
                };

            foreach (var belirti in belirtiler)
            {
                cmbBelirtiler.Items.Add(belirti);
            }
            cmbBelirtiler.SelectedIndex = 0;
        }

        // kan sekerine göre filtreleme
        private void BtnFiltrele_Click(object sender, RoutedEventArgs e)
        {
            string secilenBelirti = cmbBelirtiler.SelectedItem?.ToString();
            if (secilenBelirti == "Tümü") secilenBelirti = "";

            int? minKan = string.IsNullOrWhiteSpace(txtMinKan.Text) ? (int?)null : int.Parse(txtMinKan.Text);
            int? maxKan = string.IsNullOrWhiteSpace(txtMaxKan.Text) ? (int?)null : int.Parse(txtMaxKan.Text);

            HastalariYukle(secilenBelirti, minKan, maxKan);
        }
        private void BtnHastaVeriGor_Click(object sender, RoutedEventArgs e)
        {
            if (hastalari_listele.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir hasta seçin.");
                return;
            }

            int hastaID = Convert.ToInt32(((ComboBoxItem)hastalari_listele.SelectedItem).Value);
            SecilenHastayaAitVeriler detayWindow = new SecilenHastayaAitVeriler(hastaID);
            detayWindow.ShowDialog();
        }
    }
}
