using fNbt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NBTViewer
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<FileTab> tabList = new ObservableCollection<FileTab>();

        private static readonly string[] regionFileExtensions = new string[] { ".mca", ".mcapm", ".mcr" };

        public MainWindow()
        {
            InitializeComponent();

            tabControl.ItemsSource = tabList;
        }

        private void NBTPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                openNbtFileInViewer(files[0]);
            }
        }

        private void openNbtFileInViewer(string path)
        {
            NbtCompound rootTag = null;
            try
            {
                if (Array.Exists(regionFileExtensions, e => e == Path.GetExtension(path)))
                {
                    rootTag = NbtTreeContainer.loadRegionFile(path);
                }
                else
                {
                    rootTag = NbtTreeContainer.loadNbtFile(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.ToString() + "\n" + ex.StackTrace, "Error parsing NBT", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }

            if (rootTag != null)
            {
                TreeViewItem treeRoot = new TreeViewItem();
                foreach (NbtTag tag in rootTag)
                {
                    if (tag is ICollection<NbtTag>)
                    {
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)delegate
                            {
                                treeRoot.Items.Add(NbtTreeContainer.collectionItem(tag));
                            });
                        });
                    }
                    else
                    {
                        treeRoot.Items.Add(NbtTreeContainer.valueItem(tag));
                    }
                }

                FileTab tab = new FileTab(treeRoot,
                    String.Format("{0} ({1} {2})", Path.GetFileName(path), rootTag.Count, rootTag.Count == 1 ? "tag" : "tags"));
                tabList.Add(tab);
                tabControl.SelectedItem = tab;
            }
        }

        private RelayCommand closeTabCommand;

        public RelayCommand CloseTabCommand
        {
            get
            {
                if (closeTabCommand == null)
                {
                    closeTabCommand = new RelayCommand(w => CloseCommandHandler(w), null);
                }
                return closeTabCommand;
            }
        }

        private void CloseCommandHandler(object parameter)
        {
            tabControl.ClearValue(ItemsControl.ItemsSourceProperty);
            tabList.Remove(tabList.Where(w => w.header.Equals(parameter as string)).Single());
            tabControl.ItemsSource = tabList;
        }
    }
}
