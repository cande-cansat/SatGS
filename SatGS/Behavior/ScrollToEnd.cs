using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SatGS.Behavior
{
    internal class ScrollToEnd : Behavior<ListView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView listView)) return;

            if (listView.SelectedItem == null) return;

            listView.Dispatcher.Invoke(() =>
            {
                listView.UpdateLayout();
                listView.ScrollIntoView(listView.SelectedItem);
            });
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            base.OnDetaching();
        }
    }
}
