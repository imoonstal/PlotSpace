using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;

namespace pSpace
{
    public partial class MainWindow : Window
    {
        Process proc;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (proc != null)
            {
                System.Windows.MessageBox.Show("P盘进程已开启.");
                return;
            }
            SaveConfig();
            var path = txt_exepath.Text.Trim();
            var arguments = GetParam();
            Task.Run(() => PSpace(path, arguments));
        }


        private string GetParam()
        {
            
            string arguments = $" plots create";
            if (!string.IsNullOrWhiteSpace(txt_n.Text.Trim()))
                arguments += $" -n {txt_n.Text.Trim()}";
            if (!string.IsNullOrWhiteSpace(txt_size.Text.Trim()))
                arguments += $" -k {txt_size.Text.Trim()}";
            if (!string.IsNullOrWhiteSpace(txt_memory_cache.Text.Trim()))
                arguments += $" -b {txt_memory_cache.Text.Trim()}";
            if (!string.IsNullOrWhiteSpace(txt_thread_count.Text.Trim()))
                arguments += $" -r {txt_thread_count.Text.Trim()}";
            if (!string.IsNullOrWhiteSpace(txt_buckets.Text.Trim()))
                arguments += $" -u {txt_buckets.Text.Trim()}";

            if (!string.IsNullOrWhiteSpace(txt_farmer_public_key.Text.Trim()))
                arguments += $" -f {txt_farmer_public_key.Text.Trim()}";
            if (!string.IsNullOrWhiteSpace(txt_pool_public_key.Text.Trim()))
                arguments += $" -p {txt_pool_public_key.Text.Trim()}";
            if (!string.IsNullOrWhiteSpace(txt_fingerprint.Text.Trim()))
                arguments += $" -a {txt_fingerprint.Text.Trim()}";

            if (!string.IsNullOrWhiteSpace(txt_cache_path.Text.Trim()))
                arguments += $" -t {txt_cache_path.Text.Trim()}";
            if (!string.IsNullOrWhiteSpace(txt_final_path.Text.Trim()))
                arguments += $" -d {txt_final_path.Text.Trim()}";
            return arguments;
        }
        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() ==  System.Windows.Forms.DialogResult.OK)
            {
                txt_cache_path.Text = dialog.SelectedPath;
            }
        }

        private void SelectFile_Click1(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_final_path.Text = dialog.SelectedPath;
            }
        }

        private void WriteToTextBox(string txt)
        {
            Dispatcher.Invoke(new Action(() => this.TbResult.AppendText($"{txt}\r\n")));
        }

        private void PSpace(string exe,string arguments)
        {

            ProcessStartInfo procStartInfo = new ProcessStartInfo(exe, arguments);
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            procStartInfo.StandardOutputEncoding = Encoding.UTF8;
            proc = new Process();
            try
            {
                proc.StartInfo = procStartInfo;
                proc.Start();
                using (StreamReader reader = proc.StandardOutput)
                {
                    string line = reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        WriteToTextBox(line);
                        line = reader.ReadLine();
                    }
                }
                proc.WaitForExit();
            }
            finally
            {
                proc.Close();
            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (proc != null)
                proc.Kill();
        }

        private void Button_GetParam(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            var path = txt_exepath.Text.Trim();
            var arguments = GetParam();
            WriteToTextBox(path + arguments);
        }

        private void SelectFile_Click3(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "请选择文件";
            dialog.Filter = "所有文件(*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_exepath.Text = dialog.FileName;
            }
        }

        private void SaveConfig()
        {
            var config = new Model
            {
                Buckets = txt_buckets.Text,
                CachePath = txt_cache_path.Text,
                ExePath = txt_exepath.Text,
                FarmerPublicKey = txt_farmer_public_key.Text,
                FinalPath = txt_final_path.Text,
                Fingerprint = txt_fingerprint.Text,
                MemoryCache = txt_memory_cache.Text,
                PoolPublicKey = txt_pool_public_key.Text,
                QueueLen = txt_n.Text,
                Size = txt_size.Text,
                ThreadCount = txt_thread_count.Text,
            };
            var configPath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "config.json");
            File.WriteAllText(configPath,Newtonsoft.Json.JsonConvert.SerializeObject(config));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var configPath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "config.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var config = Newtonsoft.Json.JsonConvert.DeserializeObject<Model>(json);
                txt_exepath.Text = config.ExePath;
                txt_n.Text = config.QueueLen;
                txt_size.Text = config.Size;
                txt_memory_cache.Text = config.MemoryCache;
                txt_thread_count.Text = config.ThreadCount;
                txt_buckets.Text = config.Buckets;
                txt_farmer_public_key.Text = config.FarmerPublicKey;
                txt_pool_public_key.Text = config.PoolPublicKey;
                txt_fingerprint.Text = config.Fingerprint;
                txt_cache_path.Text = config.CachePath;
                txt_final_path.Text = config.FinalPath;
            }
        }
    }

    public class Model
    {
        public string ExePath { get; set; }
        public string Size { get; set; }
        public string ThreadCount { get; set; }
        public string QueueLen { get; set; }
        public string Buckets { get; set; }
        public string MemoryCache { get; set; }
        public string FarmerPublicKey { get; set; }
        public string PoolPublicKey { get; set; }
        public string Fingerprint { get; set; }
        public string CachePath { get; set; }
        public string FinalPath { get; set; }
    }
}
