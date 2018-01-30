using Microsoft.Win32;
using SolutionWizard.Utilities;
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
using System.ComponentModel;
using SolutionParser;
using System.IO;



namespace SolutionWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string targetFolderConfigKey = "target_dir";
        private const string suiteRepoConfigKey = "suite_path";
        private const string productRepoConfigKey = "product_path";

        public MainWindow()
        {
            InitializeComponent();

            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            txtBoxTargetFolder.Text = ConfigurationHelper.GetSetting(targetFolderConfigKey) ?? String.Empty;
            txtBoxSkySuiteRepo.Text = ConfigurationHelper.GetSetting(suiteRepoConfigKey) ?? String.Empty;
            txtBoxProductRepo.Text = ConfigurationHelper.GetSetting(productRepoConfigKey) ?? String.Empty;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();            

            if(result == System.Windows.Forms.DialogResult.OK)
            {
                txtBoxTargetFolder.Text = folderDialog.SelectedPath;                
            }
        }

        private bool GetSetupButtonState()
        {
            var isEnabled = !String.IsNullOrEmpty(txtBoxTargetFolder.Text) && !String.IsNullOrEmpty(txtBoxSkySuiteRepo.Text) 
                && !String.IsNullOrEmpty(txtBoxProductRepo.Text);
       
            return isEnabled;                        
        }

        private void btnSetup_Click(object sender, RoutedEventArgs e)
        {
            if(!GetSetupButtonState())
            {
                MessageBox.Show("You need to specify repository paths and target directory.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (!Directory.Exists(txtBoxTargetFolder.Text))
            {
                Directory.CreateDirectory(txtBoxTargetFolder.Text);
            }
                        
            // Checking out Sources
            var checkoutProcessInfo = new ProcessStartInfo();
            checkoutProcessInfo.UseShellExecute = true;                                                                           
            checkoutProcessInfo.WorkingDirectory = txtBoxTargetFolder.Text;
            checkoutProcessInfo.FileName = "cmd.exe";            
            checkoutProcessInfo.Arguments = "/C svn co " + txtBoxSkySuiteRepo.Text;
            checkoutProcessInfo.CreateNoWindow = true;
            
            var suiteCheckoutProcess = new Process();
            suiteCheckoutProcess.EnableRaisingEvents = true;
            suiteCheckoutProcess.StartInfo = checkoutProcessInfo;
            suiteCheckoutProcess.Exited += SuiteCheckoutProcess_Exited;
            suiteCheckoutProcess.Start();

            // Checking out Product Sources
            var productCheckoutProcessInfo = new ProcessStartInfo();
            productCheckoutProcessInfo.UseShellExecute = true;
            productCheckoutProcessInfo.WorkingDirectory = txtBoxTargetFolder.Text;
            productCheckoutProcessInfo.FileName = "cmd.exe";
            productCheckoutProcessInfo.Arguments = "/C svn co " + txtBoxProductRepo.Text;
            productCheckoutProcessInfo.CreateNoWindow = true;

            var productCheckoutProcess = new Process();
            productCheckoutProcess.EnableRaisingEvents = true;
            productCheckoutProcess.StartInfo = productCheckoutProcessInfo;
            productCheckoutProcess.Exited += ProductCheckoutProcess_Exited; ;
            productCheckoutProcess.Start();

            productCheckoutProcess.WaitForExit();
            suiteCheckoutProcess.WaitForExit();

            var newSlnFile = txtBoxTargetFolder.Text + "\\Suite.sln";

            var suiteProjectFiles = FileSystemHelper.FindFiles(txtBoxTargetFolder.Text + @"\Suite", "*.csproj");
            var worksProjectFiles = FileSystemHelper.FindFiles(txtBoxTargetFolder.Text + @"\product", "*.csproj");

            foreach(var projPath in suiteProjectFiles)
            {
                SolutionParser.SolutionFIleParser.AddProject(newSlnFile, projPath);
               
            }

            SolutionParser.SolutionFIleParser.ChangeRoot(true);

            foreach (var projPath in worksProjectFiles)
            {
                SolutionParser.SolutionFIleParser.AddProject(newSlnFile, projPath);
                
            }
            btnSetup.IsEnabled = false;
            btnBrowse.IsEnabled = false;
            MessageBox.Show("Completed");
            progressBar.Value = 0;
            
        }

        private void ProductCheckoutProcess_Exited(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => { progressBar.Value += 50; });
        }

        private void SuiteCheckoutProcess_Exited(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => { progressBar.Value += 50; });            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            ConfigurationHelper.SetSetting(targetFolderConfigKey, txtBoxTargetFolder.Text);
            ConfigurationHelper.SetSetting(suiteRepoConfigKey, txtBoxSkySuiteRepo.Text);
            ConfigurationHelper.SetSetting(productRepoConfigKey, txtBoxProductRepo.Text);

            ConfigurationHelper.SaveConfigs();
        }
    }
}
