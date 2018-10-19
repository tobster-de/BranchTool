using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using BranchTool.Properties;

namespace BranchTool.Controls
{
    /// <summary>
    /// Interaction logic for BranchControl.xaml
    /// </summary>
    public partial class BranchControl : UserControl
    {
        #region Static Fields

        public static readonly DependencyProperty BranchItemsProperty =
            DependencyProperty.Register(
                "BranchItems",
                typeof(ObservableCollection<BranchItem>),
                typeof(BranchControl),
                new FrameworkPropertyMetadata(OnAvailableItemsChanged)
                    {
                        BindsTwoWayByDefault = true
                    });

        #endregion

        #region Constructors and Destructors

        public BranchControl()
        {
            this.DataContext = this;
            this.BranchItems = new ObservableCollection<BranchItem>();

            this.InitializeComponent();

            this.Init();
        }


        private void Init()
        {
            Settings settings = Properties.Settings.Default;

            var releases = new List<string>();
            for (int i = 2017; i <= DateTime.Today.Year; i++)
            {
                releases.Add($"{i}A");
                releases.Add($"{i}B");
            }
            if (DateTime.Today.Month > 6)
            {
                releases.Add($"{DateTime.Today.Year + 1}A");
            }
            int selectedIndex = releases.Contains(settings.Release) ? releases.IndexOf(settings.Release) : 0;
            this.BranchItems.Add(new BranchItem(releases, selectedIndex));

            var types = new[] { "bugfix", "feature", "misc" }.ToList();
            selectedIndex = types.Contains(settings.Type) ? types.IndexOf(settings.Type) : 0;
            this.BranchItems.Add(new BranchItem(types, selectedIndex));

            this.BranchItems.Add(new BranchItem(!string.IsNullOrWhiteSpace(settings.User) ? settings.User : Environment.UserName) { ShowSlash = true });

            string proposedName = this.GetProposedBranchName(settings.Branch);
            this.BranchItems.Add(new BranchItem(proposedName));
        }

        public static Dictionary<K, V> HashtableToDictionary<K, V>(Hashtable table)
        {
            return table
              .Cast<DictionaryEntry>()
              .ToDictionary(kvp => (K)kvp.Key, kvp => (V)kvp.Value);
        }

        private string GetProposedBranchName(string last)
        {
            //const string MKSSI_ISSUE = "MKSSI_ISSUE";
            const string MKSSI_ISSUE0 = "MKSSI_ISSUE0";

            var env = System.Environment.GetEnvironmentVariables();
            var variables =
                HashtableToDictionary<string, string>(env as Hashtable).Where(kv => kv.Key.StartsWith("MKSSI")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (variables.Any() && variables.ContainsKey(MKSSI_ISSUE0))
            {
                return variables[MKSSI_ISSUE0];
            }

            return !string.IsNullOrWhiteSpace(last) ? last : "branch";
        }

        #endregion

        #region Public Properties

        public ObservableCollection<BranchItem> BranchItems
        {
            get
            {
                return (ObservableCollection<BranchItem>)this.GetValue(BranchItemsProperty);
            }
            set
            {
                this.SetValue(BranchItemsProperty, value);
            }
        }

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

        public string GetBranchName()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var branchItem in this.BranchItems)
            {
                sb.Append(branchItem.Text);
                if (branchItem.ShowSlash)
                {
                    sb.Append("/");
                }
            }
            return sb.ToString();
        }

        #endregion

        #region Methods

        private void ComboBox_OnDropDownClosed(object sender, EventArgs e)
        {
            foreach (var branchItem in this.BranchItems) branchItem.ReadMode = true;
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var hl = (Hyperlink)sender;
            var item = hl.DataContext as BranchItem;
            item.ReadMode = false;

            var sp = ((FrameworkElement)hl.Parent).Parent as StackPanel;
            if (item.ShowTextBox)
            {
                var tb = sp.Children.OfType<TextBox>().FirstOrDefault();
                tb.Focus();
                tb.SelectionStart = 0;
                tb.SelectionLength = tb.Text.Length;
            }

            if (item.ShowComboBox)
            {
                var cb = sp.Children.OfType<ComboBox>().FirstOrDefault();
                cb.IsDropDownOpen = true;
            }
        }

        private void Selector_OnSelected(object sender, RoutedEventArgs e)
        {
            var cb = (ComboBox)sender;
            var item = cb.DataContext as BranchItem;
            item.Text = cb.SelectedValue.ToString();
            item.ReadMode = true;
        }

        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            foreach (var branchItem in this.BranchItems) branchItem.ReadMode = true;
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tb = (TextBox)sender;
            var item = tb.DataContext as BranchItem;

            if (e.Key == Key.Enter)
            {
                item.Text = tb.Text;
                item.ReadMode = true;
            }

            if (e.Key == Key.Escape) item.ReadMode = true;
        }

        #endregion
    }
}