using System.Configuration;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.IO;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Globalization;

namespace DiyabetTakipSistemi
{
    public partial class HosgeldinControl : UserControl
    {
        private int hastaID;
        private string connectionString;

        public HosgeldinControl(int hastaID)
        {
            InitializeComponent();
            this.hastaID = hastaID;
            connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            HastaBilgileriniYukle();
        }

        private void HastaBilgileriniYukle()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT Ad, Soyad, DogumTarihi, Eposta, ProfilResmi FROM Kullanicilar WHERE KullaniciID = @id", con);
                cmd.Parameters.AddWithValue("@id", hastaID);
                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    string ad = rdr["Ad"].ToString();
                    string soyad = rdr["Soyad"].ToString();
                    DateTime dogumTarihi = (DateTime)rdr["DogumTarihi"];
                    string dogumTarihiStr = dogumTarihi.ToString("dd.MM.yyyy", new CultureInfo("tr-TR"));

                    // Yaş hesaplama
                    int yas = DateTime.Now.Year - dogumTarihi.Year;
                    if (DateTime.Now < dogumTarihi.AddYears(yas))
                        yas--;

                    string eposta = rdr["Eposta"].ToString();
                    byte[] resimBytes = rdr["ProfilResmi"] != DBNull.Value ? (byte[])rdr["ProfilResmi"] : null;

                    // Ekrana yaz
                    txtAdSoyad.Text = $"👤 Ad Soyadı: {ad} {soyad}";
                    txtDogumTarihi.Text = $"🎂 Doğum Tarihi: {dogumTarihiStr}";
                    txtYas.Text = $"🕒 Yaş: {yas}";
                    txtEmail.Text = $"📧 E-posta: {eposta}";

                    if (resimBytes != null)
                        imgProfil.Source = ByteArrayToImage(resimBytes); // Bu metod senin projende tanımlı olmalı
                }
                rdr.Close();
            }
        }
        private ImageSource ByteArrayToImage(byte[] imageData)
        {
            using (var ms = new MemoryStream(imageData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }
    }
}
