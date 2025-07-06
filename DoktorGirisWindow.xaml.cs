using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Configuration; // App.config dosyasından bağlantı cümlesini almak için gerekli


namespace DiyabetTakipSistemi
{
    public partial class DoktorGirisWindow : Window
    {
        private int doktorID;
        private string connectionString;

        public DoktorGirisWindow(int doktorID)
        {
            InitializeComponent();
            this.doktorID = doktorID;  // doktorgiriswindowsdan parametre olarak aldığımız doktor id yi yukarda tanımdağğımıza atadık
            byte[] resim = ProfilResminiGetir(doktorID);
            imgProfil.Source = ResmiDonustur(resim); // profil resmini byte arrayden image'e çevirip imgProfil'e atadık

        }
        private byte[] ProfilResminiGetir(int doktorID)
        {
            byte[] resim = null;
            connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ProfilResmi FROM Kullanicilar WHERE KullaniciID = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", doktorID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        resim = reader["ProfilResmi"] as byte[]; // veritabanında ProfilResmi sutunundan resmi getircek
                    }
                }
            }
            return resim;
        }

        private BitmapImage ResmiDonustur(byte[] resim_datasi)  // byte olarak kaydedilen resmi normal resim formatına donsuturcez
        {
            if (resim_datasi == null || resim_datasi.Length == 0) return null; // resim yoksa null döner

            using (var ms = new MemoryStream(resim_datasi)) // byte array'i stream'e çeviriyoruz sonrasında acılan fonksiyonu kendisi kapatır using
            {
                BitmapImage image = new BitmapImage();  
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
        private void BtnHastalarimiGor_Click(object sender, RoutedEventArgs e)
        {
            DoktorunKayitliHastalari hastalarWindow = new DoktorunKayitliHastalari(doktorID); 
            hastalarWindow.ShowDialog();
        }

        private void BtnHastaEkle_Click(object sender, RoutedEventArgs e)
        {
            HastaTanimlamaForm hastaForm = new HastaTanimlamaForm(doktorID); // hastanın hangi doktor tarafondan kaydedileceğini bilmek için DoktroID aldık
            hastaForm.ShowDialog(); 
        }
    }
}
