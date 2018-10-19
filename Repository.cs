using System.Collections.Generic;
using System.Linq;
using PropertyChanged;

namespace BranchTool
{
    public class Repository : NotifyPropertyChanged
    {
        private bool readMode;

        private string text;

        #region Constructors and Destructors

        public Repository(IEnumerable<string> items, int? selectedIndex)
        {
            this.Text = selectedIndex.HasValue ? items.ElementAt(selectedIndex.Value) : items.FirstOrDefault();
            this.ReadMode = true;
        }

        #endregion

        #region Public Properties
        
        public bool ReadMode
        {
            get
            {
                return this.readMode;
            }
            set
            {
                this.SetValue(ref this.readMode, value);
            }
        }
        
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