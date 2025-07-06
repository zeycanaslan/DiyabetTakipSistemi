using System.Configuration;
using System.Windows;

namespace DiyabetTakipSistemi
{
    public partial class HastaGirisWindow : Window
    {
        private int hastaID;
        private string connectionString;

        public HastaGirisWindow(int hastaIdFromLogin)
        {
            InitializeComponent();
            hastaID = hastaIdFromLogin;
            connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            var hosgeldin = new HosgeldinControl(hastaID);
            MainContentArea.Content = hosgeldin;
        }

        private void BtnVeriGiris_Click(object sender, RoutedEventArgs e)
        {
            var veriGirisControl = new VeriGirisControl();
            veriGirisControl.HastaID = hastaID;
            MainContentArea.Content = veriGirisControl;
        }

        private void BtnEgzrszDytTkp_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new EgzersizDiyetTakibiControl(hastaID);
        }

        private void BtnRapor_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new KanSekeriRaporControl(hastaID);
        }

        private void BtnInsulin_Click(object sender, RoutedEventArgs e)
        {
            InsulinTakipControl kontrol = new InsulinTakipControl();
            kontrol.HastaID = hastaID;
            MainContentArea.Content = kontrol;
        }
   }
}