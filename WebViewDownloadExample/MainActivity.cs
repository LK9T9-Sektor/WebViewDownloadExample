using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Webkit;
using Android.Widget;
using Java.IO;
using Shared;
using System;
using System.IO;

namespace WebViewDownloadExample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IDownloadListener
    {
        private readonly Downloader _downloader = new Downloader();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            var webview = FindViewById<WebView>(Resource.Id.webView1);

            var webSettings = webview.Settings;
            webSettings.SetSupportMultipleWindows(true);
            webSettings.SetEnableSmoothTransition(true);
            webSettings.JavaScriptEnabled = true;
            webSettings.DomStorageEnabled = true;
            webSettings.AllowFileAccessFromFileURLs = true;

            webview.SetWebViewClient(new WebViewClient());
            webview.LoadUrl(Downloader.StartPage);

            webview.SetDownloadListener(this);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OnDownloadStart(string url, string userAgent, string contentDisposition, string mimetype, long contentLength)
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, 100);
            }
            else
            {
                // С загрузкой через DownloadManager есть какие-то проблемы, воспользуемся обычным HttpClient'ом
                try
                {
                    Toast.MakeText(ApplicationContext, $"Загрузка файла...", ToastLength.Long).Show();

                    StartDownload(url);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(ApplicationContext, $"Oops something goes wrong! {ex.Message}", ToastLength.Long).Show();
                }
            }
        }

        private async void StartDownload(string url)
        {
            var result = await _downloader.GetFile(url);
            if (result != null)
            {
                Toast.MakeText(ApplicationContext, $"Загрузка файла завершена", ToastLength.Long).Show();
                SaveFile(result);
            }
        }


        private async void SaveFile(byte[] content)
        {
            var filename = "322.jpg";

            var saveFolder = string.Empty;
            if (Android.OS.Environment.IsExternalStorageEmulated)
            {
                saveFolder = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
                    Android.OS.Environment.DirectoryDownloads);
            }
            else
            {
                saveFolder = Android.OS.Environment.DirectoryDownloads;
            }

            var file = new Java.IO.File(saveFolder, filename);
            var outPutStream = new FileOutputStream(file);

            await outPutStream.WriteAsync(content);
            outPutStream.Flush();
            outPutStream.Close();

            Toast.MakeText(ApplicationContext, $"Файл {filename} сохранён!", ToastLength.Long).Show();
        }

    }
}