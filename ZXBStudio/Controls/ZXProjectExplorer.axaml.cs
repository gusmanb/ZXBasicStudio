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
using System.Xml.Linq;
using ZXBasicStudio.Classes;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Configuration;

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

        public event EventHandler<EventArgs>? SelectedPathChanged;

        FileSystemWatcher? fWatcher;

        SortableObservableCollection<ExplorerNode> _nodes;

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
            //tvExplorer.DoubleTapped += TvExplorer_DoubleTapped;
            tvExplorer.SelectionChanged += TvExplorer_SelectionChanged;
            _nodes = new SortableObservableCollection<ExplorerNode>();
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
                tvExplorer.ItemsSource = null;
                return;
            }

            rootPath = System.IO.Path.GetFullPath(RootFolder);
            _nodes = ScanFolder(RootFolder);
            tvExplorer.ItemsSource = _nodes;
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
            {
                tvExplorer.SelectedItem = node;
            }
        }

        public string[] GetChildFiles(string Path)
        {
            var node = FindNode(Path, _nodes);

            if(node != null)
                return node.ChildNodes.Where(c => c.IsFile).Select(c => c.Path).ToArray();

            return new string[0];
        }


        private void FWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                
                var node = FindNode(e.OldFullPath, _nodes);

                if (node != null)
                {
                    //Rename the node
                    node.UpdateNode(e.FullPath);

                    string pathWithoutNode = Path.GetDirectoryName(e.FullPath) ?? "";
                    string left = pathWithoutNode.Replace(rootPath ?? "", "");
                    bool isFile = File.Exists(e.FullPath);

                    SortableObservableCollection<ExplorerNode>? container = null;

                    //Find where it is contained
                    if (string.IsNullOrWhiteSpace(left))
                        container = _nodes;
                    else
                    {
                        var parent = FindNode(pathWithoutNode, _nodes);
                        if (parent != null)
                            container = parent.ChildNodes;
                    }

                    if (container != null)
                    {
                        bool needsUpdate = true;

                        //Check for child hierarchy changes
                        if (isFile)
                        {
                            //Do the node has childs?
                            if (node.ChildNodes.Count > 0 && isFile)
                            {
                                //Yes, move them to the parent as the name now does not match
                                foreach (var childNode in node.ChildNodes)
                                    container.Add(childNode);

                                //Clear the child nodes
                                node.ChildNodes.Clear();

                                //Collection has been sorted by the change of items, no need to update it
                                needsUpdate = false;
                            }

                            var docCfg = ZXDocumentProvider.GetDocumentTypeInstance(typeof(ZXConfigurationDocument));

                            //Check if the node can be a child of an existing file
                            string fileExt = Path.GetExtension(e.FullPath);

                            if (docCfg != null && docCfg.DocumentExtensions.Contains(fileExt))
                            {
                                var possibleParent = e.FullPath.Substring(0, e.FullPath.Length - fileExt.Length);
                                var parent = FindNode(possibleParent, _nodes);

                                //This is a child, move it to its parent
                                if (parent != null)
                                {
                                    container.Remove(node);
                                    parent.ChildNodes.Add(node);
                                }

                            }

                            //Check if the node can contain childs
                            if (docCfg != null)
                            {
                                foreach (var ext in docCfg.DocumentExtensions)
                                {
                                    string child = e.FullPath + ext;
                                    var childNode = container.FirstOrDefault(n => n.Path == child);

                                    //We found a child, move it
                                    if (childNode != null)
                                    {
                                        container.Remove(childNode);
                                        node.ChildNodes.Add(childNode);

                                        //Collection has been sorted by the change of items, no need to update it
                                        needsUpdate = false;
                                    }
                                }
                            }

                            string oldFileExt = Path.GetExtension(e.OldFullPath);

                            //Check if the node was a child
                            if (docCfg != null)
                            {
                                var possibleParent = e.OldFullPath.Substring(0, e.OldFullPath.Length - oldFileExt.Length);
                                var parent = FindNode(possibleParent, _nodes);

                                //This is a child, remove it from its parent and add it to the upper container
                                if (parent != null)
                                {
                                    parent.ChildNodes.Remove(node);
                                    string parentPathWithoutNode = Path.GetDirectoryName(parent.Path) ?? "";
                                    string parentLeft = pathWithoutNode.Replace(rootPath ?? "", "");

                                    if (string.IsNullOrWhiteSpace(parentLeft))
                                        container = _nodes;
                                    else
                                    {
                                        parent = FindNode(pathWithoutNode, _nodes);
                                        if (parent != null)
                                            container = parent.ChildNodes;
                                    }

                                    if (container != null)
                                    {
                                        container.Add(node);
                                        needsUpdate = false;
                                    }
                                }
                            }
                        }

                        //Collection needs to be sorted
                        if (needsUpdate && container != null)
                            container.Sort();
                    }

                    if (tvExplorer.SelectedItem == node)
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
                    string pathWithoutNode = Path.GetDirectoryName(e.FullPath) ?? "";
                    string left = pathWithoutNode.Replace(rootPath ?? "", "");

                    SortableObservableCollection<ExplorerNode>? container = null;

                    //Find where it is contained
                    if (string.IsNullOrWhiteSpace(left))
                        container = _nodes;
                    else
                    {
                        var parent = FindNode(pathWithoutNode, _nodes);
                        if (parent != null)
                            container = parent.ChildNodes;
                    }

                    //Not found? ingnore...
                    if (container != null)
                    {
                        container.Remove(node);
                        //If it is a file and the file had childs, add it to the container
                        if(node.IsFile && node.ChildNodes.Count > 0)
                        {
                            foreach (var cNode in node.ChildNodes)
                                container.Add(cNode);
                        }
                    }

                    if (SelectedPath != null && SelectedPath.StartsWith(e.FullPath))
                        SelectedPath = null;
                }
            });
        }

        private void FWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() => 
            {
                bool isFile = File.Exists(e.FullPath);
                string pathWithoutNode = Path.GetDirectoryName(e.FullPath) ?? "";
                string left = pathWithoutNode.Replace(rootPath ?? "", "");
                var newNode = new ExplorerNode(e.FullPath, this);
                var docCfg = ZXDocumentProvider.GetDocumentTypeInstance(typeof(ZXConfigurationDocument));

                SortableObservableCollection<ExplorerNode>? container = null;

                //Find where to add the new node
                if (string.IsNullOrWhiteSpace(left))
                    container = _nodes;
                else
                {
                    var node = FindNode(pathWithoutNode, _nodes);
                    if (node != null)
                        container = node.ChildNodes;
                }

                //We didn't found where to add it... :(
                if (container == null)
                    return;

                //Check if any existing file at the same level is a child or if this is a child
                if (isFile && docCfg != null)
                {
                    //Check if the node can be a child of an existing file

                    string fileExt = Path.GetExtension(e.FullPath);

                    if (docCfg != null && docCfg.DocumentExtensions.Contains(fileExt))
                    {
                        var possibleParent = e.FullPath.Substring(0, e.FullPath.Length - fileExt.Length);
                        var parent = container.FirstOrDefault(n => n.Path == possibleParent);

                        //This is a child
                        if (parent != null)
                        {
                            parent.ChildNodes.Add(newNode);
                            return;
                        }
                    }

                    //Add it to the hierarchy
                    container.Add(newNode);

                    foreach (var ext in docCfg.DocumentExtensions)
                    {
                        string child = e.FullPath + ext;
                        var childNode = container.FirstOrDefault(n => n.Path == child);

                        //We found a child, move it
                        if (childNode != null)
                        {
                            container.Remove(childNode);
                            newNode.ChildNodes.Add(childNode);
                        }
                    }
                }
                else
                    //Add it to the hierarchy
                    container.Add(newNode);
            });
        }

        private ExplorerNode? FindNode(string Path, SortableObservableCollection<ExplorerNode> Nodes)
        {
            foreach (var node in Nodes)
            {
                if(node.Path == Path)
                    return node;
            }

            foreach (var node in Nodes)
            {
                var n = FindNode(Path, node.ChildNodes);

                if(n != null)
                    return n;
                
            }

            return null;
        }

        private void TvExplorer_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
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
            {
                RaiseEvent(new RoutedEventArgs(OpenFileRequestedEvent));
                e.Handled = true;
            }
        }

        private SortableObservableCollection<ExplorerNode> ScanFolder(string Folder)
        {
            SortableObservableCollection<ExplorerNode> nodes = new SortableObservableCollection<ExplorerNode> { SortingSelector = node => $"{(node.IsFile ? "0000-0001" : "0000-0000")} - {node.Text}" };
            var folders = System.IO.Directory.GetDirectories(Folder);
            foreach (var folder in folders) 
            {
                ExplorerNode node = new ExplorerNode(folder, this);
                node.ChildNodes = ScanFolder(folder);
                nodes.Add(node);
            }
            var files = System.IO.Directory.GetFiles(Folder);

            List<string> childFiles = new List<string>();
            var cfgType = ZXDocumentProvider.GetDocumentTypeInstance(typeof(ZXConfigurationDocument));

            //Check for child cfg's
            if (cfgType != null)
            {
                //Warning! We can have problems if casing does not match
                var nonCfgFiles = files.Where(f => !cfgType.DocumentExtensions.Contains(Path.GetExtension(f)));

                foreach (var noCfg in nonCfgFiles)
                {
                    foreach (var ext in cfgType.DocumentExtensions)
                    {
                        string cfg = noCfg + ext;
                        if (files.Contains(cfg))
                            childFiles.Add(cfg);
                    }
                }
            }

            foreach (var file in files) 
            {
                //If this is a child, ignore it for now
                if (childFiles.Contains(file))
                    continue;

                ExplorerNode node = new ExplorerNode(file, this);
                nodes.Add(node);

                if (cfgType != null)
                {
                    //Check for any child
                    foreach (var ext in cfgType.DocumentExtensions)
                    {
                        string cfg = file + ext;
                        if (childFiles.Contains(cfg))
                        {
                            ExplorerNode childNode = new ExplorerNode(cfg, this);
                            node.ChildNodes.Add(childNode);
                            childFiles.Add(cfg);
                        }
                    }
                }
            }
            return nodes;
        }
        public class ExplorerNode : AvaloniaObject
        {

            static Bitmap bmpFile;
            static Bitmap bmpFolder;

            public static readonly StyledProperty<SortableObservableCollection<ExplorerNode>> ChildNodesProperty = StyledProperty<SortableObservableCollection<ExplorerNode>>.Register<ExplorerNode, SortableObservableCollection<ExplorerNode>>("ChildNodes");
            public static readonly StyledProperty<string> TextProperty = StyledProperty<string>.Register<ExplorerNode, string>("Text");
            public static readonly StyledProperty<Bitmap> ImageProperty = StyledProperty<Bitmap>.Register<ExplorerNode, Bitmap>("Image");

            static ExplorerNode()
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                bmpFile = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/unknFile.png")));
                bmpFolder = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/folder.png")));
            }

            ZXProjectExplorer explorer;
            public SortableObservableCollection<ExplorerNode> ChildNodes 
            { 
                get { return GetValue<SortableObservableCollection<ExplorerNode>>(ChildNodesProperty); }
                set { SetValue(ChildNodesProperty, value); }
            }
            bool _isFile;
            public bool IsFile { get { return _isFile;   } }
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
                ChildNodes = new SortableObservableCollection<ExplorerNode> { SortingSelector = node => $"{ (node.IsFile ? "0000-0001" : "0000-0000") } - {node.Text}" };
                UpdateNode(path);
            }
            public void UpdateNode(string NewPath)
            {
                Path = NewPath;
                Text = System.IO.Path.GetFileName(NewPath);
                if (System.IO.File.Exists(Path))
                {

                    var docType = ZXDocumentProvider.GetDocumentType(Path);

                    if (docType != null)
                        Image = docType.DocumentIcon;
                    else
                        Image = bmpFile;

                    _isFile = true;
                }
                else if (System.IO.Directory.Exists(Path))
                {
                    Image = bmpFolder;
                }
                else
                {
                    Image = bmpFile;
                    _isFile = true;
                }


            }

            
        }
    }
}
