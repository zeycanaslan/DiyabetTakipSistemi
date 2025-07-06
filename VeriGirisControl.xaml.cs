using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace DiyabetTakipSistemi
{
    public partial class VeriGirisControl : UserControl
    {
        private int hastaID;
        private string connectionString;

        // Günlük ölçümler -1 değeri henüz ölçüm girilmediğini belirtir.
        double sabah = -1, oglen = -1, ikindi = -1, aksam = -1, gece = -1; 

        public VeriGirisControl()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        }

        // HastaID'yi HastGirisWindowsdan falan alabilmek için
        public int HastaID
        {
            get => hastaID;
            set => hastaID = value;
        }
        private void OlcumKaydet(string olcumZamani, double kanSekeri, string zamanDegeri) // olcumzamanlari:zaman dilimleri
        { //                OlcumKaydet("sabah", double.Parse(txtSabah.Text), "Sabah");
            List<string> zaman_dilimleri = new List<string> { "sabah", "öğle", "ikindi", "akşam", "gece" };

            bool olcum_ortlamaya_dahil = false;

            string olcumZamaniLower = olcumZamani.ToLower();

            double toplam = 0;
            int olcum_sayisi = 0;

            string doktorMesaji = ""; // doktorun ekrana(sistemine) gidecek olan mesaj
            string uyariTipi = "";

            // geçerli zaman dilimi mi kontrolü
            if (!zaman_dilimleri.Contains(olcumZamaniLower))
            {
                uyariTipi = "Zaman Dilimi Uyarısı";
                doktorMesaji = $"{olcumZamani} ölçümü geçersiz zaman diliminde yapıldı. Lütfen belirtilen saat aralıklarında ölçüm yapınız.";
                //geçersiz zaman diliminde ölçüm kaydettik ancak ortalamaya dahil etmiycez
                olcum_ortlamaya_dahil = false;
            }
            else
            {
                olcum_ortlamaya_dahil = true; // dahil değil false ise else'de dahil olmuş olur 

                //kan şekeri değerlerine göre uyarı tipi ve mesaj oluşturuluyor
                if (kanSekeri < 70)
                {
                    uyariTipi = "Acil Uyarı";
                    doktorMesaji = "Hastanın kan şekeri seviyesi 70 mg/dL'nin altına düştü. Hipoglisemi riski! Hızlı müdahale gerekebilir.";
                }
                else if (kanSekeri >= 70 && kanSekeri < 110)
                {
                    uyariTipi = "Uyarı Yok";
                    doktorMesaji = "Kan şekeri seviyesi normal aralıkta. Hiçbir işlem gerekmez.";
                }
                else if (kanSekeri >= 110 && kanSekeri <= 150)
                {
                    uyariTipi = "Takip Uyarısı";
                    doktorMesaji = "Hastanın kan şekeri 111-150 mg/dL arasında. Durum izlenmeli.";
                }

                else if (kanSekeri >= 151 && kanSekeri < 200)
                {
                    uyariTipi = "İzleme Uyarısı";
                    doktorMesaji = "Hastanın kan şekeri 151-200 mg/dL arasında. Diyabet kontrolü gereklidir.";
                }
                else if (kanSekeri >= 200)
                {
                    uyariTipi = "Acil Müdahale Uyarısı";
                    doktorMesaji = "Hastanın kan şekeri 200 mg/dL'nin üzerinde. Hiperglisemi durumu. Acil müdahale gerekebilir.";
                }
            }

            //ölçümü uygun değişkene ata ve ortalama hesaplamaya dahil et
            if (olcum_ortlamaya_dahil) // true
            {
                switch (olcumZamaniLower)
                {
                    case "sabah": sabah = kanSekeri; break;
                    case "öğle":
                    case "öğlen":
                    case "oglen": oglen = kanSekeri; break;
                    case "ikindi": ikindi = kanSekeri; break;
                    case "akşam":
                    case "aksam": aksam = kanSekeri; break;
                    case "gece": gece = kanSekeri; break;
                }
            }

            // gecerli olcumlerle ortalamayı hesapladık
            toplam = 0;
            olcum_sayisi = 0;

            if (sabah != -1) { toplam += sabah; olcum_sayisi++; }
            if (oglen != -1) { toplam += oglen; olcum_sayisi++; }
            if (ikindi != -1) { toplam += ikindi; olcum_sayisi++; }
            if (aksam != -1) { toplam += aksam; olcum_sayisi++; }
            if (gece != -1) { toplam += gece; olcum_sayisi++; }

            if (olcum_sayisi < 5)
            {
                doktorMesaji += "\nÖlçüm eksik! Ortalama alınırken bu ölçüm hesaba katılmadı.";
            }
            if (olcum_sayisi > 0 && olcum_sayisi <= 3)
            {
                doktorMesaji += "\nYetersiz veri! Ortalama hesaplaması güvenilir değildir.";
            }
            else if (olcum_sayisi == 0)
            {
                uyariTipi = "Ölçüm Eksik Uyarısı";
                doktorMesaji = "Hasta gün boyunca kan şekeri ölçümü yapmamıştır. Acil takip önerilir.";
            }

            double ortalama = olcum_sayisi > 0 ? toplam / olcum_sayisi : 0;

            string doz = "";
            if (ortalama < 70) doz = "YOK (Hipoglisemi)";
            else if (ortalama >= 70 && ortalama <= 110) doz = "YOK";
            else if (ortalama >= 111 && ortalama <= 150) doz = "1 ml";
            else if (ortalama >= 151 && ortalama <= 200) doz = "2 ml";
            else doz = "3 ml";

            // Doktor mesajına insülin dozajı önerisi ekle
            doktorMesaji += $"\nÖlçüm ortalaması: {ortalama:F2} mg/dL → Önerilen insülin: {doz}";

            // TEK SEFERDE veriyi kaydet
            KaydetVeri(zamanDegeri, kanSekeri.ToString(), uyariTipi, doktorMesaji, doz);

            txtUyari.Text += uyariTipi + "\n" + doktorMesaji + "\n";
        
        txtUyari.Visibility = Visibility.Visible;
        }

        //KaydetVeri() ile veritabanına kaydeder  OlcumKaydet() ile analiz ve öneri üretir.
        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            // egzersiz ve Diyet durumunu sadece 1 kere, gün içinde kaydet
            KaydetGunlukDurum();

            if (!string.IsNullOrWhiteSpace(txtSabah.Text))
                OlcumKaydet("sabah", double.Parse(txtSabah.Text), "Sabah");

            if (!string.IsNullOrWhiteSpace(txtOgle.Text))
                OlcumKaydet("öğle", double.Parse(txtOgle.Text), "Öğle");

            if (!string.IsNullOrWhiteSpace(txtIkindi.Text))
                OlcumKaydet("ikindi", double.Parse(txtIkindi.Text), "İkindi");

            if (!string.IsNullOrWhiteSpace(txtAksam.Text))
                OlcumKaydet("akşam", double.Parse(txtAksam.Text), "Akşam");

            if (!string.IsNullOrWhiteSpace(txtGece.Text))
                OlcumKaydet("gece", double.Parse(txtGece.Text), "Gece");

            txtDurum.Text = "Veriler başarıyla kaydedildi.";
        }

        private void KaydetVeri(string zamanDilimi, string kanSekeriDegeri, string uyariMesaji, string doktorMesaji, string insulinDozu)
        { //      KaydetVeri(zamanDegeri, kanSekeri.ToString(), uyariTipi, doktorMesaji, doz);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO GunlukVeriler (HastaID, ZamanDilimi, KanSekeri, VeriTarihi, Uyari, DoktorMesaji, InsulinDozu) " +
                               "VALUES (@HastaID, @ZamanDilimi, @Deger, @Tarih, @Uyari, @DoktorMesaji, @InsulinDozu)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HastaID", hastaID);
                    cmd.Parameters.AddWithValue("@ZamanDilimi", zamanDilimi);
                    cmd.Parameters.AddWithValue("@Deger", kanSekeriDegeri);
                    cmd.Parameters.AddWithValue("@Tarih", DateTime.Now.Date);
                    cmd.Parameters.AddWithValue("@DoktorMesaji", doktorMesaji ?? "");
                    cmd.Parameters.AddWithValue("@Uyari", uyariMesaji ?? "");
                    cmd.Parameters.AddWithValue("@InsulinDozu", insulinDozu ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //gunluk egzersiz ve diyet durumunu kaydet
         private void KaydetGunlukDurum()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // aynı günde aynı hastaya ait kayıt varsa yeniden ekleme (gunde bir defa tum kaydı alıcaz)
                string kontrolQuery = "SELECT COUNT(*) FROM GunlukDurumlar WHERE HastaID = @HastaID AND Tarih = @Tarih";
                using (SqlCommand kontrolCmd = new SqlCommand(kontrolQuery, conn))
                {
                    kontrolCmd.Parameters.AddWithValue("@HastaID", hastaID);
                    kontrolCmd.Parameters.AddWithValue("@Tarih", DateTime.Now.Date);

                    int kayitSayisi = (int)kontrolCmd.ExecuteScalar();
                    //eğer aynı gün için zaten kayıt varsa, fonksiyondan çıkılıyor, yani tekrar eklenmiyor.
                    if (kayitSayisi > 0)
                        return; 
                }
                //kayıt yoksa yeni kayıt yapılacak.
                string insertQuery = "INSERT INTO GunlukDurumlar (HastaID, Tarih, EgzersizDurumu, DiyetDurumu) " +
                                     "VALUES (@HastaID, @Tarih, @EgzersizDurumu, @DiyetDurumu)";

                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@HastaID", hastaID);
                    cmd.Parameters.AddWithValue("@Tarih", DateTime.Now.Date);
                    cmd.Parameters.AddWithValue("@EgzersizDurumu", chkEgzersizYapildi.IsChecked == true ? "Yapıldı" : "Yapılmadı");
                    cmd.Parameters.AddWithValue("@DiyetDurumu", chkDiyetUygulandi.IsChecked == true ? "Uygulandı" : "Uygulanmadı");
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
