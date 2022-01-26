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
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace WebViewDownloadExample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IDownloadListener
    {
        // Рандомный сайт с картинками, где можно нажать кнопку "Скачать"
        private const string _startPage = "https://unsplash.com/wallpapers";

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
            webview.LoadUrl(_startPage);

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
                Download(url);
            }
        }

        private async void Download(string url)
        {
            // Валидация https сертификатов отдельная тема...
            ServicePointManager.ServerCertificateValidationCallback += (o, cert, chain, errors) => true;

            try
            {
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content;
                    // Расширение файла, можно преобразовать из типа контента
                    var contentType = content.Headers.ContentType;
                    // Имя файла можно достать также из контента, но для этого сперва надо убрать все "спец символы"
                    var filename = "123.jpg";
                    var result = await content.ReadAsByteArrayAsync();

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

                    outPutStream.Write(result);
                    outPutStream.Flush();
                    outPutStream.Close();

                    Toast.MakeText(ApplicationContext, $"Файл сохранён: {filename}", ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(ApplicationContext, $"{url} not loaded. Code: {response.StatusCode}", ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(ApplicationContext, $"Oops something goes wrong! {ex.Message}", ToastLength.Long).Show();
            }
        }

    }
}