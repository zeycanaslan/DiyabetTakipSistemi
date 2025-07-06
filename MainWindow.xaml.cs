using System.Windows;
using System;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace DiyabetTakipSistemi
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        /*
            try
            {
                // Doktor bilgileri
                string tcKimlikNo = "20000000030";
                string ad = "Betül";
                string soyad = "Erdoğan";
                DateTime dogumTarihi = new DateTime(1970, 11,12);
                string cinsiyet = "Kadın";
                string eposta = "betul.erdogan@gmail.com";
                string sifre = "betul";

                // Profil resmi yükle
                byte[] profilResmi = File.ReadAllBytes(@"D:\prolab3\DiyabetTakipSistemi\resimler\8.jpg");

                // Şifreyi AES ile şifrele
// Şifreyi AES ile şifrele
                byte[] sifrelenmis = AESAlgoritmasi.Sifrele(sifre, AESAlgoritmasi.Key, AESAlgoritmasi.IV);

                // Veritabanına kayıt yap
                string connStr = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Kullanicilar 
                            (TCKimlikNo, Sifre, Ad, Soyad, DogumTarihi, Cinsiyet, Eposta, ProfilResmi, KullaniciTipi)
                        VALUES 
                            (@tc, @sifre, @ad, @soyad, @dogum, @cinsiyet, @eposta, @profil, 'Doktor');
                        SELECT SCOPE_IDENTITY();", conn);

                    cmd.Parameters.AddWithValue("@tc", tcKimlikNo);
                    cmd.Parameters.AddWithValue("@sifre", sifrelenmis);
                    cmd.Parameters.AddWithValue("@ad", ad);
                    cmd.Parameters.AddWithValue("@soyad", soyad);
                    cmd.Parameters.AddWithValue("@dogum", dogumTarihi);
                    cmd.Parameters.AddWithValue("@cinsiyet", cinsiyet);
                    cmd.Parameters.AddWithValue("@eposta", eposta);
                    cmd.Parameters.AddWithValue("@profil", profilResmi);

                    int kullaniciID = Convert.ToInt32(cmd.ExecuteScalar());

                    SqlCommand doktorCmd = new SqlCommand("INSERT INTO Doktorlar (DoktorID) VALUES (@id)", conn);
                    doktorCmd.Parameters.AddWithValue("@id", kullaniciID);
                    doktorCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Doktor kayıt işlemi başarısız: " + ex.Message);
            }
        */
        }    
        
        private void BtnDoktorGiris_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow("doktor");
            login.Show();
            this.Close();
        }
        private void BtnHastaGiris_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow("hasta");
            login.Show();
            this.Close();
        }
    }
}
