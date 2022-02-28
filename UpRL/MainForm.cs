// <copyright file="MainForm.cs" company="PublicDomain.is">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace UpRL
{
    // Directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using HtmlAgilityPack;

    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:UpRL.MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the add directory button click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAddDirectoryButtonClick(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog.ShowDialog() == DialogResult.OK && this.folderBrowserDialog.SelectedPath.Length > 0)
            {
                this.directoriesListBox.Items.Add(this.folderBrowserDialog.SelectedPath);
            }

            // Update directories count
            this.directoriesCountToolStripStatusLabel.Text = this.directoriesListBox.Items.Count.ToString();
        }

        /// <summary>
        /// Handles the process button click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnProcessButtonClick(object sender, EventArgs e)
        {
            // Disable buttons while processing
            this.addDirectoryButton.Enabled = false;
            this.processButton.Enabled = false;

            // List of URL files
            List<string> urlFilesList = new List<string>();

            // Iterate folders
            foreach (string directory in this.directoriesListBox.Items)
            {
                // Add URL paths
                urlFilesList.AddRange(Directory.GetFiles(directory, "*.url", this.scanSubdirectoriesToolStripMenuItem.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            }

            // Update files with new titles
            foreach (string urlFile in urlFilesList)
            {
                try
                {
                    // Title
                    string title = string.Empty;

                    // Read all lines
                    var lines = File.ReadAllLines(urlFile);

                    // Iterate lines
                    for (int i = 0; i < lines.Length; i++)
                    {
                        // Look for URL line
                        if (lines[i].StartsWith("url=", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Extract URL
                            string url = lines[i].Split('=')[1];

                            // Get title from URL
                            var htmlWeb = new HtmlWeb();
                            var document = htmlWeb.Load(url);
                            title = document.DocumentNode.SelectSingleNode("html/head/title").InnerText;
                        }
                    }

                    // Backup
                    if (this.backupFilesToolStripMenuItem.Checked)
                    {
                        // Backup directory
                        var backupDirectory = Path.Combine(Path.GetDirectoryName(urlFile), "UpRL-backup");

                        // Create backup directory
                        Directory.CreateDirectory(backupDirectory);

                        // Copy current .url file
                        File.Move(urlFile, Path.Combine(backupDirectory, Path.GetFileName(urlFile)));
                    }

                    // Save URL file to disk
                    File.WriteAllLines(Path.Combine(Path.GetDirectoryName(urlFile), $"{string.Join("_", title.Split(Path.GetInvalidFileNameChars()))}.url"), lines);

                    // Update processed count
                    this.processedCountToolStripStatusLabel.Text = (int.Parse(this.processedCountToolStripStatusLabel.Text) + 1).ToString();
                }
                catch (Exception ex)
                {
                    // Append to error log
                    File.AppendAllText("UpRL-ErrorLog.txt", $"{Environment.NewLine }File: {urlFile}, Message: {ex.Message}");
                }
            }

            // Disable buttons after processing
            this.addDirectoryButton.Enabled = true;
            this.processButton.Enabled = true;
        }

        /// <summary>
        /// Handles the directories list box drag enter.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnDirectoriesListBoxDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        /// <summary>
        /// Handles the directories list box drag drop.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnDirectoriesListBoxDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (var possibleDirectory in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    if (Directory.Exists(possibleDirectory))
                    {
                        this.directoriesListBox.Items.Add(possibleDirectory);
                    }
                }

                // Update directories count
                this.directoriesCountToolStripStatusLabel.Text = this.directoriesListBox.Items.Count.ToString();
            }
        }

        /// <summary>
        /// Handles the new tool strip menu item1 click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnNewToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Clear list
            this.directoriesListBox.Items.Clear();

            // Reset counters
            this.directoriesCountToolStripStatusLabel.Text = "0";
            this.processedCountToolStripStatusLabel.Text = "0";
        }

        /// <summary>
        /// Handles the options tool strip menu item drop down item clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOptionsToolStripMenuItemDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Set tool strip menu item
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)e.ClickedItem;

            // Toggle checked
            toolStripMenuItem.Checked = !toolStripMenuItem.Checked;

            // Set topmost by check box
            this.TopMost = this.alwaysOnTopToolStripMenuItem.Checked;
        }

        /// <summary>
        /// Handles the Free Releases @ PublicDomain.is tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnFreeReleasesPublicDomainisToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Open our website
            Process.Start("https://publicdomain.is");
        }

        /// <summary>
        /// Handles the original thread @ DonationCoder.com tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOriginalThreadDonationCodercomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Handles the source code @ GitHub.com tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSourceCodeGithubcomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Handles the about tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Handles the exit tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }
    }
}
