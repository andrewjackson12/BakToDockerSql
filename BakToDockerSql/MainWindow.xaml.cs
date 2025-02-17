using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace BakToDockerSql
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        string composeTemplateName = "DockerComposeTemplate.txt";
        string dockerFileTemplateName = "DockerFileTemplate.txt";
        public MainWindow()
        {
            InitializeComponent();
            RefreshFiles();
        }

        private void btnAddBak_Click(object sender, RoutedEventArgs e)
        {
            // Create and configure the OpenFileDialog
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Backup Files (*.bak)|*.bak|All Files (*.*)|*.*",
                Title = "Select a .bak File"
            };

            // Show the dialog
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // Get the selected file's path
                string selectedFile = dlg.FileName;

                // Ensure the file has a .bak extension (case-insensitive)
                if (Path.GetExtension(selectedFile).ToLower() != ".bak")
                {
                    MessageBox.Show("Please select a file with a .bak extension.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Define the destination folder ("Backups") relative to the application's base directory

                // Create the "Backups" folder if it doesn't exist
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                // Define the destination file path using the same file name
                string destFile = Path.Combine(backupFolder, Path.GetFileName(selectedFile));

                try
                {
                    // Copy the file to the "Backups" folder, overwriting if it already exists
                    File.Copy(selectedFile, destFile, true);
                    MessageBox.Show("File uploaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Add file name to listbox
                    string[] selectedFileSeparated = selectedFile.Split('\\');
                    lbFiles.Items.Add(selectedFileSeparated[selectedFileSeparated.Length - 1]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error uploading file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRemoveBak_Click(object sender, RoutedEventArgs e)
        {
            /*
             * Removes the selected file from storage. 
             * Currently only works for one selected file.
             */

            // Check if a file is selected
            if (lbFiles.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an Item first!");
                return;
            }

            // Get the name of the selected file
            string selectedFile = lbFiles.SelectedItem.ToString();

            // Construct the path of the file stored locally
            string fileToDelete = backupFolder + $@"\{selectedFile}";

            try
            {
                // Delete the file
                File.Delete(fileToDelete);
                MessageBox.Show($"{selectedFile} has been deleted.");

                //Refresh the ListBox
                RefreshFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting the file {selectedFile}: {ex.Message}", "Error Deleting File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshFiles()
        {
            // Clear the LB
            lbFiles.Items.Clear();

            // Check if the directory exists
            if (!Directory.Exists(backupFolder))
            {
                return;
            }

            // Get all the files and add them to the LB
            string[] existingFiles = Directory.GetFiles(backupFolder);

            foreach(string file in existingFiles)
            {
                lbFiles.Items.Add(Path.GetFileName(file));
            }

        }

        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            /*
             * Starts the dockerized SQL server based on user inputs
            */
            // Check if a file is selected
            if (lbFiles.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an Item first!");
                return;
            }

            // Get the name of the selected file
            string selectedFile = lbFiles.SelectedItem.ToString();

            // Get the password
            string sqlPassword = txtPassword.Text.ToString();

            // Get the port
            string port = txtPort.Text.ToString();

            // Get our template files as strings
            string composeTemplate = File.ReadAllText(composeTemplateName);
            string dockerFileTemplate = File.ReadAllText(dockerFileTemplateName);


            // Replace placeholders with actual user input values
            composeTemplate = composeTemplate.Replace("{port}", port)
                .Replace("{sqlPassword}", sqlPassword);

            dockerFileTemplate = dockerFileTemplate.Replace("{port}", port)
                .Replace("{sqlPassword}", sqlPassword)
                .Replace("{backupFileName}", selectedFile);

            // Add a folder to store the generated files
            string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeneratedFiles");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }


            // Write the Docker Compose file
            string dockerComposePath = Path.Combine(outputDirectory, "docker-compose.yml");
            File.WriteAllText(dockerComposePath, composeTemplate);

            // Write the Dockerfile
            string dockerFilePath = Path.Combine(outputDirectory, "Dockerfile");
            File.WriteAllText(dockerFilePath, dockerFileTemplate);

        }
    }
}