// --------------------------------------------------------------------------------------------------------------------
// <copyright company="dSPACE GmbH" file="BasePropertyChanged.cs">
//   Copyright 2014, dSPACE GmbH. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace PropertyChanged
{
    /// <summary>
    /// Presents the basis for notify property changed and error provider support.
    /// Use <see cref="NotifyPropertyChanged"/> class if error info support is not required.
    /// </summary>
    public abstract class BasePropertyChanged : NotifyPropertyChanged, IDataErrorInfo
    {
        // ===============================================================
        #region Fields

        /// <summary>
        /// The error list.
        /// </summary>
        protected internal Dictionary<string, KeyValuePair<string, bool>> errorList = new Dictionary<string, KeyValuePair<string, bool>>();

        /// <summary>
        /// The warning list.
        /// </summary>
        protected internal Dictionary<string, KeyValuePair<string, bool>> warningList = new Dictionary<string, KeyValuePair<string, bool>>();

        /// <summary>
        /// The info list.
        /// </summary>
        protected internal Dictionary<string, string> infoList = new Dictionary<string, string>();

        #endregion

        // ===============================================================

        // ===============================================================
        #region Properties

        /// <summary>
        /// Gets a value that represents the Error count of the internal ViewModel (without Errors of ExtendedControl).
        /// </summary>
        public int ErrorCount
        {
            get
            {
                return this.errorList.Count;
            }
        }

        /// <summary>
        /// Gets a value that represents the Warning count of the internal ViewModel (without Warnings of ExtendedControl).
        /// </summary>
        public int WarningCount
        {
            get
            {
                return this.warningList.Count;
            }
        }

        /// <summary>
        /// Gets a value that represents the Info count of the internal ViewModel (without Infos of ExtendedControl).
        /// </summary>
        public int InfoCount
        {
            get
            {
                return this.infoList.Count;
            }
        }

        #endregion Properties

        // ===============================================================

        // ===============================================================
        #region Property changed

        /// <summary>
        /// This Method fires the PropertyChanges of an Entity to the ViewModel Properties.
        /// </summary>
        /// <param name="sender">
        /// Sender Entity.
        /// </param>
        /// <param name="e">
        /// Property Change Event.
        /// </param>
        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.DeleteError(e.PropertyName);
            this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        // ===============================================================

        // ===============================================================

        // ===============================================================
        #region DataErrorInfo

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error => string.Empty;

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">
        /// The name of the property whose error message to get.
        /// </param>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        public string this[string columnName]
        {
            get
            {
                KeyValuePair<string, bool> errorKeyValuePair;
                if (this.errorList.TryGetValue(columnName, out errorKeyValuePair))
                {
                    return errorKeyValuePair.Key;
                }

                KeyValuePair<string, bool> warningKeyValuePair;
                if (this.warningList.TryGetValue(columnName, out warningKeyValuePair))
                {
                    return warningKeyValuePair.Key;
                }

                string value;
                if (this.infoList.TryGetValue(columnName, out value))
                {
                    return value;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// This methods deletes an error from the ViewModel, that was caused by the control validation mechanism.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        public virtual void DeleteControlValidationError(string propertyName)
        {
            if (propertyName != null)
            {
                KeyValuePair<string, bool> errorToRemove;
                if (this.errorList.TryGetValue(propertyName, out errorToRemove))
                {
                    // Delete only if error was set by Control
                    if (errorToRemove.Value)
                    {
                        this.errorList.Remove(propertyName);
                        this.OnPropertyChanged(propertyName);
                        this.OnPropertyChanged("ErrorCount");
                    }
                }

                KeyValuePair<string, bool> warningToRemove;
                if (this.warningList.TryGetValue(propertyName, out warningToRemove))
                {
                    // Delete only if warning was set by Control
                    if (warningToRemove.Value)
                    {
                        this.warningList.Remove(propertyName);
                        this.OnPropertyChanged(propertyName);
                        this.OnPropertyChanged("WarningCount");
                    }
                }
            }
        }

        /// <summary>
        /// This Method removes an error message of a property.
        /// </summary>
        /// <param name="propertyName">
        /// Name of the Property from that the binded error message should be deleted.
        /// </param>
        public virtual void DeleteError(string propertyName = null)
        {
            if (propertyName != null)
            {
                // Remove non Control Errors
                if (this.errorList.ContainsKey(propertyName) && this.errorList[propertyName].Value == false)
                {
                    this.errorList.Remove(propertyName);
                    this.OnPropertyChanged("ErrorCount");
                }

                // Remove non Control Warning
                if (this.warningList.ContainsKey(propertyName) && this.warningList[propertyName].Value == false)
                {
                    this.warningList.Remove(propertyName);
                    this.OnPropertyChanged("WarningCount");
                }

                // Remove all Infos
                if (this.infoList.ContainsKey(propertyName))
                {
                    this.infoList.Remove(propertyName);
                    this.OnPropertyChanged("InfoCount");
                }
            }
            else
            {
                // Remove all non Control Errors
                List<string> errorsToDelete = this.errorList.Where(o => !o.Value.Value).Select(o => o.Key).ToList();
                if (errorsToDelete.Any())
                {
                    errorsToDelete.ForEach(o => this.errorList.Remove(o));
                    this.OnPropertyChanged("ErrorCount");
                }

                // Remove all non Control Warnings
                List<string> warningsToDelete = this.warningList.Where(o => !o.Value.Value).Select(o => o.Key).ToList();
                if (warningsToDelete.Any())
                {
                    warningsToDelete.ForEach(o => this.warningList.Remove(o));
                    this.OnPropertyChanged("WarningCount");
                }

                // Remove all Infos
                if (this.infoList.Any())
                {
                    this.infoList.Clear();
                    this.OnPropertyChanged("InfoCount");
                }
            }
        }

        #endregion

        // ===============================================================
    }
}