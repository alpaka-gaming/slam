using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    internal class GameSetup : ICloneable, INotifyPropertyChanged
    {
        public GameSetup()
        {

        }

        public GameSetup(GameSetup original)
        {
            var props = this.GetType().GetProperties().Where(m => m.CanRead && m.CanWrite);
            foreach (var prop in props)
                prop.SetValue(this, prop.GetValue(original));
        }

        public object Clone()
        {
            return new GameSetup(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public Engines Engine { get; set; }
        public string PathFileName { get; set; }
        public string AppPathFileName { get; set; }
        public string AppOptions { get; set; }
        public string CompilerPathFileName { get; set; }
        public string ViewerPathFileName { get; set; }
        public string MappingToolPathFileName { get; set; }
        public string PackerPathFileName { get; set; }

    }
}
