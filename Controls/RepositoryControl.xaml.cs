using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BranchTool.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using PropertyChanged;

namespace BranchTool.Controls
{
    /// <summary>
    /// Interaction logic for RepositoryControl.xaml
    /// </summary>
    public partial class RepositoryControl : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<PropertyChangedEventArgs> RepositorySelected;

        #region Constants

        internal const string SelectRepository = "<select repository>";

        #endregion

        #region Static Fields

        public static readonly DependencyProperty RepositoriesProperty =
            DependencyProperty.Register(
                "Repositories",
                typeof(ObservableCollection<string>),
                typeof(RepositoryControl),
                new FrameworkPropertyMetadata(OnAvailableItemsChanged)
                    {
                        BindsTwoWayByDefault = true
                    });

        public static readonly DependencyProperty SelectedRepositoryProperty =
            DependencyProperty.Register(
                "SelectedRepository",
                typeof(string),
                typeof(RepositoryControl),
                new PropertyMetadata(SelectRepository, PropertyChangedCallback));

        #endregion

        #region Fields

        private bool readMode;

        private string selectedRepository;

        #endregion

        #region Constructors and Destructors

        public RepositoryControl()
        {
            this.DataContext = this;
            this.ReadMode = true;
            this.Repositories = new ObservableCollection<string>();

            this.InitializeComponent();

            this.Initialize();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<string> ComboBoxItems { get; } = new ObservableCollection<string>();

        public bool ReadMode
        {
            get
            {
                return this.readMode;
            }
            set
            {
                this.readMode = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ShowComboBox));
            }
        }

        public ObservableCollection<string> Repositories
        {
            get
            {
                return (ObservableCollection<string>)this.GetValue(RepositoriesProperty);
            }
            set
            {
                this.SetValue(RepositoriesProperty, value);
            }
        }

        public string SelectedRepository
        {
            get
            {
                return this.selectedRepository;
            }
            set
            {
                this.selectedRepository = value;
                if (this.ComboBox != null) this.ComboBox.SelectedItem = value;
                this.OnPropertyChanged();
                this.ReadMode = true;

                this.RepositorySelected?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SelectedRepository)));
            }
        }

        public bool ShowComboBox => !this.ReadMode;

        #endregion

        #region Public Methods and Operators

        public static void OnAvailableItemsChanged(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            // Breakpoint here to see if the new value is being set
            var newValue = e.NewValue;
            //Debugger.Break();
        }

        #endregion

        #region Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue.Equals(SelectRepository))
            {
                var rc = dependencyObject as RepositoryControl;
                rc.OpenRepository();
            }
        }

        private void AddRepository(string directory)
        {
            if (!this.ComboBoxItems.Contains(directory) && Directory.Exists(directory))
            {
                this.ComboBoxItems.Add(directory);
                this.Repositories.Add(directory);
            }
        }

        private void CheckDirIsRepository(string currentDir)
        {
            var git = Path.Combine(currentDir, ".git");
            if (Directory.Exists(git))
            {
                this.AddRepository(currentDir);
                return;
            }

            if (currentDir.LastIndexOf(Path.DirectorySeparatorChar) > 3) this.CheckDirIsRepository(currentDir.Substring(0, currentDir.LastIndexOf(Path.DirectorySeparatorChar)));
        }

        private void ComboBox_OnDropDownClosed(object sender, EventArgs e)
        {
            this.ReadMode = true;
        }

        private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ReadMode) return;

            var cb = (ComboBox)sender;
            var item = cb.SelectedItem.ToString();
            if (string.Equals(item, SelectRepository)) this.OpenRepository();
            else this.SelectedRepository = item;
            this.ReadMode = true;
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.Repositories.Any())
            {
                this.ReadMode = false;
                this.ComboBox.IsDropDownOpen = true;
            }
            else
            {
                this.OpenRepository();
            }
        }

        private void Initialize()
        {
            var settings = Settings.Default;

            var currentDir = Directory.GetCurrentDirectory();

            settings.Repositories?.ForEach(this.AddRepository);

            this.CheckDirIsRepository(currentDir);

            var match = this.Repositories.FirstOrDefault(repo => currentDir.StartsWith(repo));
            if (match != null)
            {
                this.SelectedRepository = match;
            }
            else if (!string.IsNullOrWhiteSpace(settings.SelectedRepository)
                     && !string.Equals(SelectRepository, settings.SelectedRepository))
            {
                this.SelectedRepository = settings.SelectedRepository;
                this.AddRepository(this.SelectedRepository);
            }
            else
            {
                this.SelectedRepository = SelectRepository;
            }

            this.ComboBoxItems.Add(SelectRepository);
        }

        private void InsertRepository(string directory)
        {
            if (!this.Repositories.Contains(directory))
            {
                this.ComboBoxItems.Insert(this.ComboBoxItems.Count - 1, directory);
                this.Repositories.Insert(this.Repositories.Count, directory);

                this.SelectedRepository = directory;
            }
        }

        private void OpenRepository()
        {
            var last = this.SelectedRepository;

            var dialog = new CommonOpenFileDialog();
            //dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (Path.GetExtension(dialog.FileName) == ".git")
                {
                    var path = Path.GetDirectoryName(dialog.FileName);
                    this.InsertRepository(path);
                }

                if (Directory.Exists(Path.Combine(dialog.FileName, ".git"))) this.InsertRepository(dialog.FileName);
            }
            else
            {
                this.SelectedRepository = last;
            }
        }

        #endregion
    }
}