using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;

namespace DiyabetTakipSistemi
{
    public partial class HastaTanimlamaForm : Window
    {
        private int doktorID;
        private byte[] secilenResim;
        public HastaTanimlamaForm(int doktorID)
        {
            InitializeComponent();
            this.doktorID = doktorID;   // bu pencereden paramtre olarak gelen doktorID'yi alır
        }

        private void BtnResimSec_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;";  //  ResimDosyalari : dosya türü için görünen isimdir
            if (ofd.ShowDialog() == true)
            {
                secilenResim = File.ReadAllBytes(ofd.FileName); // bu dosyayı byte dizisine çevirir
            }
        }
        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            string tc = txtTC.Text.Trim();
            string sifre = txtSifre.Password;
            string ad = txtAd.Text.Trim();
            string soyad = txtSoyad.Text.Trim();
            DateTime dogum = dateDogumTarihi.SelectedDate ?? DateTime.Now;
            string cinsiyet = ((ComboBoxItem)cmbCinsiyet.SelectedItem)?.Content?.ToString();
            string eposta = txtEposta.Text.Trim();

            if (string.IsNullOrEmpty(tc) ||
                string.IsNullOrEmpty(sifre) ||
                string.IsNullOrEmpty(ad) ||
                string.IsNullOrEmpty(soyad) ||
                string.IsNullOrEmpty(cinsiyet) ||
                string.IsNullOrEmpty(eposta) ||
                string.IsNullOrWhiteSpace(txtBaslangicKanSekeri.Text) || 
                secilenResim == null ||
                dateDogumTarihi.SelectedDate == null)
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.");
                return;
            }

            // girilen sifreyi sifreleme
            byte[] key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("bu1sabahgizli1key1234567890123456"));
            byte[] iv = Encoding.UTF8.GetBytes("bu1baslangicivkey123").Take(16).ToArray();
            byte[] sifrelenmisSifre = AESAlgoritmasi.Sifrele(sifre, key, iv);

            // belirtilerden hangisi secildi
            List<string> belirtiler = new List<string>();
            if (chkPolifaji.IsChecked == true) belirtiler.Add("polifaji");
            if (chkPolidipsi.IsChecked == true) belirtiler.Add("polidipsi");
            if (chkPoliüri.IsChecked == true) belirtiler.Add("poliüri");
            if (chkYorgunluk.IsChecked == true) belirtiler.Add("yorgunluk");
            if (chkKiloKaybi.IsChecked == true) belirtiler.Add("kilo kaybı");
            if (chkNoropati.IsChecked == true) belirtiler.Add("nöropati");
            if (chkBulanikGorme.IsChecked == true) belirtiler.Add("bulanık görme");
            if (chkYaralar.IsChecked == true) belirtiler.Add("yaraların yavaş iyileşmesi");

            double kanSekeri = double.Parse(txtBaslangicKanSekeri.Text); 
            string diyet = "";
            string egzersiz = "";

            var diyetAciklamalari = new Dictionary<string, string>
            {
                ["Az Şekerli"] = "Şekerli gıdalar sınırlanır, kompleks karbonhidratlara öncelik verilir. " +
                "Lifli gıdalar ve düşük glisemik indeksli besinler tercih edilir. ",
                ["Şekersiz"] = " Rafine şeker ve şeker katkılı tüm ürünler tamamen dışlanır. Hiperglisemi riski taşıyan bireylerde önerilir.",
                ["Dengeli Beslenme"] = "Diyabetli bireylerin yaşam tarzına uygun, dengeli ve sürdürülebilir bir diyet yaklaşımıdır. " +
                "Tüm besin gruplarından yeterli miktarda alınır; porsiyon kontrolü, mevsimsel taze ürünler ve su tüketimi temel unsurlardır. "
            };

            var egzersizAciklamalari = new Dictionary<string, string>
            {
                ["Yürüyüş"] = "Hafif tempolu, günlük yapılabilecek bir egzersizdir.",
                ["Bisiklet"] = "Alt vücut kaslarını çalıştırır ve dış mekanda veya sabit bisikletle uygulanabilir.",
                ["Klinik Egzersiz"] = "Doktor tarafından verilen belirli hareketleri içeren planlı egzersizlerdir." +
                "Stresi azaltılması ve hareket kabiliyetinin artırılması amaçlanır."
            };

            if (kanSekeri < 70)
            {
                if (belirtiler.Contains("nöropati") && belirtiler.Contains("polifaji") && belirtiler.Contains("yorgunluk"))
                {
                    diyet = "Dengeli Beslenme";
                    egzersiz = "Yok";
                }
            }
            else if (kanSekeri >= 70 && kanSekeri < 110)
            {
                if (belirtiler.Contains("polifaji") && belirtiler.Contains("polidipsi"))
                {
                    diyet = "Az Şekerli";
                    egzersiz = "Yürüyüş";
                }
                else if (belirtiler.Contains("yorgunluk") && belirtiler.Contains("kilo kaybı"))
                {
                    diyet = "Dengeli Beslenme";
                    egzersiz = "Yürüyüş";
                }
            }
            else if (kanSekeri >= 110 && kanSekeri < 180)
            {
                if (belirtiler.Contains("bulanık görme") && belirtiler.Contains("nöropati"))
                {
                    diyet = "Az Şekerli";
                    egzersiz = "Klinik Egzersiz";
                }
                else if (belirtiler.Contains("poliüri") && belirtiler.Contains("polidipsi"))
                {
                    diyet = "Şekersiz";
                    egzersiz = "Klinik Egzersiz";
                }
                else if (belirtiler.Contains("yorgunluk") && belirtiler.Contains("nöropati") && belirtiler.Contains("bulanık görme"))
                {
                    diyet = "Az Şekerli";
                    egzersiz = "Yürüyüş";
                }
            }
            else if (kanSekeri >= 180)
            {
                if (belirtiler.Contains("yaraların yavaş iyileşmesi") && belirtiler.Contains("polifaji") && belirtiler.Contains("polidipsi"))
                {
                    diyet = "Şekersiz";
                    egzersiz = "Klinik Egzersiz";
                }
                else if (belirtiler.Contains("yaraların yavaş iyileşmesi") && belirtiler.Contains("kilo kaybı"))
                {
                    diyet = "Şekersiz";
                    egzersiz = "Yürüyüş";
                }
            }

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ConnectionString))
                {
                    conn.Open();

                    // E-posta kontrolü
                    string checkQuery = "SELECT COUNT(*) FROM Kullanicilar WHERE Eposta = @eposta";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@eposta", eposta);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Bu e-posta zaten kayıtlı.");
                        return;
                    }

                    //Kullanicilar tablosuna kayıt
                    string query = @"INSERT INTO Kullanicilar 
                                    (TCKimlikNo, Sifre, Ad, Soyad, DogumTarihi, Cinsiyet, Eposta, ProfilResmi, KullaniciTipi)
                                    OUTPUT INSERTED.KullaniciID
                                    VALUES (@tc, @sifre, @ad, @soyad, @dogum, @cinsiyet, @eposta, @resim, 'Hasta')";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@tc", tc);
                    cmd.Parameters.AddWithValue("@sifre", sifrelenmisSifre);
                    cmd.Parameters.AddWithValue("@ad", ad);
                    cmd.Parameters.AddWithValue("@soyad", soyad);
                    cmd.Parameters.AddWithValue("@dogum", dogum);
                    cmd.Parameters.AddWithValue("@cinsiyet", cinsiyet);
                    cmd.Parameters.AddWithValue("@eposta", eposta);
                    cmd.Parameters.AddWithValue("@resim", secilenResim);

                    int kullaniciID = Convert.ToInt32(cmd.ExecuteScalar());

                    //Hastalar tablosuna ilişkilendirme
                    string hastaQuery = "INSERT INTO Hastalar (HastaID, DoktorID) VALUES (@hastaID, @doktorID)";
                    SqlCommand hastaCmd = new SqlCommand(hastaQuery, conn);
                    hastaCmd.Parameters.AddWithValue("@hastaID", kullaniciID);
                    hastaCmd.Parameters.AddWithValue("@doktorID", doktorID);
                    hastaCmd.ExecuteNonQuery();

                //Kan şekeri başlangıç değerini alıp KanSekeriOlcumleri tablosuna ekleme
                if (decimal.TryParse(txtBaslangicKanSekeri.Text, out decimal baslangicDeger))
                    {
                        string insertKanSekeriQuery = @"INSERT INTO KanSekeriOlcumleri 
                                                (HastaID, OlcumDegeri, OlcumTarihi, ZamanDilimi, GonderenTipi) 
                                                VALUES (@HastaID, @OlcumDegeri, @OlcumTarihi, @ZamanDilimi, @GonderenTipi)";

                        using (SqlCommand cmdKan = new SqlCommand(insertKanSekeriQuery, conn))
                        {
                            cmdKan.Parameters.AddWithValue("@HastaID", kullaniciID);
                            cmdKan.Parameters.AddWithValue("@OlcumDegeri", baslangicDeger);
                            cmdKan.Parameters.AddWithValue("@OlcumTarihi", DateTime.Now);
                            cmdKan.Parameters.AddWithValue("@ZamanDilimi", "Doktor İlk Giriş");
                            cmdKan.Parameters.AddWithValue("@GonderenTipi", "Doktor");  // doktor kayıt yaptığı için direkt sabit doktor giriyoz (gerek yok)
                            cmdKan.ExecuteNonQuery();
                        }
                    }
                //diyet planını DiyetPlanlari tablosuna ekleme
                if (!string.IsNullOrEmpty(diyet))
                    {
                    string aciklama = diyetAciklamalari.ContainsKey(diyet) ? diyetAciklamalari[diyet] : ""; 

                    string insertDiyet = "INSERT INTO DiyetPlanlari (HastaID, DiyetTuru, PlanTarihi, BaslangicTarihi, Aciklama) " +
                                              "VALUES (@HastaID, @DiyetTuru, @Tarih, @BaslangicTarihi, @Aciklama)";
                        using (SqlCommand cmdDiyet = new SqlCommand(insertDiyet, conn))
                        {
                            cmdDiyet.Parameters.AddWithValue("@HastaID", kullaniciID);
                            cmdDiyet.Parameters.AddWithValue("@DiyetTuru", diyet);  // yukarda belirtilen diyetleri sutun adına atadık
                            cmdDiyet.Parameters.AddWithValue("@Tarih", DateTime.Now);
                            cmdDiyet.Parameters.AddWithValue("@BaslangicTarihi", DateTime.Now);
                            cmdDiyet.Parameters.AddWithValue("@Aciklama", aciklama);
                            cmdDiyet.ExecuteNonQuery();
                        }
                    }

                // egzersiz planını EgzersizPlanlari tablosuna ekleme
                if (!string.IsNullOrEmpty(egzersiz) && egzersiz != "Yok")
                    {
                    string aciklama = egzersizAciklamalari.ContainsKey(egzersiz) ? egzersizAciklamalari[egzersiz] : "";

                    string insertEgzersiz = "INSERT INTO EgzersizPlanlari (HastaID, EgzersizTuru, PlanTarihi,BaslangicTarihi, Aciklama) " +
                                                "VALUES (@HastaID, @EgzersizTuru, @Tarih, @BaslangicTarihi, @Aciklama)";
                        using (SqlCommand cmdEgzersiz = new SqlCommand(insertEgzersiz, conn))
                        {
                            cmdEgzersiz.Parameters.AddWithValue("@HastaID", kullaniciID);
                            cmdEgzersiz.Parameters.AddWithValue("@EgzersizTuru", egzersiz);
                            cmdEgzersiz.Parameters.AddWithValue("@Tarih", DateTime.Now);
                            cmdEgzersiz.Parameters.AddWithValue("@BaslangicTarihi", DateTime.Now);
                            cmdEgzersiz.Parameters.AddWithValue("@Aciklama", aciklama);
                            cmdEgzersiz.ExecuteNonQuery();
                        }
                    }

                // Belirtileri kaydetme
                if (belirtiler.Count > 0)
                {
                    foreach (string belirti in belirtiler)
                    {
                        string insertBelirti = "INSERT INTO Belirtiler (HastaID, Belirti, BelirtiTarihi) VALUES (@HastaID, @Belirti, @Tarih)";
                        using (SqlCommand cmdBelirti = new SqlCommand(insertBelirti, conn))
                        {
                            cmdBelirti.Parameters.AddWithValue("@HastaID", kullaniciID);
                            cmdBelirti.Parameters.AddWithValue("@Belirti", belirti);
                            cmdBelirti.Parameters.AddWithValue("@Tarih", DateTime.Now);
                            cmdBelirti.ExecuteNonQuery();
                        }
                    }

                }
                conn.Close();

            }

            // Kayıt başarılıysa kullanıcıya bilgi mesajı göster
            string mesaj = $"Önerilen Diyet Türü: {diyet}\n" +
                           $"Önerilen Egzersiz Türü: {egzersiz}\n\n" +
                           "Bilgiler hastanın sistemine başarıyla kaydedilmiştir.";

            MessageBox.Show(mesaj, "Kayıt Başarılı", MessageBoxButton.OK, MessageBoxImage.Information); this.Close();

            // maile sifre ve tc bilgilerini gönderme
            try
            {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress("zeyacnaslan7@gmail.com");
                    mail.To.Add(eposta);
                    mail.Subject = "Kayıt Bilgileriniz";
                    mail.Body = $"Sayın {ad} {soyad},\n\nSisteme kaydınız başarıyla yapılmıştır.\n\nTC Kimlik No: {tc}\nGeçici Şifre: {sifre}\n\nLütfen giriş yaptıktan sonra şifrenizi değiştiriniz.\n\nİyi günler dileriz.";

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                    smtp.Credentials = new NetworkCredential("zeycanaslan7@gmail.com", "znzg jxqo eami gmxv\r\n");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("E-posta gönderilemedi: " + ex.Message);
                }
                this.Close();
            }
        }
    }
