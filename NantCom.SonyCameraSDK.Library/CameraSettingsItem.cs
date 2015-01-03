using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NantCom.SonyCameraSDK
{
    public class CameraSetting : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public string Group { get; set; }

        /// <summary>
        /// Name of the settings
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the settings
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Available Options
        /// </summary>
        public string[] AvailableOptions { get; set; }

        /// <summary>
        /// Get/set the owner of this settings
        /// </summary>
        public Camera Owner { get; internal set; }

        [JsonIgnore]
        public Action<string> OnNewValue = delegate { };

        private string _Value;

        /// <summary>
        /// Gets/set the value
        /// </summary>
        public string Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (value != _Value)
                {
                    _Value = value;

                    if (this.Owner != null)
                    {
                        if (this.Owner.DisableUpdate == false)
                        {
                            this.OnNewValue(value);
                        }
                    }

                    this.OnPropertyChanged("Value");
                }
            }
        }

        public CameraSetting()
        {

        }

        public CameraSetting( Camera owner )
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Returns current value of this setting
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Value;
        }

        private void OnPropertyChanged(string name)
        {
            this.Owner.Context.Post((o) =>
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));

            }, null);
        }

    }
}
