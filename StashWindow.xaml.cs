using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using BranchTool.Controls;
using BranchTool.Properties;

namespace BranchTool
{
    /// <summary>
    /// Interaction logic for StashWindow.xaml
    /// </summary>
    public partial class StashWindow : Window
    {
        #region Constructors and Destructors

        public StashWindow()
        {
            this.InitializeComponent();

            this.RepositoryControl.RepositorySelected += (sender, args) =>
                {
                    this.GetRecentStash();
                };

            this.GetRecentStash();
        }

        #endregion

        #region Methods

        private void BranchStash(object sender, RoutedEventArgs e)
        {
            var branchName = this.BranchControl.GetBranchName();

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (s, args) =>
                {
                    this.StartGit($"stash branch {branchName}");
                };
            bw.RunWorkerCompleted += (o, args) =>
            {
                this.GetRecentStash();
            };
            bw.RunWorkerAsync();
        }

        private void ClearStash(object sender, RoutedEventArgs e)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (s, args) =>
                {
                    this.StartGit("stash clear");
                };
            bw.RunWorkerCompleted += (o, args) =>
                {
                    this.GetRecentStash();
                };
            bw.RunWorkerAsync();
        }

        private void GetRecentStash()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (sender, args) =>
                {
                    var list = this.StartGit("stash list", true);
                    var first = list?.Trim().Split(new[] { "\n" }, StringSplitOptions.None).FirstOrDefault();

                    this.Dispatcher.Invoke(
                        () =>
                            {
                                bool hasStashes = !string.IsNullOrWhiteSpace(first);
                                this.RecentStashDesc.Text = hasStashes ? first : "<no stashes>";
                                this.PopButton.IsEnabled = this.BranchButton.IsEnabled = this.ClearButton.IsEnabled = hasStashes;
                            });
                };

            bw.RunWorkerAsync();
        }

        private void PopStash(object sender, RoutedEventArgs e)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (s, args) =>
            {
                this.StartGit("stash pop");
            };
            bw.RunWorkerCompleted += (o, args) =>
            {
                this.GetRecentStash();
            };
            bw.RunWorkerAsync();
        }

        private void PushStash(object sender, RoutedEventArgs e)
        {
            var stashDescText = this.StashDesc.Text;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (s, args) =>
                {
                    this.StartGit($"stash push{(!string.IsNullOrWhiteSpace(stashDescText) ? " -m " : string.Empty)}{stashDescText}");
                };
            bw.RunWorkerCompleted += (o, args) =>
            {
                this.GetRecentStash();
            };
            bw.RunWorkerAsync();
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

        private void StashWindow_OnClosing(object sender, CancelEventArgs e)
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

    }
}