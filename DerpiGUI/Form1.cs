using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace DerpiGUI
{
    public partial class DerpiGUI : Form
    {
        public DerpiGUI()
        {
            InitializeComponent();
        }

        private DerpiObject.Rootobject deserializeJSON(string str)
        {
           
                return JsonConvert.DeserializeObject<DerpiObject.Rootobject>(str);
 
        }

        private static async Task<string> Derpibooru(string link)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(link).ConfigureAwait(continueOnCapturedContext: false) ;
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }

                catch (HttpRequestException e)
                {
                    //lazy work around, displays 0 results when user puts in invalid search. Will later fix!
                    HttpResponseMessage response = await client.GetAsync("https://derpibooru.org/search.json?q=pinkie+pie+AND+NOT+pinkie+pie").ConfigureAwait(continueOnCapturedContext: false);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
            }

        }

       
        private static async Task DownloadFile(Uri fileUri, string extension, string name, string address)
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

        private void DerpiGUI_Load(object sender, EventArgs e)
        {

        }

        private void Search_Click(object sender, EventArgs e)
        {
            DerpiObject.Rootobject results = deserializeJSON(Derpibooru(rating(Input.Text, 1, GetSort())).Result);
            List<DerpiObject.Search> searches = new List<DerpiObject.Search>();
            searches.AddRange(results.search.ToList());
            Random rand = new Random();
            DerpiObject.Search chosenImage = searches.ElementAt(rand.Next(searches.Count));
            
            label1.Text = $"Total Results: {results.total.ToString()}";
            if (results.total == 0)
            {
                output.Text = "No results! Either your tags are misspelled or no images match your search.";
            }
            else
            {
                var pony = chosenImage.tags;
                Uri clocky = new Uri($"https:{chosenImage.image}");
                string extensions = $"https://derpicdn.net/img/view/{chosenImage.created_at.Date.ToString("yyyy/M/d")}/{chosenImage.id}.{chosenImage.original_format}";
                pictureBox1.Load($"https:{chosenImage.image}");
                richTextBox1.Text = extensions;
                output.Text = pony;
            }
        }
        //this function prevents users from submitting an empty query
        private void Input_TextChanged(object sender, EventArgs e)
        {
            
            
            if(Input.Text == "")
            {
                Search.Enabled = false;
            }
            else if(Input.Text != "")
            {
                Search.Enabled = true;
            }
            
        }

        public string rating(string tags, int pageNumber, string sort)
        {
            string rating = "https://derpibooru.org/search.json?q=pinkie+pie";
           switch(trackBar1.Value)
            {
                case 1:
                    rating = $"https://derpibooru.org/search.json?q={tags}+AND+safe&filter_id=160747&sf={sort}&sd=desc&perpage=50&page={pageNumber}";
                    
                    break;
                case 2:
                    rating = $"https://derpibooru.org/search.json?q={tags}+AND+%28safe+OR+suggestive%29&filter_id=56027&sf={sort}&sd=desc&perpage=50&page={pageNumber}";
                   
                    break;
                case 3:
                    rating = $"https://derpibooru.org/search.json?q={tags}+AND+%28safe+OR+questionable+OR+suggestive%29&filter_id=56027&sf={sort}&sd=desc&perpage=50&page={pageNumber}";
                    
                    break;
                case 4:
                    rating = $"https://derpibooru.org/search.json?q={tags}&key=TSgfZUMEhyV7YdzBC-sD&filter_id=56027&sf={sort}&sd=desc&perpage=50&page={pageNumber}";
               
                    break;
                
            }
            return rating;

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            switch (trackBar1.Value)
            {
                case 1:
                   
                    label2.Text = "Safe";
                    break;
                case 2:
                   
                    label2.Text = "Suggestive";
                    break;
                case 3:
                    
                    label2.Text = "Questionable";
                    break;
                case 4:
                   
                    label2.Text = "Everything";
                    break;

            }
        }
        //makes the link under image clickable
        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
            
        }

        private async void download_ClickAsync(object sender, EventArgs e)
        {
            
            string location = "no path";
            output.Text = "Beginning...";
            DerpiObject.Rootobject response = deserializeJSON(Derpibooru(rating(Input.Text, 0, GetSort())).Result);
            List<DerpiObject.Search> searches = new List<DerpiObject.Search>();
            int num_pages = response.total / 50;

            if(response.total % 50 > 0)
            {
                num_pages++;
            }
            int u = 0;
            if (response.total == 0)
            {
                output.Text = "Nothing to download!";
            }
            else  if (response.total >= 50)
            {
                folderBrowserDialog1.ShowDialog();
                location = folderBrowserDialog1.SelectedPath;
                for (int pages = 1; pages <= (num_pages); pages++)
                {
                    
                    response = deserializeJSON(Derpibooru(rating(Input.Text, pages,GetSort())).Result);
                    searches.AddRange(response.search.ToList());
                    
                    
                    foreach (DerpiObject.Search i in searches)
                    {
                        Uri link = new Uri($"https:{i.image}");
                        
                        await DownloadFile(link, i.original_format, $"testing{u}", location);
                        u++;
                        
                            output.Text = $"{u} out of {response.total}";
                        
                    }
                    searches.Clear();
                }
            }
            else if (response.total > 0 && response.total <= 50)
            {
                folderBrowserDialog1.ShowNewFolderButton = true;
                folderBrowserDialog1.ShowDialog();
                location = folderBrowserDialog1.SelectedPath;
                searches.AddRange(response.search.ToList());
                foreach (DerpiObject.Search i in searches)
                {
                    Uri link = new Uri($"https:{i.image}");
                    output.Text = "Downloading!";
                    await DownloadFile(link, i.original_format, $"{filename.Text.Replace("*", u.ToString())}", location);
                    u++;
                    if (u <= response.total)
                    {
                        output.Text = $"{u} out of {response.total}";
                    }
                }
            }

        }
        //checks for correct filename
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (filename.Text == "" || Input.Text == "")
            {
                download.Enabled = false;
            }
            else if (filename.Text != "" && Input.Text != "" && filename.Text.Contains("*"))
            {
                output.Text = "Ready to go!";
                download.Enabled = true;
            }
            else if (filename.Text != "" && Input.Text != "" && !filename.Text.Contains("*"))
            {
                output.Text = "Please put an asterisk (*) within the filename.";
            }
        }

        private string GetSort()
        {
            string result = "score";
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    result = "created_at";
                    break;
                case 1:
                    result = "wilson";
                    break;
                case 2:
                    result = "relevance";
                    break;
                case 3:
                    result = "random%3A1096362";
                    break;
            }
            return result;
        }
    }
    }

