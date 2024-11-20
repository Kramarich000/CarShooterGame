using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1.Models
{
    public abstract class GameObject
    {
        public FrameworkElement Element { get; }
        public List<FrameworkElement> AllObjects { get; }

        protected GameObject(FrameworkElement element, List<FrameworkElement> allObjects)
        {
            Element = element;
            AllObjects = allObjects;
        }
    }

}
