using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Configuration;
using System;

namespace DiyabetTakipSistemi
{
    public partial class LoginWindow : Window
    {
        private string kullaniciTipi;
        private string connectionString;
        private int doktorID;
        private int hastaID;

        public LoginWindow(string tip)
        {
            InitializeComponent();
            kullaniciTipi = tip;  // önceki mainnwindowdan gelen kullanıcı tipi bilgisi

            // App.config'ten bağlantı cümlesini alıyoruz
            connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        }

        private void BtnGiris_Click(object sender, RoutedEventArgs e)  // aldığı tc ve sifreisne göre ve tipe bakarak hasta doktor paneline yönlendirir
        {
            string tc = TxtTC.Text;
            string girilenSifre = TxtSifre.Password;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Kullanicilar WHERE TCKimlikNo = @tc AND KullaniciTipi = @rol";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tc", tc); //SQL sorgusunda yer alan @tc adlı parametreyi tanımlar.Bu parametreye C# tarafındaki tc değişkeninin değerini atar
                cmd.Parameters.AddWithValue("@rol", kullaniciTipi);

                SqlDataReader reader = cmd.ExecuteReader(); // sorguların satır satır okunması sağlanır

                if (reader.Read())
                {
                    doktorID = Convert.ToInt32(reader["KullaniciID"]); // doktorID'yi(kullanıcılar tablosundan) al

                    byte[] sifreliVeri = (byte[])reader["Sifre"]; //veritabanından Sifre kolonunu aldık
                    //byte[] key, iv;
                    //using (var sha256 = SHA256.Create())
                        // Eski (yanlış):
                        // string cozulmusSifre = AESAlgoritmasi.SifreCoz(sifreliVeri, AESAlgoritmasi.Key, AESAlgoritmasi.IV);

                        // Yeni (doğru):                                                    
                   // byte[] key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("bu1sabahgizli1key1234567890123456"));
                   // byte[] iv = Encoding.UTF8.GetBytes("bu1baslangicivkey123").Take(16).ToArray();

                    string cozulmusSifre = AESAlgoritmasi.SifreCoz(sifreliVeri, AESAlgoritmasi.Key, AESAlgoritmasi.IV);

                    if (cozulmusSifre == girilenSifre)
                    {
                        MessageBox.Show("Giriş Başarılı!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

                        // tek bir login penceremiz olduğu için butonlara tıklayınca aynı login penceresine gidicek ancak 
                        // burda tıkladığı butona göre kullanıcı tipi bilgisini tuttuk. giriş sayfasında ona göre kontrol yapılacaktır 
                        if (kullaniciTipi == "doktor")
                        {
                            doktorID = Convert.ToInt32(reader["KullaniciID"]);
                            DoktorGirisWindow girisWindow = new DoktorGirisWindow(doktorID);  // giris yapcak olan hastanın id sini al
                            girisWindow.Show();
                        }

                        else
                        {
                            hastaID = Convert.ToInt32(reader["KullaniciID"]);
                            HastaGirisWindow girisWindow2 = new HastaGirisWindow(hastaID);
                            girisWindow2.Show();
                        }
                            this.Close(); // Login penceresini kapatmak istersen
                    }
                    else
                    {
                        MessageBox.Show("Şifre hatalı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("TC veya kullanıcı tipi hatalı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}