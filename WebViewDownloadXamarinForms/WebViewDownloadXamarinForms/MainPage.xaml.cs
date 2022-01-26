using Shared;
using Xamarin.Forms;

namespace WebViewDownloadXamarinForms
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            webView.Source = Downloader.StartPage;
        }

    }
}
