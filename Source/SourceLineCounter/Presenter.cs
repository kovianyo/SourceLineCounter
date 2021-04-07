using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SourceLineCounter
{
    internal class Presenter
    {
        private Label _label;

        private ContextMenu _contextMenu;

        public event Action OnRecalculate;

        public void CreateControls()
        {
            CreateControlsInternal();
            SetLineCount(0);
        }

        public void SetLineCount(int lineCount)
        {
            if (_label != null)
            {
                _label.Content = $"{lineCount:N0} sloc";
            }
        }

        private void CreateControlsInternal()
        {
            var resizeGripControl = FindChild<Control>(Application.Current.MainWindow, "ResizeGripControl");
            
            var dockPanel = resizeGripControl.Parent as DockPanel;
            
            if (dockPanel != null)
            {
                _label = CreateLabel(dockPanel);

                CreateContextMenu(_label);
            }
        }

        private Label CreateLabel(DockPanel dockPanel)
        {
            var label = new Label { Foreground = Brushes.White };
            DockPanel.SetDock(label, Dock.Right);
            dockPanel.Children.Insert(1, label);
            label.MouseRightButtonUp += ShowContextMenu;

            return label;
        }

        private void CreateContextMenu(UIElement placementTargetElement)
        {
            var contextMenu = new ContextMenu { PlacementTarget = placementTargetElement };
            var menuItem = new MenuItem { Header = "Recalculate" };
            menuItem.Click += RecalculateOnClick;
            contextMenu.Items.Add(menuItem);
            _contextMenu = contextMenu;
        }

        private void ShowContextMenu(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _contextMenu.IsOpen = true;
        }

        private void RecalculateOnClick(object sender, RoutedEventArgs e)
        {
            OnRecalculate?.Invoke();
        }

        /// <summary>
        /// Looks for a child control within a parent by name
        /// </summary>
        private static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null)
            {
                return null;
            }

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null)
                    {
                        break;
                    }
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                    else
                    {
                        // recursively drill down the tree
                        foundChild = FindChild<T>(child, childName);

                        // If the child is found, break so we do not overwrite the found child.
                        if (foundChild != null)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }
}
