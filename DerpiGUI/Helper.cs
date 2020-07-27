using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DerpiGUI
{
    class Helper
    {
        public static DerpiObject.Rootobject deserializeJSON(string str)
        {

            return JsonConvert.DeserializeObject<DerpiObject.Rootobject>(str);

        }

        public static async Task<string> Derpibooru(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                string type = "application/json";
                client.BaseAddress = new Uri(url);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(type));
                //add user agent with my info on it, necessary not to receive errors
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DerpiGUI/Discord Hoovier#4192");
                HttpResponseMessage response = await client.GetAsync(String.Empty);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return string.Empty;
            }
        }


        public static async Task DownloadFile(Uri fileUri, string extension, string name, string address)
        {
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(fileUri))
            {
                response.EnsureSuccessStatusCode();
                var stream = await response.Content.ReadAsStreamAsync();

                using (var fileStream = File.Create($@"{address}\{name}.{extension}"))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }

        public static string sort(int index)
        {
            string result = "score";
            switch (index)
            {
                case 0:
                    result = "created_at";
                    break;
                case 1:
                    result = "score";
                    break;
                case 2:
                    result = "wilson";
                    break;
                case 3:
                    result = "relevance";
                    break;
                case 4:
                    result = "random%3A1096362";
                    break;
            }
            return result;
        }
    }
}
