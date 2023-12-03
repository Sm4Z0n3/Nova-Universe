using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfDemo
{
    /// <summary>
    /// Log.xaml 的交互逻辑
    /// </summary>
    public partial class Log : Window
    {
        public Log()
        {
            InitializeComponent();
            printf("Nova Universe 2023" ,Brushes.Red);
        }
        public void printf(string newItem, SolidColorBrush color)
        {
            string newTextWithoutNewlines = newItem.Replace("\n", "").Replace("\r", "");

            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = newTextWithoutNewlines;
            listBoxItem.Foreground = color;

            List_.Items.Add(listBoxItem);

            // 检查滑块是否位于最下方
            var scrollViewer = GetScrollViewer(List_);
            if (scrollViewer != null)
            {
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                {
                    // 如果滑块已经在最下方，则将其滚动到底部
                    scrollViewer.ScrollToEnd();
                }
                // 如果滑块不在最下方，不做滚动处理
            }
        }

        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = GetScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }



        private void List__SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = (sender as ListBox).SelectedItems;

            if (selectedItems.Count > 0)
            {
                string selectedContent = selectedItems[0].ToString();
                Clipboard.SetText(selectedContent);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Collapsed;
        }
    }
}
