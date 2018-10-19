using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using BranchTool.Controls;
using BranchTool.Properties;

namespace BranchTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BranchWindow : Window
    {
        #region Fields

        public ObservableCollection<BranchItem> BranchItems = new ObservableCollection<BranchItem>();

        #endregion

        #region Constructors and Destructors

        public BranchWindow()
        {
            this.DataContext = this;

            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void CreateBranch(object sender, RoutedEventArgs e)
        {
            string branch = this.BranchControl.GetBranchName();

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (s, args) =>
            {
                this.StartGit($"checkout -b {branch}");
            };
            bw.RunWorkerAsync();
        }

        private void RenameBranch(object sender, RoutedEventArgs e)
        {
            string branch = this.BranchControl.GetBranchName();

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (s, args) =>
            {
                this.StartGit($"branch -m {branch}");
            };
            bw.RunWorkerAsync();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            var settings = Settings.Default;

            settings.Repositories = new List<string>(this.RepositoryControl.Repositories.Where(r => !r.Equals(RepositoryControl.SelectRepository)).ToList());
            settings.SelectedRepository = this.RepositoryControl.SelectedRepository;

            var branchItems = this.BranchControl.BranchItems;
            settings.Release = branchItems[0].Text;
            settings.Type = branchItems[1].Text;
            settings.User = branchItems[2].Text;
            settings.Branch = branchItems[3].Text;

            settings.Save();
        }

        private void StashWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SizeToContent = SizeToContent.Height;
        }

        private void StashWindow_OnContentRendered(object sender, EventArgs e)
        {
            this.MinHeight = this.ActualHeight;
            this.MaxHeight = this.ActualHeight;
        }

        private string StartGit(string arguments, bool quiet = false)
        {
            try
            {
                if (this.RepositoryControl.SelectedRepository == RepositoryControl.SelectRepository) return null;

                this.Dispatcher.Invoke(
                    () =>
                    {
                        if (!quiet)
                        {
                            this.Output.Text = string.Empty;
                            this.MaxHeight = Int32.MaxValue;
                            this.Output.Visibility = Visibility.Collapsed;
                            this.UpdateLayout();
                        }
                        this.IsEnabled = false;
                    });

                Directory.SetCurrentDirectory(this.RepositoryControl.SelectedRepository);

                string output;
                if ((!Git.StartGit(arguments, out output) || !string.IsNullOrWhiteSpace(output)) && !quiet)
                {
                    this.Dispatcher.Invoke(
                        () =>
                        {
                            this.Output.Text = output;
                            this.MaxHeight = Int32.MaxValue;
                            this.Output.Visibility = Visibility.Visible;
                            this.UpdateLayout();
                        });
                }

                return output;
            }
            finally
            {
                this.Dispatcher.Invoke(() => this.IsEnabled = true);
            }
        }

        #endregion
        
    }
}