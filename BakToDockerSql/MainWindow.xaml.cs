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
        public MainWindow()
        {
            InitializeComponent();
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
                string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

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
            string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            backupFolder += $@"\{selectedFile}";

            try
            {
                // Delete the file
                File.Delete(backupFolder);
                MessageBox.Show($"{selectedFile} has been deleted.");
                // RefreshFiles call here (should be used on open of app too)
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting the file {selectedFile}: {ex.Message}", "Error Deleting File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}