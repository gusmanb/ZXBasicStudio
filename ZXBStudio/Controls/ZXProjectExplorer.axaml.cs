using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.Controls
{
    public partial class ZXProjectExplorer : UserControl
    {
        #region Event registration
        public static readonly RoutedEvent<RoutedEventArgs> OpenFileRequestedEvent = RoutedEvent.Register<ZXProjectExplorer, RoutedEventArgs>(nameof(OpenFileRequested), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> NewFileRequestedEvent = RoutedEvent.Register<ZXProjectExplorer, RoutedEventArgs>(nameof(NewFileRequested), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> NewFolderRequestedEvent = RoutedEvent.Register<ZXProjectExplorer, RoutedEventArgs>(nameof(NewFolderRequested), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> CopyPathRequestedEvent = RoutedEvent.Register<ZXProjectExplorer, RoutedEventArgs>(nameof(CopyPathRequested), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> RenameRequestedEvent = RoutedEvent.Register<ZXProjectExplorer, RoutedEventArgs>(nameof(RenameRequested), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> DeleteRequestedEvent = RoutedEvent.Register<ZXProjectExplorer, RoutedEventArgs>(nameof(DeleteRequested), RoutingStrategies.Bubble);
        #endregion

        #region Event declaration
        public event EventHandler<RoutedEventArgs> OpenFileRequested
        { 
            add => AddHandler(OpenFileRequestedEvent, value);
            remove => RemoveHandler(OpenFileRequestedEvent, value);
        }
        public event EventHandler<RoutedEventArgs> NewFileRequested
        {
            add => AddHandler(NewFileRequestedEvent, value);
            remove => RemoveHandler(NewFileRequestedEvent, value);
        }
        public event EventHandler<RoutedEventArgs> NewFolderRequested
        {
            add => AddHandler(NewFolderRequestedEvent, value);
            remove => RemoveHandler(NewFolderRequestedEvent, value);
        }
        public event EventHandler<RoutedEventArgs> CopyPathRequested
        {
            add => AddHandler(CopyPathRequestedEvent, value);
            remove => RemoveHandler(CopyPathRequestedEvent, value);
        }
        public event EventHandler<RoutedEventArgs> RenameRequested
        {
            add => AddHandler(RenameRequestedEvent, value);
            remove => RemoveHandler(RenameRequestedEvent, value);
        }
        public event EventHandler<RoutedEventArgs> DeleteRequested
        {
            add => AddHandler(DeleteRequestedEvent, value);
            remove => RemoveHandler(DeleteRequestedEvent, value);
        }
        #endregion

        public event EventHandler<EventArgs> SelectedPathChanged;

        FileSystemWatcher? fWatcher;

        ObservableCollection<ExplorerNode> _nodes;

        string? rootPath;
        public string? RootPath { get { return rootPath; } }

        string? _selectedPath;
        public string? SelectedPath 
        { 
            get { return _selectedPath; } 
            private set 
            { 
                _selectedPath = value; 
                if (SelectedPathChanged != null)
                    SelectedPathChanged(this, EventArgs.Empty);
            } 
        }

        public ZXProjectExplorer()
        {
            InitializeComponent();
            tvExplorer.DoubleTapped += TvExplorer_DoubleTapped;
            tvExplorer.SelectionChanged += TvExplorer_SelectionChanged;
        }
        private void ContextMenuOpenFileClick(object? sender, RoutedEventArgs e)
        { RaiseEvent(new RoutedEventArgs(OpenFileRequestedEvent)); }
        private void ContextMenuNewFileClick(object? sender, RoutedEventArgs e)
        { RaiseEvent(new RoutedEventArgs(NewFileRequestedEvent)); }
        private void ContextMenuFolderFileClick(object? sender, RoutedEventArgs e)
        { RaiseEvent(new RoutedEventArgs(NewFolderRequestedEvent)); }
        private void ContextMenuCopyClick(object? sender, RoutedEventArgs e)
        {
            var item = tvExplorer.SelectedItem as ExplorerNode;

            if (item == null)
                return;

            Application.Current?.Clipboard?.SetTextAsync(item.Path);
        }
        private void ContextMenuRenameClick(object? sender, RoutedEventArgs e)
        { RaiseEvent(new RoutedEventArgs(RenameRequestedEvent)); }
        private void ContextMenuDeleteClick(object? sender, RoutedEventArgs e)
        { RaiseEvent(new RoutedEventArgs(DeleteRequestedEvent)); }

        private void ContextMenuShowBrowserClick(object? sender, RoutedEventArgs e)
        {
            var item = tvExplorer.SelectedItem as ExplorerNode;

            if (item == null)
                return;

            var path = item.Path;

            if(path == null) 
                return;

            if(File.Exists(path))
                path = Path.GetDirectoryName(path);

            if (path == null)
                return;

            if (!path.EndsWith("\\")) 
                path += "\\";  

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void TvExplorer_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selItem = tvExplorer.SelectedItems.Cast<ExplorerNode>().FirstOrDefault();

            if(selItem == null)
                SelectedPath = null;
            else
                SelectedPath = selItem.Path;
        }

        public void OpenProjectFolder(string? RootFolder)
        {
            rootPath = string.Empty;
            SelectedPath = null;

            if (RootFolder == null)
            {
                if (fWatcher != null)
                    fWatcher.Dispose();
                fWatcher = null;
                tbRoot.Text = "No project open";
                tvExplorer.Items = null;
                return;
            }

            rootPath = System.IO.Path.GetFullPath(RootFolder);
            _nodes = ScanFolder(RootFolder);
            tvExplorer.Items = _nodes;
            tbRoot.Text = "Project " + System.IO.Path.GetFileName(RootFolder);
            

            if (fWatcher != null)
            {
                fWatcher.Created -= FWatcher_Created;
                fWatcher.Deleted -= FWatcher_Deleted;
                fWatcher.EnableRaisingEvents = false;
                fWatcher.Dispose();
            }

            fWatcher = new FileSystemWatcher(RootFolder) { IncludeSubdirectories = true };
            fWatcher.Created += FWatcher_Created;
            fWatcher.Deleted += FWatcher_Deleted;
            fWatcher.Renamed += FWatcher_Renamed;
            fWatcher.EnableRaisingEvents = true;
        }

        public void SelectPath(string? Path)
        {
            var node = FindNode(Path, _nodes);

            if (node != null)
                tvExplorer.SelectedItem = node;
        }

        private void FWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                
                var node = FindNode(e.OldFullPath, _nodes);

                if (node != null)
                {
                    node.UpdateNode(e.FullPath);
                    if(tvExplorer.SelectedItem == node)
                        SelectedPath = e.FullPath;
                }
            });
        }

        private void FWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() => { 
                var node = FindNode(e.FullPath, _nodes);
                if (node != null)
                {
                    string pathWithoutNode = Path.GetDirectoryName(e.FullPath);
                    string left = pathWithoutNode.Replace(rootPath, "");

                    if (string.IsNullOrWhiteSpace(left))
                        _nodes.Remove(node);
                    else
                    {
                        var parent = FindNode(pathWithoutNode, _nodes);
                        if (parent != null)
                            parent.ChildNodes.Remove(node);
                    }

                    if (SelectedPath != null && SelectedPath.StartsWith(e.FullPath))
                        SelectedPath = null;
                }
            });
        }

        private void FWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                string pathWithoutNode = Path.GetDirectoryName(e.FullPath);
                string left = pathWithoutNode.Replace(rootPath, "");

                if (string.IsNullOrWhiteSpace(left))
                    _nodes.Add(new ExplorerNode(e.FullPath, this));
                else
                {
                    var node = FindNode(pathWithoutNode, _nodes);
                    if (node != null)
                        node.ChildNodes.Add(new ExplorerNode(e.FullPath, this));
                }
            });
        }

        private ExplorerNode? FindNode(string Path, ObservableCollection<ExplorerNode> Nodes)
        {
            foreach (var node in Nodes)
            {
                if(node.Path == Path)
                    return node;
            }

            foreach (var node in Nodes)
            {
                if (!node.IsFile)
                {
                    var n = FindNode(Path, node.ChildNodes);

                    if(n != null)
                        return n;
                }
            }

            return null;
        }

        private void TvExplorer_DoubleTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var source = e.Source as Control;

            TreeViewItem? item = null;
            
            if (source is TreeViewItem)
                item = source as TreeViewItem;
            else
                item = source.FindAncestorOfType<TreeViewItem>();

            if(item == null) 
                return;

            var data = item.DataContext as ExplorerNode;

            if(data == null) 
                return;

            if (data.IsFile)
                RaiseEvent(new RoutedEventArgs(OpenFileRequestedEvent));
            else
                item.IsExpanded = !item.IsExpanded;
        }

        private ObservableCollection<ExplorerNode> ScanFolder(string Folder)
        {
            ObservableCollection<ExplorerNode> nodes = new ObservableCollection<ExplorerNode>();
            var folders = System.IO.Directory.GetDirectories(Folder);
            foreach (var folder in folders) 
            {
                ExplorerNode node = new ExplorerNode(folder, this);
                node.ChildNodes = ScanFolder(folder);
                nodes.Add(node);
            }
            var files = System.IO.Directory.GetFiles(Folder);
            foreach (var file in files) 
            {
                var name = System.IO.Path.GetFileName(file);
                if (name.Contains(".compiletemp."))
                    continue;
                ExplorerNode node = new ExplorerNode(file, this);
                nodes.Add(node);
            }
            return nodes;
        }
        public class ExplorerNode : AvaloniaObject
        {

            static Bitmap bmpZxb;
            static Bitmap bmpAsm;
            static Bitmap bmpFile;
            static Bitmap bmpConfig;
            static Bitmap bmpFolder;

            public static readonly StyledProperty<ObservableCollection<ExplorerNode>> ChildNodesProperty = StyledProperty<ObservableCollection<ExplorerNode>>.Register<ExplorerNode, ObservableCollection<ExplorerNode>>("ChildNodes");
            public static readonly StyledProperty<string> TextProperty = StyledProperty<string>.Register<ExplorerNode, string>("Text");
            public static readonly StyledProperty<Bitmap> ImageProperty = StyledProperty<Bitmap>.Register<ExplorerNode, Bitmap>("Image");
            static ExplorerNode()
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                bmpZxb = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/zxbFile.png")));
                bmpAsm = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/asmFile.png")));
                bmpFile = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/unknFile.png")));
                bmpConfig = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/cfgFile.png")));
                bmpFolder = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/folder.png")));
            }

            ZXProjectExplorer explorer;
            public ObservableCollection<ExplorerNode> ChildNodes 
            { 
                get { return GetValue<ObservableCollection<ExplorerNode>>(ChildNodesProperty); }
                set { SetValue(ChildNodesProperty, value); }
            }
            public bool IsFile { get { return System.IO.File.Exists(Path);   } }
            public string Path { get; set; }
            public string Text 
            { 
                get { return GetValue<string>(TextProperty); }
                set { SetValue(TextProperty, value); }
            }
            public Bitmap Image
            {
                get { return GetValue<Bitmap>(ImageProperty); }
                set { SetValue(ImageProperty, value); }
            }
            public ExplorerNode(string path, ZXProjectExplorer explorer)
            {
                
                this.explorer = explorer;
                ChildNodes = new ObservableCollection<ExplorerNode>();
                UpdateNode(path);
            }
            public void UpdateNode(string NewPath)
            {
                Path = NewPath;
                Text = System.IO.Path.GetFileName(NewPath);
                if (System.IO.File.Exists(Path))
                {
                    if (Path.IsZXBasic())
                        Image = bmpZxb;
                    else if (Path.IsZXAssembler())
                        Image = bmpAsm;
                    else if (Path.IsZXConfig())
                        Image = bmpConfig;
                    else
                        Image = bmpFile;
                }
                else if (System.IO.Directory.Exists(Path))
                {
                    Image = bmpFolder;
                }
                else
                {
                    Image = bmpFile;
                }
            }

            
        }
    }
}
