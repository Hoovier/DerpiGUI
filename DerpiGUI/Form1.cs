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

        private static async Task<string> Derpibooru(string url)
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
            comboBox1.SelectedIndex = 0;
            label2.Text = "Rating: Safe";
            pictureBox1.Image = Properties.Resources.instructions;
        }

        private async void Search_Click(object sender, EventArgs e)
        {
            
            DerpiObject.Rootobject results = deserializeJSON(await Derpibooru(rating(Input.Text, 1, GetSort())));
            List<DerpiObject.Image> searches = new List<DerpiObject.Image>();
            searches.AddRange(results.images.ToList());
            Random rand = new Random();
            
            
            label1.Text = $"Total Results: {results.total.ToString()}";
            if (results.total == 0)
            {
                output.Text = "No results! Either your tags are misspelled or no images match your search.";
            }
            else
            {

                DerpiObject.Image chosenImage = searches.ElementAt(rand.Next(searches.Count));
                var displayImage = String.Join(",", chosenImage.tags);
                string cleanLink = $"https://derpicdn.net/img/view/{chosenImage.created_at.Date.ToString("yyyy/M/d")}/{chosenImage.id}.{chosenImage.format.ToLower()}";
                pictureBox1.Load($"{chosenImage.view_url}");
                richTextBox1.Text = cleanLink;
                output.Text = displayImage;
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
            string key;
            if (KeyBox.Text == "")
            {
                key = $"";
            }
            else
            {
                key = $"&key={KeyBox.Text}";
            }
            string rating = "https://derpibooru.org/api/v1/json/search/images?q=pinkie+pie";
           switch(trackBar1.Value)
            {
                case 1:
                    rating = $"https://derpibooru.org/api/v1/json/search/images?q={tags}+AND+safe&filter_id=160747{key}&sf={sort}&sd=desc&per_page=50&page={pageNumber}";
                    
                    break;
                case 2:
                    rating = $"https://derpibooru.org/api/v1/json/search/images?q={tags}+AND+%28safe+OR+suggestive%29{key}&filter_id=56027&sf={sort}&sd=desc&per_page=50&page={pageNumber}";
                   
                    break;
                case 3:
                    rating = $"https://derpibooru.org/api/v1/json/search/images?q={tags}+AND+%28safe+OR+questionable+OR+suggestive%29{key}&filter_id=56027&sf={sort}&sd=desc&per_page=50&page={pageNumber}";
                    
                    break;
                case 4:
                    rating = $"https://derpibooru.org/api/v1/json/search/images?q={tags}&key=TSgfZUMEhyV7YdzBC-sD&filter_id=56027{key}&sf={sort}&sd=desc&per_page=50&page={pageNumber}";
               
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
            DerpiObject.Rootobject response = deserializeJSON(await Derpibooru(rating(Input.Text, 1, GetSort())));
            List<DerpiObject.Image> searches = new List<DerpiObject.Image>();
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
                    
                    response = deserializeJSON(await Derpibooru(rating(Input.Text, pages,GetSort())));
                    searches.AddRange(response.images);
                    
                    
                    foreach (DerpiObject.Image i in searches)
                    {
                        Uri link = new Uri($"{i.view_url}");
                        string filenameFixed = filename.Text.Replace("*", u.ToString());
                        await DownloadFile(link, i.format, filenameFixed, location);
                        u++;
                        pictureBox1.Load(location + @"\" +  filenameFixed + "." + i.format);
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
                searches.AddRange(response.images);
                foreach (DerpiObject.Image i in searches)
                {
                    Uri link = new Uri($"{i.view_url}");
                    output.Text = "Downloading!";
                    await DownloadFile(link, i.format, $"{filename.Text.Replace("*", u.ToString())}", location);
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
        string oldText ="";
        private void label6_MouseHover(object sender, EventArgs e)
        {
            oldText = output.Text;

            TextBox temp = sender as TextBox;
            string chosenTemplate = temp.Name;
            switch (chosenTemplate)
            {
                case "filename":
                    output.Text = "Please input a name for your files, with an asterisk to be replaced by the file number, Ex: 'MyFavorites#*'  ";
                    break;
                case "KeyBox":
                    output.Text = "Optional slot for your derpibooru API key. This allows you to access your own favorites or watched!";
                    break;
            }
        }

        private void textBox1_MouseLeave(object sender, EventArgs e)
        {
            output.Text = oldText;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {   
            this.linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://derpibooru.org/users/edit");
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.instructions;
        }
    }
    }

