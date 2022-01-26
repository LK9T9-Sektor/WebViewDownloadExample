using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shared
{
    public class Downloader
    {
        // Рандомный сайт с картинками, где можно нажать кнопку "Скачать"
        public static string StartPage = "https://unsplash.com/wallpapers";

        public async Task<byte[]> GetFile(string url)
        {
            // Валидация https сертификатов отдельная тема...
            ServicePointManager.ServerCertificateValidationCallback += (o, cert, chain, errors) => true;

            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content;
                        // Расширение файла, можно преобразовать из типа контента
                        var contentType = content.Headers.ContentType;
                        // Имя файла можно достать также из контента, но для этого сперва надо убрать все "спец символы"
                        //var filename = "123.jpg";
                        var result = await content.ReadAsByteArrayAsync();

                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

    }
}
