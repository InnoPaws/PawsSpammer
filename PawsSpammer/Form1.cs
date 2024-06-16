using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;


namespace PawsSpammer
{
    public partial class Form1 : Form
    {
        private bool isSpamming = false;
        private List<string> webhooks = new List<string>();
        private int delayMilliseconds = 1000; // Default delay of 1 second
        public Form1()
        {
            InitializeComponent();
            //Proxy Updater
            timer1.Interval = 500; // 0.5 second
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();

            //Error Updater
            timer2.Interval = 500; // 0.5 second
            timer2.Tick  += new EventHandler(timer2_Tick);
            timer2.Start();
            
            //Message Updater
            timer3.Interval = 500; // 0.5 second
            timer3.Tick += new EventHandler(timer3_Tick);
            timer3.Start();          

            // Context Menu Strip Code
            listBox2.ContextMenuStrip = contextMenuStrip1;
            listBox1.ContextMenuStrip = contextMenuStrip2;
            listBox3.ContextMenuStrip = contextMenuStrip3;

            removeProxyToolStripMenuItem.Click += removeProxyToolStripMenuItem_Click;
            removeWebhookToolStripMenuItem.Click += removeWebhookToolStripMenuItem_Click;
            clearOutputToolStripMenuItem.Click += clearOutputToolStripMenuItem_Click;
        }
        #region Buttons Code
        private async void fetchButton_Click(object sender, EventArgs e)
        {
            await FetchProxies();
        }
        private void HomeBtn_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedIndex = 0;
        }
        private void SettingsBtn_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedIndex = 1;
        }
        private void EventLogsBtn_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedIndex = 2;
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                DialogResult result = MessageBox.Show("By enabling the proxy feature, your IP will be protected from being limited by Discord.\nHowever, using proxies may slow down sending and lead to more errors.\n\nAre you sure you want to enable this?", "About", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    
                }
                else
                {
                    checkBox1.Checked = false;
                }
            }

        }
        private void HelpBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This delays the sending to the api and less api limited.\n1000 (millisecond) is 1 second is the defualt value to make it fast but this will cause a api limit error", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void AddWebhookBtn_Click(object sender, EventArgs e)
        {
            string textToAdd = webhookTextBox.Text.Trim();
            if (string.IsNullOrEmpty(textToAdd))
            {
                MessageBox.Show("Please enter a webhook to add.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogsListBox.Items.Add("Error Webhook textbox null or empty!");
                return;
            }
            listBox2.Items.Add(textToAdd);
            webhookTextBox.Text = "";
        }
        private async void StartAndStopBtn_Click(object sender, EventArgs e)
        {
            if (!isSpamming)
            {
                if (listBox2.Items.Count == 0)
                {
                    MessageBox.Show("Add at least one webhook to start spamming.");
                    return;
                }

                webhooks.Clear();
                foreach (var item in listBox2.Items)
                {
                    webhooks.Add(item.ToString());
                }

                if (!int.TryParse(DelayTextBox.Text, out delayMilliseconds) || delayMilliseconds <= 0)
                {
                    MessageBox.Show("Invalid delay value. Please enter a valid positive integer.");
                    return;
                }

                isSpamming = true;
                StartAndStopBtn.Text = "Stop";

                await StartSpammingAsync(checkBox1.Checked); 
            }
            else
            {
                // Stop spamming
                isSpamming = false;
                StartAndStopBtn.Text = "Start";
            }
        }

        private void removeWebhookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItems != null)
            {
                listBox2.Items.Remove(listBox2.SelectedItem);
            }
        }
        private void removeProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems != null)
            {
                listBox1.Items.Remove(listBox1.SelectedItem);
            }

        }
        private void clearOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox3.Items != null) 
            {
                listBox3.Items.Clear();
            }
        }
        #endregion

        #region Main Code
        private async Task FetchProxies()
        {
            
            string url = $"{ProxyAPIText.Text}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync(url);
                    string[] proxies = response.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    listBox1.Items.Clear();

                    foreach (string proxy in proxies)
                    {
                        listBox1.Items.Add(proxy);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching proxies: " + ex.Message);
                LogsListBox.Items.Add("Error fetching proxies: " + ex.Message);
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateProxyCount();
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            UpdateErrorCount();
        }
        private void timer3_Tick(object sender, EventArgs e)
        {
            UpdateMessageCount();
        }
        private void UpdateProxyCount()
        {
            int proxyCount = listBox1.Items.Count;
            ProxyCount.Text = $"{proxyCount}";
        }
        private void UpdateErrorCount() 
        {
            int errorCount = LogsListBox.Items.Count;
            ErrorCounts.Text = $"{errorCount}";
        }
        private void UpdateMessageCount() 
        {
            int messagcount = listBox3.Items.Count;
            ValidCounts.Text = $"{messagcount}";
        }

        private async Task StartSpammingAsync(bool useProxy)
        {
            var message = richTextBox1.Text;

            try
            {
                while (isSpamming)
                {
                    foreach (var webhook in webhooks)
                    {
                        try
                        {
                            if (useProxy)
                            {
                                foreach (var proxyItem in listBox1.Items)
                                {
                                    var proxy = proxyItem.ToString();
                                    await SendWebhookMessage(webhook, message, true, proxy);
                                    await Task.Delay(delayMilliseconds);

                                    if (!isSpamming)
                                        break;
                                }
                            }
                            else
                            {
                                await SendWebhookMessage(webhook, message, false);
                                await Task.Delay(delayMilliseconds);

                                if (!isSpamming)
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogsListBox.Items.Add($"Error sending message to {webhook}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogsListBox.Items.Add($"Error occurred: {ex.Message}");
                isSpamming = false;
                StartAndStopBtn.Text = "Start";
            }
        }

        private async Task SendWebhookMessage(string webhookUrl, string message, bool useProxy, string proxyUrl = null, int timeoutMilliseconds = 5000)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            try
            {
                using (var httpClientHandler = new HttpClientHandler())
                {
                    if (useProxy && !string.IsNullOrEmpty(proxyUrl))
                    {
                        httpClientHandler.Proxy = new WebProxy(proxyUrl);
                        httpClientHandler.UseProxy = true;
                    }

                    using (var client = new HttpClient(httpClientHandler))
                    {
                        var jsonContent = JsonConvert.SerializeObject(new { content = message });
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var cancellationTokenSource = new CancellationTokenSource();
                        cancellationTokenSource.CancelAfter(timeoutMilliseconds);

                        var response = await client.PostAsync(webhookUrl, content, cancellationTokenSource.Token);

                        if (response.IsSuccessStatusCode)
                        {
                            Invoke((Action)(() =>
                            {
                                listBox3.Items.Add($"Message sent to {webhookUrl} via proxy {proxyUrl ?? "Direct"}");
                            }));
                        }
                        else
                        {
                            Invoke((Action)(() =>
                            {
                                LogsListBox.Items.Add($"HTTP Error {response.StatusCode} for webhook {webhookUrl} via proxy {proxyUrl ?? "Direct"}: {response.ReasonPhrase}");
                            }));
                        }
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Invoke((Action)(() =>
                {
                    LogsListBox.Items.Add($"Timeout occurred while sending message to {webhookUrl} via proxy {proxyUrl ?? "Direct"}");
                }));
            }
            catch (HttpRequestException ex)
            {
                Invoke((Action)(() =>
                {
                    LogsListBox.Items.Add($"HTTP Error: {ex.Message}");

                    if (ex.InnerException != null)
                    {
                        LogsListBox.Items.Add($"Inner Exception: {ex.InnerException.Message}");
                    }
                }));
            }
            catch (Exception ex)
            {
                Invoke((Action)(() =>
                {
                    LogsListBox.Items.Add($"Error occurred: {ex.Message}");
                }));
            }
        }
        #endregion
    }
}
