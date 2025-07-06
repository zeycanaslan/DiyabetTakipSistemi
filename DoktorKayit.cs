using System.Linq;

namespace DiyabetTakipSistemi
{
    using System;
    using System.Data.SqlClient;
    using System.Security.Cryptography;
    using System.Text;
    using System.Configuration;

    public class DoktorKayit
    {
        public void KayitYap(string tcKimlikNo, string ad, string soyad, DateTime dogumTarihi, string cinsiyet, string eposta, string sifre, byte[] profilResmi)
        {
            // AES için geçerli key ve iv uzunluğu sağlanıyor
            byte[] key = new byte[32]; // 256-bit key
            byte[] iv = new byte[16];  // 128-bit IV

            // Anahtarı ve IV'yi hashleyin
            using (var sha256 = SHA256.Create())
            {
                key = sha256.ComputeHash(Encoding.UTF8.GetBytes("bu1sabahgizli1key1234567890123456"));
            }

            iv = Encoding.UTF8.GetBytes("bu1baslangicivkey123").Take(16).ToArray(); // IV 16 byte olmalı

            // Şifreyi AES ile şifrele
            byte[] sifreliVeri = AESAlgoritmasi.Sifrele(sifre, key, iv);

            // Veritabanına bağlantı
            string connStr = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 1. Kullanıcı kaydını ekle
                string query = "INSERT INTO Kullanicilar (TCKimlikNo, Sifre, Ad, Soyad, DogumTarihi, Cinsiyet, Eposta, ProfilResmi, KullaniciTipi) " +
                               "VALUES (@tc, @sifre, @ad, @soyad, @dogumTarihi, @cinsiyet, @eposta, @profilResmi, @rol); SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tc", tcKimlikNo);
                cmd.Parameters.AddWithValue("@sifre", sifreliVeri);
                cmd.Parameters.AddWithValue("@ad", ad);
                cmd.Parameters.AddWithValue("@soyad", soyad);
                cmd.Parameters.AddWithValue("@dogumTarihi", dogumTarihi);
                cmd.Parameters.AddWithValue("@cinsiyet", cinsiyet);
                cmd.Parameters.AddWithValue("@eposta", eposta);
                cmd.Parameters.AddWithValue("@profilResmi", profilResmi);
                cmd.Parameters.AddWithValue("@rol", "Doktor");  // Kullanıcı tipi "Doktor"

                // Kullanıcıyı eklerken, yeni KullaniciID'yi alıyoruz
                int kullaniciID = Convert.ToInt32(cmd.ExecuteScalar());

                // 2. Doktor kaydını ekle
                string doktorQuery = "INSERT INTO Doktorlar (DoktorID) VALUES (@doktorID)";
                SqlCommand doktorCmd = new SqlCommand(doktorQuery, conn);
                doktorCmd.Parameters.AddWithValue("@doktorID", kullaniciID);
                doktorCmd.ExecuteNonQuery();

                Console.WriteLine("Doktor kaydı başarıyla oluşturuldu.");
            }
        }
    }

}
