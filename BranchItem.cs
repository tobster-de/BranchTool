using System.Collections.Generic;
using System.Linq;
using PropertyChanged;

namespace BranchTool
{
    public class BranchItem : NotifyPropertyChanged
    {
        private bool readMode;

        private string text;

        #region Constructors and Destructors

        public BranchItem(string text)
        {
            this.Text = text;
            this.ReadMode = true;
            this.ComboBoxItems = new List<string>();
            this.ShowSlash = false;
        }

        public BranchItem(IEnumerable<string> items, int? selectedIndex)
        {
            this.Text = selectedIndex.HasValue ? items.ElementAt(selectedIndex.Value) : items.FirstOrDefault();
            this.ReadMode = true;
            this.ComboBoxItems = new List<string>(items);
            this.ShowSlash = true;
        }

        #endregion

        #region Public Properties

        public List<string> ComboBoxItems { get; set; }

        public bool ReadMode
        {
            get
            {
                return this.readMode;
            }
            set
            {
                this.SetValue(ref this.readMode, value);
                this.OnPropertyChanged(() => this.ShowTextBox);
                this.OnPropertyChanged(() => this.ShowComboBox);
            }
        }

        public bool ShowComboBox => !this.ReadMode && this.ComboBoxItems.Any();

        public bool ShowSlash { get; set; }

        public bool ShowTextBox => !this.ReadMode && !this.ComboBoxItems.Any();

        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.SetValue(ref this.text, value);
            }
        }

        #endregion
    }
}