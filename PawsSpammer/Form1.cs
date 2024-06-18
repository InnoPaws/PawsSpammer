using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.UI.WebControls;
using System.Windows.Forms;


namespace PawsSpammer
{
    public partial class Form1 : Form
    {
        private bool isSpamming = false;
        private List<string> webhooks = new List<string>();
        private int delayMilliseconds = 1000; // Default delay of 1 second

        // System Monitor Stuff
        private PerformanceCounter cpuCounter;
        private PerformanceCounter availableMemoryCounter;
        private PerformanceCounter diskReadCounter;
        private PerformanceCounter diskWriteCounter;
        private System.Windows.Forms.Timer timer5; // DO NOT REMOVE THIS OR THE CODE WILL BREAK!

        public Form1()
        {
            InitializeComponent();
            // System Monitor 
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");

            diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
            diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");

            timer5 = new System.Windows.Forms.Timer(); // Initialize System.Windows.Forms.Timer
            timer5.Interval = 1000; // 1 second
            timer5.Tick += timer5_Tick;
            timer5.Start();

            //User Agent Gen Click
            GenUserAgentBtn.Click += GenUserAgentBtn_Click; 

            //Proxy Updater
            timer1.Interval = 500; // 0.5 seconds
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();

            //Error Updater
            timer2.Interval = 500; // 0.5 seconds
            timer2.Tick  += new EventHandler(timer2_Tick);
            timer2.Start();
            
            //Message Updater
            timer3.Interval = 500; // 0.5 seconds
            timer3.Tick += new EventHandler(timer3_Tick);
            timer3.Start();  

            //User Agent Updater
            timer4.Interval = 500; // 0.5 seconds
            timer4.Tick += new EventHandler(timer4_Tick);
            timer4.Start();

            // System Info Grabber - Grabs sys info and puts it in listbox
            GetSystemInfo();

            // Context Menu Strip Code
            listBox2.ContextMenuStrip = contextMenuStrip1;
            listBox1.ContextMenuStrip = contextMenuStrip2;
            listBox3.ContextMenuStrip = contextMenuStrip3;
            LogsListBox.ContextMenuStrip = contextMenuStrip4;

            removeProxyToolStripMenuItem.Click += removeProxyToolStripMenuItem_Click;
            removeWebhookToolStripMenuItem.Click += removeWebhookToolStripMenuItem_Click;
            clearOutputToolStripMenuItem.Click += clearOutputToolStripMenuItem_Click;
            ClearErrorLogtoolStripMenuItem1.Click += ClearErrorLogtoolStripMenuItem1_Click;
        }
        #region User Agent Code
        private static readonly List<string> browsers = new List<string>
    {
        "Chrome/91.0.4472.124",
        "Firefox/89.0",
        "Safari/537.36",
        "Edge/91.0.864.59",
        "Opera/76.0.4017.177",
        "Chrome/90.0.4430.85",
        "Firefox/88.0",
        "Safari/605.1.15"
    };

        private static readonly List<string> os = new List<string>
    {
        "Windows NT 10.0; Win64; x64",
        "Macintosh; Intel Mac OS X 10_15_7",
        "X11; Linux x86_64",
        "iPhone; CPU iPhone OS 14_6 like Mac OS X",
        "Android 10; Mobile",
        "Windows NT 6.1; Win64; x64",
        "Macintosh; Intel Mac OS X 11_2_3",
        "Android 11; Tablet"
    };

        private static readonly List<string> devices = new List<string>
    {
        "AppleWebKit/537.36 (KHTML, like Gecko)",
        "Gecko/20100101",
        "AppleWebKit/605.1.15 (KHTML, like Gecko)",
        "AppleWebKit/534.30 (KHTML, like Gecko)",
        "Gecko/20121011",
        "AppleWebKit/537.36"
    };
        #endregion
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
        private void MoreOptionsBtn_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedIndex = 2;
        }
        private void HackerOptionsBtn_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedIndex = 3;
        }
        private void EventLogsBtn_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedIndex = 4;
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
        private void GenUserAgentBtn_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            string browser = browsers[rand.Next(browsers.Count)];
            string operatingSystem = os[rand.Next(os.Count)];
            string device = devices[rand.Next(devices.Count)];

            string userAgent = $"Mozilla/5.0 ({operatingSystem}) {device} {browser}";

            listBox4.Items.Add(userAgent);
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
                // Start spamming
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

                bool useProxy = checkBox1.Checked && listBox1.Items.Count > 0;
                bool useUserAgent = checkBox2.Checked && listBox4.Items.Count > 0;

                if (useProxy || useUserAgent)
                {
                    await StartSpammingAsync(useProxy, useUserAgent);
                }
                else
                {
                    await StartSpammingAsync(false, false); 
                }
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
        private void ClearErrorLogtoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (LogsListBox.Items != null)
            {
                LogsListBox.Items.Clear();
            }
        }
        private void deleteWebhookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem == null)
            {
                MessageBox.Show("Please select a webhook to delete.");
                return;
            }

            string selectedWebhookUrl = listBox2.SelectedItem.ToString();
            var result = MessageBox.Show($"Are you sure you want to delete the webhook: {selectedWebhookUrl}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string output = ExecuteCurlDelete(selectedWebhookUrl);
                    LogInfo($"Webhook deleted successfully: {output}");
                    listBox2.Items.Remove(selectedWebhookUrl);
                    MessageBox.Show("Webhook deleted successfully.");
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Exception occurred: {ex.Message}";
                    LogError(errorMessage);
                    MessageBox.Show(errorMessage);
                }
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
                LogsListBox.Items.Add($"{DateTime.Now} " + "Error fetching proxies: " + ex.Message);
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
        private void timer4_Tick(object sender, EventArgs e)
        {
            UpdateUserAgentCount();
        }
        private void timer5_Tick(object sender, EventArgs e)
        {
            float cpuUsage = cpuCounter.NextValue();
            float availableMemoryMB = availableMemoryCounter.NextValue();
            float availableMemoryGB = availableMemoryMB / 1024;

            // Get disk read and write bytes per second
            float diskReadBytesPerSec = diskReadCounter.NextValue();
            float diskWriteBytesPerSec = diskWriteCounter.NextValue();

            cpuStatusLabel.Text = $"{cpuUsage:F2}%";
            MemStatusLabel.Text = $"{availableMemoryGB:F2} GB";

            // Display disk read and write activity
            DiskReadStatusLabel.Text = FormatBytes(diskReadBytesPerSec) + "/s";
            DiskWriteStatusLabel.Text = FormatBytes(diskWriteBytesPerSec) + "/s";
        }
        private string FormatBytes(float bytes)
        {
            string[] suffixes = { "B/s", "KB/s", "MB/s", "GB/s", "TB/s" };
            int suffixIndex = 0;
            while (bytes >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                bytes /= 1024;
                suffixIndex++;
            }
            return $"{bytes:F2} {suffixes[suffixIndex]}";
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
        private void UpdateUserAgentCount() 
        { 
            int userAgentCount = listBox4.Items.Count;
            UserAgentCountLabel.Text = $"{userAgentCount}";
        }
        // This is the Main API code 
        private async Task StartSpammingAsync(bool useProxy, bool useUserAgent)
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
                                    await SendWebhookMessage(webhook, message, true, proxy, useUserAgent ? GetRandomUserAgent() : null);
                                    await Task.Delay(delayMilliseconds);

                                    if (!isSpamming)
                                        break;
                                }
                            }
                            else
                            {
                                await SendWebhookMessage(webhook, message, false, null, useUserAgent ? GetRandomUserAgent() : null);
                                await Task.Delay(delayMilliseconds);

                                if (!isSpamming)
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogsListBox.Items.Add($"{DateTime.Now} " + $"Error sending message to {webhook}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogsListBox.Items.Add($"{DateTime.Now} " + $"Error occurred: {ex.Message}");
                isSpamming = false;
                StartAndStopBtn.Text = "Start";
            }
        }
        private async Task SendWebhookMessage(string webhookUrl, string message, bool useProxy, string proxyUrl = null, string userAgent = null, int timeoutMilliseconds = 5000)
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
                        if (!string.IsNullOrEmpty(userAgent) && userAgent != "User Agent Spoofing Disabled")
                        {
                            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                        }

                        var jsonContent = JsonConvert.SerializeObject(new { content = message });
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var cancellationTokenSource = new CancellationTokenSource();
                        cancellationTokenSource.CancelAfter(timeoutMilliseconds);

                        var response = await client.PostAsync(webhookUrl, content, cancellationTokenSource.Token);

                        string logMessage = $"Message sent to {webhookUrl} via {(useProxy ? $"proxy {proxyUrl}" : "Direct")} with {(userAgent == "User Agent Spoofing Disabled" ? "User Agent Spoofing Disabled" : $"User Agent: {userAgent}")}";
                        if (response.IsSuccessStatusCode)
                        {
                            Invoke((Action)(() =>
                            {
                                listBox3.Items.Add($"{DateTime.Now}: " + logMessage);
                            }));
                        }
                        else
                        {
                            Invoke((Action)(() =>
                            {
                                LogsListBox.Items.Add($"{DateTime.Now} " + $"HTTP Error {response.StatusCode} for webhook {webhookUrl} via {(useProxy ? $"proxy {proxyUrl}" : "Direct")} with {(userAgent == "User Agent Spoofing Disabled" ? "User Agent Spoofing Disabled" : $"User Agent: {userAgent}")}: {response.ReasonPhrase}");
                            }));
                        }
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Invoke((Action)(() =>
                {
                    LogsListBox.Items.Add($"{DateTime.Now} " + $"Timeout occurred while sending message to {webhookUrl} via {(useProxy ? $"proxy {proxyUrl}" : "Direct")} with {(userAgent == "User Agent Spoofing Disabled" ? "User Agent Spoofing Disabled" : $"User Agent: {userAgent}")}");
                }));
            }
            catch (HttpRequestException ex)
            {
                Invoke((Action)(() =>
                {
                    LogsListBox.Items.Add($"{DateTime.Now} " + $"HTTP Error: {ex.Message}");

                    if (ex.InnerException != null)
                    {
                        LogsListBox.Items.Add($"{DateTime.Now} " + $"Inner Exception: {ex.InnerException.Message}");
                    }
                }));
            }
            catch (Exception ex)
            {
                Invoke((Action)(() =>
                {
                    LogsListBox.Items.Add($"{DateTime.Now} " + $"Error occurred: {ex.Message}");
                }));
            }
        }
        private string GetRandomUserAgent()
        {
            if (checkBox2.Checked && listBox4.Items.Count > 0)
            {
                Random rand = new Random();
                return listBox4.Items[rand.Next(listBox4.Items.Count)].ToString();
            }
            return "User Agent Spoofing Disabled";
        }
        // THIS IS ONLY FOR THE WEBHOOK DELETER!!! 
        private string ExecuteCurlDelete(string url)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "curl";
                process.StartInfo.Arguments = $"-X DELETE \"{url}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Curl error: {error}");
                }

                return output;
            }
        }

        private void LogError(string message)
        {
            if (LogsListBox.InvokeRequired)
            {
                LogsListBox.Invoke(new Action(() => LogsListBox.Items.Add($"{DateTime.Now}: {message}")));
            }
            else
            {
                LogsListBox.Items.Add($"{DateTime.Now}: {message}");
            }
        }

        private void LogInfo(string message)
        {
            if (listBox3.InvokeRequired)
            {
                listBox3.Invoke(new Action(() => LogsListBox.Items.Add($"{DateTime.Now}: {message}")));
            }
            else
            {
                listBox3.Items.Add($"{DateTime.Now}: {message}");
            }
        }
        // System Info Grabber
        private void GetSystemInfo()
        {
            listBox5.Items.Clear();

            string cpuName = GetCpuName();
            listBox5.Items.Add($"CPU: {cpuName}");

            string memory = GetInstalledMemory();
            listBox5.Items.Add($"Memory: {memory}");
            totalMemoryLabel.Text = memory;

            string[] gpus = GetGpuDetails();
            foreach (var gpu in gpus)
            {
                listBox5.Items.Add($"GPU: {gpu}");
            }

            string[] storageDevices = GetStorageDevices();
            foreach (var storage in storageDevices)
            {
                listBox5.Items.Add($"Storage: {storage}");
            }
        }

        private string GetCpuName()
        {
            string cpuName = "Unknown";
            using (var searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
            {
                foreach (var item in searcher.Get())
                {
                    cpuName = item["Name"].ToString();
                    break;
                }
            }
            return cpuName;
        }

        private string GetInstalledMemory()
        {
            double totalMemory = 0;
            using (var searcher = new ManagementObjectSearcher("select Capacity from Win32_PhysicalMemory"))
            {
                foreach (var item in searcher.Get())
                {
                    totalMemory += Convert.ToDouble(item["Capacity"]);
                }
            }
            return $"{(totalMemory / (1024 * 1024 * 1024)).ToString("F2")} GB";
        }

        private string[] GetGpuDetails()
        {
            var gpus = new System.Collections.Generic.List<string>();
            using (var searcher = new ManagementObjectSearcher("select Name from Win32_VideoController"))
            {
                foreach (var item in searcher.Get())
                {
                    gpus.Add(item["Name"].ToString());
                }
            }
            return gpus.ToArray();
        }

        private string[] GetStorageDevices()
        {
            var storageDevices = new System.Collections.Generic.List<string>();
            using (var searcher = new ManagementObjectSearcher("select Model, Size from Win32_DiskDrive"))
            {
                foreach (var item in searcher.Get())
                {
                    string model = item["Model"].ToString();
                    double size = Convert.ToDouble(item["Size"]);
                    storageDevices.Add($"{model} - {(size / (1024 * 1024 * 1024)).ToString("F2")} GB");
                }
            }
            return storageDevices.ToArray();
        }
        #endregion
    }
}
