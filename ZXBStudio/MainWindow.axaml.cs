using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Skia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreSpectrum.Debug;
using CoreSpectrum.Enums;
using CoreSpectrum.SupportClasses;
using HarfBuzzSharp;
using Metsys.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using Svg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.BuildSystem;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Common;
using ZXBasicStudio.Common.ZXSinclairBasic;
using ZXBasicStudio.Controls;
using ZXBasicStudio.Controls.DockSystem;
using ZXBasicStudio.Dialogs;
using ZXBasicStudio.DocumentEditors;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;
using ZXBasicStudio.Emulator.Classes;
using ZXBasicStudio.Emulator.Controls;
using ZXBasicStudio.Extensions;
using I = ZXBasicStudio.Common.ZXSinclairBasic.ZXSinclairBasicInstruction;

namespace ZXBasicStudio
{
    public partial class MainWindow : ZXWindowBase //, IObserver<RawInputEventArgs>
    {
        const string repoUrl = "https://github.com/gusmanb/ZXBasicStudio";
        const string zxbHelpUrl = "https://zxbasic.readthedocs.io/en/docs/";

        #region Shortcut handling

        internal static Guid KeybSourceId = Guid.Parse("72af48c7-4d62-4bef-8676-63c10d99de20");

        internal static ZXKeybCommand[] KeybCommands =
        {
            new ZXKeybCommand { CommandId = Guid.Parse("62c23849-7312-41ac-8788-9f6d851cc3b9"), CommandName = "Build and run", Key = Key.F5, Modifiers = KeyModifiers.None },
            new ZXKeybCommand { CommandId = Guid.Parse("d9222ec4-5d7a-4b04-b9e3-e29d6d8bca78"), CommandName = "Build and debug", Key = Key.F6, Modifiers = KeyModifiers.None },
            new ZXKeybCommand { CommandId = Guid.Parse("57aff55a-f4a1-4a57-a532-a38117e1a532"), CommandName = "Pause emulation", Key = Key.F7, Modifiers = KeyModifiers.None },
            new ZXKeybCommand { CommandId = Guid.Parse("d4dc5ad4-d5f2-431c-a550-541b56d52fbb"), CommandName = "Resume emulation", Key = Key.F8, Modifiers = KeyModifiers.None },
            new ZXKeybCommand { CommandId = Guid.Parse("df3bacb0-baab-40b9-a287-653fa339fe1d"), CommandName = "Assembler step", Key = Key.F9, Modifiers = KeyModifiers.None },
            new ZXKeybCommand { CommandId = Guid.Parse("bd77367e-f17b-4550-9dd1-26013f7d250a"), CommandName = "Basic step", Key = Key.F10, Modifiers = KeyModifiers.None },
            new ZXKeybCommand { CommandId = Guid.Parse("cf0216e0-3180-4429-8e09-664e6262dbd9"), CommandName = "Stop emulation", Key = Key.F11, Modifiers = KeyModifiers.None },
            new ZXKeybCommand { CommandId = Guid.Parse("703808fa-abec-4c65-940b-16544c2bd93d"), CommandName = "Turbo mode", Key = Key.F12, Modifiers = KeyModifiers.None },
            new ZXKeybCommand { CommandId = Guid.Parse("077a0fb9-34a4-461b-9f44-8f3472f9e872"), CommandName = "Swap fullscreen mode", Key = Key.Enter, Modifiers = KeyModifiers.Alt },
            new ZXKeybCommand { CommandId = Guid.Parse("573741e3-5e26-4195-aa89-c49066a28762"), CommandName = "Code View", Key = Key.F8, Modifiers = KeyModifiers.Control | KeyModifiers.Shift },
            new ZXKeybCommand { CommandId = Guid.Parse("0aa2887d-4fb7-4234-81c1-c07efbf02eb3"), CommandName = "Project View", Key = Key.F9, Modifiers = KeyModifiers.Control | KeyModifiers.Shift },
            new ZXKeybCommand { CommandId = Guid.Parse("7b50369f-3f48-4653-99c1-26e566691e3c"), CommandName = "Debug View", Key = Key.F10, Modifiers = KeyModifiers.Control | KeyModifiers.Shift },
            new ZXKeybCommand { CommandId = Guid.Parse("86b341a1-8d07-4af2-b4b0-ca953cd3dbc0"), CommandName = "Play View", Key = Key.F11, Modifiers = KeyModifiers.Control | KeyModifiers.Shift },
            new ZXKeybCommand { CommandId = Guid.Parse("424f7395-c29d-44f9-8f9e-43b8891ec261"), CommandName = "All tools View", Key = Key.F12, Modifiers = KeyModifiers.Control | KeyModifiers.Shift },
            new ZXKeybCommand { CommandId = Guid.Parse("21bc5c34-df5e-449e-a826-88e1f42d7810"), CommandName = "Exit application", Key = Key.None, Modifiers = KeyModifiers.None }
        };

        Dictionary<Guid, Action> _shortcuts = new Dictionary<Guid, Action>();

        #endregion

        List<ZXDocumentEditorBase> openDocuments = new List<ZXDocumentEditorBase>();

        ObservableCollection<TabItem> editTabs = new ObservableCollection<TabItem>();

        ZXProgram? loadedProgram;
        List<Breakpoint> basicBreakpoints = new List<Breakpoint>();
        List<Breakpoint> disassemblyBreakpoints = new List<Breakpoint>();
        List<Breakpoint> romBreakpoints = new List<Breakpoint>();
        List<Breakpoint> userBreakpoints = new List<Breakpoint>();
        List<ZXCodeLine> romLines = new List<ZXCodeLine>();
        Breakpoint? currentBp;
        public ObservableCollection<TabItem> EditItems { get; set; }

        public FileInfoProvider FileInfo { get; set; } = new FileInfoProvider();
        public EmulatorInfoProvider EmulatorInfo { get; set; } = new EmulatorInfoProvider();

        protected override bool PersistBounds { get { return true; } }

        private object? rootContent = null;

        private bool skipLayout;

        ZXTapePlayer _player;
        ZXDockingControl _playerDock;

        ZXDocumentEditorBase? _activeEditor = null;

        bool skipCloseCheck = false;

        public MainWindow()
        {
            InitializeComponent();

            GlobalKeybHandler.KeyUp += GlobalKeyUp;

            EditItems = editTabs;
            DataContext = this;

            #region Attach explorer events
            peExplorer.OpenFileRequested += OpenFile;
            peExplorer.NewFileRequested += CreateFile;
            peExplorer.NewFolderRequested += CreateFolder;
            peExplorer.RenameRequested += Rename;
            peExplorer.DeleteRequested += Delete;
            peExplorer.SelectedPathChanged += PeExplorer_SelectedPathChanged;
            #endregion

            #region Attach menu events
            mnuOpenProject.Click += OpenProject;
            mnuOpenLastProject.Click += OpenLastProject;
            mnuCreateProject.Click += CreateProject;
            mnuCreateFolder.Click += CreateFolder;
            mnuCreateFile.Click += CreateFile;
            mnuSaveFile.Click += SaveFile;
            mnuCloseProject.Click += CloseProject;
            mnuCloseFile.Click += CloseFile;
            mnuExitApplication.Click += ExitApplication;
            mnuConfigureProject.Click += ConfigureProject;
            mnuBuild.Click += Build;
            mnuBuildRun.Click += BuildAndRun;
            mnuBuildDebug.Click += BuildAndDebug;
            mnuExport.Click += Export;
            mnuPause.Click += PauseEmulator;
            mnuContinue.Click += ResumeEmulator;
            mnuBasicStep.Click += BasicStepEmulator;
            mnuAssemblerStep.Click += AssemblerStepEmulator;
            mnuGlobalOptions.Click += ConfigureGlobalSettings;
            mnuDumpMem.Click += DumpMemory;
            mnuDumpRegs.Click += DumpRegisters;
            mnuTurbo.Click += TurboModeEmulator;
            mnuRestoreLayout.Click += RestoreLayout;
            mnuCodeView.Click += FullLayout;
            mnuProjectView.Click += ExplorerLayout;
            mnuAllToolsView.Click += ToolsLayout;
            mnuDebugView.Click += DebugLayout;
            mnuPlayView.Click += PlayLayout;
            mnuRepo.Click += OpenRepository;
            mnuZXHelp.Click += OpenZXHelp;
            mnuAbout.Click += OpenAbout;
            #endregion

            #region Attach toolbar events
            btnOpenProject.Click += OpenProject;
            btnNewFolder.Click += CreateFolder;
            btnNewFile.Click += CreateFile;
            btnSave.Click += SaveFile;
            btnSaveAll.Click += SaveAllFiles;
            btnRun.Click += BuildAndRun;
            btnDebug.Click += BuildAndDebug;
            btnPause.Click += PauseEmulator;
            btnResume.Click += ResumeEmulator;
            btnNextInstruction.Click += AssemblerStepEmulator;
            btnNextLine.Click += BasicStepEmulator;
            btnStop.Click += StopEmulator;
            btnFontIncrease.Click += BtnFontIncrease_Click;
            btnFontDecrease.Click += BtnFontDecrease_Click;
            btnCollapse.Click += BtnCollapse_Click;
            btnExpand.Click += BtnExpand_Click;
            btnComment.Click += BtnComment_Click;
            btnUncomment.Click += BtnUncomment_Click;
            btnRemoveBreakpoints.Click += BtnRemoveBreakpoints_Click;
            btnTurbo.Click += TurboModeEmulator;
            btnBorderless.Click += Borderless;
            btnDirectScreen.Click += DirectScreen;
            btnTape.Click += ShowTapePlayer;
            btnPowerOn.Click += PowerOn;
            btnMapKeyboard.Click += BtnMapKeyboard_Click;
            #endregion

            #region Attach Breakpoint manager events
            BreakpointManager.BreakPointAdded += BreakpointManager_BreakPointAdded;
            BreakpointManager.BreakPointRemoved += BreakpointManager_BreakPointRemoved;
            #endregion

            #region Attach emulator events
            emu.Breakpoint += Emu_Breakpoint;
            emu.ProgramReady += Emu_ProgramReady;
            emu.ExceptionTrapped += Emu_ExceptionTrapped;
            #endregion

            #region Attach editors view events
            tcEditors.SelectionChanged += TcEditors_SelectionChanged;
            #endregion

            #region Debugging tools initialization
            regView.Registers = emu.Registers;
            memView.Initialize(emu.Memory);
            CreateRomBreakpoints();
#if DEBUG
            this.AttachDevTools();
#endif
            #endregion

            #region Player intialization

            _player = new ZXTapePlayer();
            _player.Datacorder = emu.Datacorder;
            _playerDock = new ZXDockingControl();
            _playerDock.DockedControl = _player;
            _playerDock.CanClose = true;
            _playerDock.Title = "Tape player";
            _playerDock.DesiredFloatingSize = new Size(230, 270);
            _playerDock.Name = "TapePlayerDock";

            #endregion

            #region Shortcut initialization
            _shortcuts = new Dictionary<Guid, Action>()
            {
                { Guid.Parse("62c23849-7312-41ac-8788-9f6d851cc3b9"), () => {
                    if (EmulatorInfo.CanRun)
                        BuildAndRun(this, new RoutedEventArgs());} },
                { Guid.Parse("d9222ec4-5d7a-4b04-b9e3-e29d6d8bca78"), () => {
                    if (EmulatorInfo.CanDebug)
                        BuildAndDebug(this, new RoutedEventArgs()); } },
                { Guid.Parse("57aff55a-f4a1-4a57-a532-a38117e1a532"), () => {
                    if (EmulatorInfo.CanPause)
                        PauseEmulator(this, new RoutedEventArgs());} },
                { Guid.Parse("d4dc5ad4-d5f2-431c-a550-541b56d52fbb"), () => {
                    if (EmulatorInfo.CanResume)
                        ResumeEmulator(this, new RoutedEventArgs());} },
                { Guid.Parse("df3bacb0-baab-40b9-a287-653fa339fe1d"), () => {
                    if (EmulatorInfo.CanStep)
                        AssemblerStepEmulator(this, new RoutedEventArgs()); } },
                { Guid.Parse("bd77367e-f17b-4550-9dd1-26013f7d250a"), () => {
                    if (EmulatorInfo.CanStep)
                        BasicStepEmulator(this, new RoutedEventArgs());} },
                { Guid.Parse("cf0216e0-3180-4429-8e09-664e6262dbd9"), () => {
                    if (EmulatorInfo.IsRunning)
                        StopEmulator(this, new RoutedEventArgs());} },
                { Guid.Parse("703808fa-abec-4c65-940b-16544c2bd93d"), () => {
                    if (EmulatorInfo.IsRunning)
                        TurboModeEmulator(this, new RoutedEventArgs());} },
                { Guid.Parse("077a0fb9-34a4-461b-9f44-8f3472f9e872"), () => {
                    if (EmulatorInfo.IsRunning)
                        SwapFullScreen();} },
                { Guid.Parse("573741e3-5e26-4195-aa89-c49066a28762"), () => {
                        FullLayout(this, new RoutedEventArgs()); }},
                { Guid.Parse("0aa2887d-4fb7-4234-81c1-c07efbf02eb3"), () => {
                        ExplorerLayout(this, new RoutedEventArgs()); }},
                { Guid.Parse("7b50369f-3f48-4653-99c1-26e566691e3c"), () => {
                        DebugLayout(this, new RoutedEventArgs()); }},
                { Guid.Parse("86b341a1-8d07-4af2-b4b0-ca953cd3dbc0"), () => {
                        PlayLayout(this, new RoutedEventArgs()); }},
                { Guid.Parse("424f7395-c29d-44f9-8f9e-43b8891ec261"), () => {
                        ToolsLayout(this, new RoutedEventArgs()); }},
                { Guid.Parse("21bc5c34-df5e-449e-a826-88e1f42d7810"), () => {
                       ExitApplication(this, new RoutedEventArgs());
                }},
            };
            #endregion

            #region Default emulator settings
            btnBorderless.IsChecked = emu.Borderless = ZXOptions.Current.Borderless;
            emu.AntiAlias = ZXOptions.Current.AntiAlias;
            #endregion

            //Layout restoration
            ZXLayoutPersister.RestoreLayout(grdMain, dockLeft, dockRight, dockBottom, new[] { _playerDock });
        }

        private void OpenAbout(object? sender, RoutedEventArgs e)
        {
            ZXAboutDialog zXAboutDialog = new ZXAboutDialog();
            zXAboutDialog.ShowDialog(this);
        }

        private void OpenZXHelp(object? sender, RoutedEventArgs e)
        {
            OpenUrl(zxbHelpUrl);
        }

        private void OpenRepository(object? sender, RoutedEventArgs e)
        {
            OpenUrl(repoUrl);
        }

        #region File manipulation
        private void PeExplorer_SelectedPathChanged(object? sender, System.EventArgs e)
        {
            if (peExplorer.SelectedPath != null)
                FileInfo.FileSystemObjectSelected = true;
        }
        private async void Delete(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

            var path = peExplorer.SelectedPath;
            if (path == null)
                return;

            bool isFile = File.Exists(path);
            bool confirm = false;
            if (isFile)
                confirm = await this.ShowConfirm("Delete file", $"Are you sure you want to delete the file \"{Path.GetFileName(path)}\"?");
            else
                confirm = await this.ShowConfirm("Delete folder", $"Are you sure you want to delete the folder \"{Path.GetFileName(path)}\" and all its content?");

            if (!confirm)
                return;
            try
            {
                if (isFile)
                    File.Delete(path);
                else
                    Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                await this.ShowError("Delete error", $"Unexpected error trying to delete the {(isFile ? "file" : "directory")}: {ex.Message} - {ex.StackTrace}");
            }
        }

        private async void Rename(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var path = peExplorer.SelectedPath;
            if (path == null)
                return;

            bool isFile = File.Exists(path);

            //TODO: Check better, it might match a folder wich has a partial name of other folder
            if (!isFile && openDocuments.Any(e => e.DocumentPath.ToLower().StartsWith(path.ToLower())))
            {
                await this.ShowError("Open documents", "There are open documents in the selected folder, close any document in the folder before renaming it.");
                return;
            }

            string? newName = null;
            if (isFile)
                newName = await this.ShowInput("Rename file", "Select the new name for the file", "File name", Path.GetFileName(path));
            else
                newName = await this.ShowInput("Rename folder", "Select the new name for the folder", "Folder name", Path.GetFileName(path));

            if (newName == null)
                return;
            try
            {
                if (isFile)
                {
                    var dir = Path.GetDirectoryName(path);
                    string newPath = Path.Combine(dir ?? "", newName);
                    var childFiles = peExplorer.GetChildFiles(path);

                    //Compose a list of old and new files, and its related document editors
                    List<(string oldFile, string newFile, ZXDocumentEditorBase? editor)> files = new List<(string oldFile, string newFile, ZXDocumentEditorBase? editor)>();

                    files.Add((path, newPath, openDocuments.FirstOrDefault(d => d.DocumentPath == path)));

                    foreach (var childPath in childFiles)
                    {
                        string ext = Path.GetExtension(childPath);
                        string cNewPath = newPath + ext;
                        files.Add((childPath, cNewPath, openDocuments.FirstOrDefault(d => d.DocumentPath == childPath)));
                    }

                    //First check if the file or any of its childs are open and modified
                    if (files.Any(f => f.editor?.Modified ?? false))
                    {
                        await this.ShowError("Rename file", "The file or one of its childs you are trying to rename is open and modified. Save or discard the changes before renaming.");
                        return;
                    }

                    //Create a list of renamed files in case of the need to undo
                    List<(string oldFile, string newFile, ZXDocumentEditorBase? editor)> renamed = new List<(string oldFile, string newFile, ZXDocumentEditorBase? editor)>();

                    bool undo = false;

                    //Move the files
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Move(file.oldFile, file.newFile);
                            renamed.Add(file);

                            //Notify the editor about the file movement
                            if (file.editor != null)
                            {
                                if (!file.editor.RenameDocument(file.newFile, outLog.Writer))
                                {
                                    //Error in the editor!
                                    await this.ShowError("Error", "Error renaming the file, check the output log for more information.");
                                    undo = true;
                                    break;
                                }

                                //Update editor tab with new file name
                                var tab = editTabs.First(t => t.Content == file.editor);
                                tab.Tag = Path.GetFileName(file.newFile);
                            }
                        }
                        catch
                        {
                            undo = true;
                            break;
                        }
                    }

                    //Something went wrong, try to undo
                    if (undo)
                    {
                        foreach (var file in renamed)
                        {
                            try
                            {
                                File.Move(file.newFile, file.oldFile);
                                if (file.editor != null)
                                {
                                    file.editor.RenameDocument(file.oldFile, outLog.Writer);
                                    //Update editor tab with new file name
                                    var tab = editTabs.First(t => t.Content == file.editor);
                                    tab.Tag = Path.GetFileName(file.newFile);
                                }
                            }
                            catch
                            {
                                //Total failure, we can't do more...
                                await this.ShowError("Error", "Error reverting file changes, manual intervention required.");
                                break;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                await this.ShowError("Rename error", $"Unexpected error trying to rename the {(isFile ? "file" : "directory")}: {ex.Message} - {ex.StackTrace}");
            }
        }

        public async void CloseFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            TabItem? tab;

            if (sender is Button)
            {
                var button = sender as Button;

                if (button == null)
                    return;

                tab = button.FindAncestorOfType<TabItem>();

                if (tab == null)
                    return;
            }
            else
            {
                tab = editTabs.Where(t => t.IsSelected).FirstOrDefault();
                if (tab == null)
                    return;
            }

            if (tab.Content is ZXDocumentEditorBase)
            {
                var document = tab.Content as ZXDocumentEditorBase;

                if (document == null)
                    return;

                if (document.Modified)
                {
                    var res = await this.ShowConfirm("Modified", "This document has been modified, if you close it now you will lose the changes, are you sure you want to close it?");

                    if (!res)
                        return;

                    document.CloseDocument(outLog.Writer, true);
                }
                else
                {
                    if (!document.CloseDocument(outLog.Writer, false))
                    {
                        var res = await this.ShowConfirm("Close error", "There was an error closing the document, do you want to force it? (more info on the output log).");

                        if (!res)
                            return;

                        document.CloseDocument(outLog.Writer, true);
                    }
                }

                openDocuments.Remove(document);
                document.Dispose();
            }

            editTabs.Remove(tab);

            if (openDocuments.Count == 0)
                FileInfo.FileLoaded = false;
        }

        private async void CloseProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (openDocuments.Any(e => e.Modified))
            {
                var resConfirm = await this.ShowConfirm("Modified documents", "Some documents have been modified but not saved, if you close the project all the changes will be lost, are you sure you want to close the project?");

                if (!resConfirm)
                    return;
            }

            foreach (var doc in openDocuments)
                doc.CloseDocument(outLog.Writer, true);

            ZXProjectManager.CloseProject();

            editTabs.Clear();
            peExplorer.UpdateProjectFolder();
            FileInfo.FileLoaded = false;
            FileInfo.ProjectLoaded = false;
            EmulatorInfo.CanDebug = false;
            EmulatorInfo.CanRun = false;
            EmulatorInfo.CanPause = false;
            EmulatorInfo.CanStep = false;
            Cleanup();
            BreakpointManager.ClearBreakpoints();
        }

        private async void SaveFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var activeTab = editTabs.FirstOrDefault(t => t.IsSelected);

            if (activeTab == null)
                return;

            var editor = activeTab.Content as ZXDocumentEditorBase;

            if (editor == null)
                return;

            if (!editor.SaveDocument(outLog.Writer))
            {
                await this.ShowError("Error", "Cannot save the file, check the output log for more info.");
                return;
            }
        }

        private void SaveAllFiles(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SaveAllFiles();
        }

        private bool SaveAllFiles()
        {
            foreach (var edit in openDocuments)
            {
                if (edit.Modified)
                    if (!edit.SaveDocument(outLog.Writer))
                        return false;
            }

            return true;
        }

        private async void OpenFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var path = peExplorer.SelectedPath;

            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!File.Exists(path))
                return;

            await OpenFile(path);
        }

        private async void OpenNewFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

            var opts = new FilePickerOpenOptions();
            opts.FileTypeFilter = ZXDocumentProvider.GetDocumentFilters();
            opts.AllowMultiple = false;
            opts.Title = "Open file...";

            var result = await this.StorageProvider.OpenFilePickerAsync(opts);

            if (result == null || result.Count == 0)
                return;

            await OpenFile(result[0].Path.LocalPath);
        }

        private async void OpenProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (FileInfo.ProjectLoaded)
            {
                if (openDocuments.Any(e => e.Modified))
                {
                    var resConfirm = await this.ShowConfirm("Warning!", "There are unsaved documents, opening a new project will discard the changes, are you sure you want to open a new project?");

                    if (!resConfirm)
                        return;

                    foreach (var doc in openDocuments)
                        doc.CloseDocument(outLog.Writer, true);
                }

                ZXProjectManager.CloseProject();
            }

            FolderPickerOpenOptions opts = new FolderPickerOpenOptions { AllowMultiple = false, Title = "Open project" };
            var res = await this.StorageProvider.OpenFolderPickerAsync(opts);

            if (res == null || res.Count == 0)
                return;

            ZXProjectManager.OpenProject(res[0].Path.LocalPath);
            ZXOptions.Current.LastProjectPath = res[0].Path.LocalPath;
            ZXOptions.SaveCurrentSettings();

            Cleanup();
            BreakpointManager.ClearBreakpoints();
            peExplorer.UpdateProjectFolder();
            editTabs.Clear();
            openDocuments.Clear();
            FileInfo.ProjectLoaded = true;
            FileInfo.FileLoaded = false;
            FileInfo.FileSystemObjectSelected = false;
            EmulatorInfo.CanRun = true;
            EmulatorInfo.CanDebug = true;

        }

        private async void OpenLastProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

            if (!FileInfo.ProjectLoaded && string.IsNullOrEmpty(ZXOptions.Current.LastProjectPath) == false)
            {
                
                ZXProjectManager.OpenProject(ZXOptions.Current.LastProjectPath);

                Cleanup();
                BreakpointManager.ClearBreakpoints();
                peExplorer.UpdateProjectFolder();
                editTabs.Clear();
                openDocuments.Clear();
                FileInfo.ProjectLoaded = true;
                FileInfo.FileLoaded = false;
                FileInfo.FileSystemObjectSelected = false;
                EmulatorInfo.CanRun = true;
                EmulatorInfo.CanDebug = true;
                
            }
        }

        private async void CreateFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var path = peExplorer.SelectedPath;

            if (path == null)
                path = peExplorer.RootPath ?? "";

            if (File.Exists(path))
                path = Path.GetDirectoryName(path) ?? "";

            var dlg = new ZXNewFileDialog();
            var result = await dlg.ShowDialog<(IZXDocumentType DocumentType, string Name)?>(this);

            if (result == null)
                return;

            var finalName = Path.Combine(path, result.Value.Name);

            if (File.Exists(finalName))
            {
                if (!await this.ShowConfirm("Existing file", "Warning! File already exists, do you want to overwrite it?"))
                    return;

                try
                {
                    File.Delete(finalName);

                }
                catch (Exception ex)
                {
                    await this.ShowError("Error", "Cannot delete existing file, aborting.");
                    return;
                }
            }

            var created = result.Value.DocumentType.DocumentFactory.CreateDocument(finalName, outLog.Writer);

            if (!created)
            {
                await this.ShowError("Error", "Cannot create file, check the output log for more details");
                return;
            }

            await OpenFile(finalName);
            await Task.Delay(100);
            peExplorer.SelectPath(finalName);

        }

        private async void CreateFolder(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var path = peExplorer.SelectedPath;

            if (path == null)
                path = peExplorer.RootPath;

            if (File.Exists(path))
                path = Path.GetDirectoryName(path);

            var fileName = await this.ShowInput("New folder", "Enter the name of the folder to be created.", "Folder:");

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var finalPath = Path.Combine(path ?? "", fileName);

            try
            {
                Directory.CreateDirectory(finalPath);
            }
            catch (Exception ex)
            {
                await this.ShowError("Create error", $"Unexpected error trying to create the directory: {ex.Message} - {ex.StackTrace}");
            }
        }

        private async void CreateProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (FileInfo.ProjectLoaded)
            {
                if (openDocuments.Any(e => e.Modified))
                {
                    var resConfirm = await this.ShowConfirm("Warning!", "Current project has pending changes, creating a new project will discard those changes. Do you want to continue?");

                    if (!resConfirm)
                        return;

                    foreach (var doc in openDocuments)
                        doc.CloseDocument(outLog.Writer, true);

                }

                openDocuments.Clear();
                editTabs.Clear();
                ZXProjectManager.CloseProject();
            }

            outLog.Clear();

            var fld = await StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions { AllowMultiple = false, Title = "Select project's folder." });

            if (fld == null || fld.Count == 0)
                return;

            string selFolder = fld.First().Path.LocalPath;

            var dlg = new ZXBuildSettingsDialog();
            if (ZXOptions.Current.DefaultBuildSettings != null)
                dlg.Settings = ZXOptions.Current.DefaultBuildSettings.Clone();

            var setRes = await dlg.ShowDialog<bool>(this);

            if (!setRes)
                return;

            if (!ZXProjectManager.CreateProject(selFolder, dlg.Settings, outLog.Writer))
            {
                await this.ShowError("Create error", $"Error creating project, check the output log for more details.");
                return;
            }

            ZXProjectManager.OpenProject(selFolder);

            peExplorer.UpdateProjectFolder();
            editTabs.Clear();
            openDocuments.Clear();
            FileInfo.ProjectLoaded = true;
            FileInfo.FileLoaded = false;
            FileInfo.FileSystemObjectSelected = false;
            EmulatorInfo.CanDebug = true;
            EmulatorInfo.CanRun = true;
        }

        async Task<ZXDocumentEditorBase?> OpenFile(string file)
        {
            var opened = openDocuments.FirstOrDefault(ef => Path.GetFullPath(file) == Path.GetFullPath(ef.DocumentPath));
            if (opened != null)
            {
                var tab = editTabs.First(t => t.Content == opened);
                tab.IsSelected = true;
                return opened;
            }

            var docType = ZXDocumentProvider.GetDocumentType(file);

            if (docType == null || !docType.CanEdit)
            {
                await this.ShowError("Cannot open file", "The specified file is not editable by ZX Basic Studio. Operation aborted.");
                return null;
            }

            //TODO: Provide commands
            ZXDocumentEditorBase? editor = docType.DocumentFactory.CreateEditor(file, outLog.Writer);

            if (editor == null)
            {
                await this.ShowError("Cannot open file", "Error opening document, check the output log for more information.");
                return null;
            }


            TabItem tItem = new TabItem();
            tItem.Classes.Add("closeTab");
            tItem.Tag = Path.GetFileName(file);
            tItem.Content = editor;
            editTabs.Add(tItem);
            openDocuments.Add(editor);

            tItem.IsSelected = true;
            editor.DocumentModified += EditorDocumentModified;
            editor.DocumentSaved += EditorDocumentSaved;
            editor.DocumentRestored += EditorDocumentRestored;
            editor.RequestSaveDocument += EditorRequestSaveDocument;

            FileInfo.FileLoaded = true;
            peExplorer.SelectPath(file);

            if (editor is ZXTextEditor)
            {
                var txtEdit = (ZXTextEditor)editor;

                if (EmulatorInfo.IsRunning && !EmulatorInfo.IsPaused)
                    txtEdit.Readonly = true;
                else if (EmulatorInfo.IsRunning && EmulatorInfo.IsPaused)
                {
                    var bp = basicBreakpoints.FirstOrDefault(bp => bp.Address == emu.Registers.PC);

                    if (bp != null)
                    {
                        var line = bp.Tag as ZXCodeLine;
                        if (line != null)
                        {
                            if (line.File == file)
                                txtEdit.BreakLine = line.LineNumber + 1;
                        }
                    }
                }
            }

            return editor;


        }

        private async void EditorRequestSaveDocument(object? sender, EventArgs e)
        {
            var editor = sender as ZXDocumentEditorBase;

            if (editor == null)
                return;

            if (!editor.SaveDocument(outLog.Writer))
            {
                await this.ShowError("Error", "Cannot save the file, check if another program is blocking it.");
                return;
            }
        }

        private void EditorDocumentRestored(object? sender, EventArgs e)
        {
            if (sender is not ZXDocumentEditorBase)
                return;

            var editor = (ZXDocumentEditorBase)sender;

            var tab = editor.Parent as TabItem;
            if (tab == null)
                return;
            tab.Tag = tab.Tag?.ToString()?.Replace("*", "");
        }

        private void EditorDocumentSaved(object? sender, System.EventArgs e)
        {

            if (sender is not ZXDocumentEditorBase)
                return;

            var editor = (ZXDocumentEditorBase)sender;

            var tab = editor.Parent as TabItem;
            if (tab == null)
                return;
            tab.Tag = tab.Tag?.ToString()?.Replace("*", "");

        }

        private void EditorDocumentModified(object? sender, System.EventArgs e)
        {
            if (sender is not ZXDocumentEditorBase)
                return;

            var editor = (ZXDocumentEditorBase)sender;

            var tab = editor.Parent as TabItem;
            if (tab == null)
                return;
            tab.Tag = tab.Tag?.ToString() + "*";

        }


        private async Task CloseDocumentByFile(string File)
        {
            var disEdit = openDocuments.FirstOrDefault(ef => ef.DocumentPath == File);

            if (disEdit != null)
            {
                if (disEdit.Modified)
                {
                    var res = await this.ShowConfirm("Modified", "This document has been modified, if you close it now you will lose the changes, are you sure you want to close it?");

                    if (!res)
                        return;
                }

                disEdit.CloseDocument(outLog.Writer, true);
                openDocuments.Remove(disEdit);
                disEdit.Dispose();

                var tab = editTabs.First(t => t.Content == disEdit);
                editTabs.Remove(tab);

                if (openDocuments.Count == 0)
                    FileInfo.FileLoaded = false;
            }
        }
        #endregion

        #region Emulator control

        private void PowerOn(object? sender, RoutedEventArgs e)
        {
            CheckSpectrumModel();
            emu.Start();
            EmulatorInfo.IsRunning = true;
        }


        private void ShowTapePlayer(object? sender, RoutedEventArgs e)
        {
            if (!_player.IsAttachedToVisualTree())
                ZXFloatController.MakeFloating(_playerDock);

            _player.Datacorder = emu.Datacorder;
        }

        private void DirectScreen(object? sender, RoutedEventArgs e)
        {
            emu.DirectMode = btnDirectScreen.IsChecked ?? false;
        }

        private void Borderless(object? sender, RoutedEventArgs e)
        {
            emu.Borderless = btnBorderless.IsChecked ?? false;
        }
        public void SwapFullScreen()
        {

            ZXFloatingWindow? floatW = ZXFloatController.Windows.FirstOrDefault(w => w.DockingContainer.DockingControls.Contains(emuDock));

            if (floatW == null)
            {

                bool running = emu.Running;
                bool paused = emu.Paused;

                if (rootContent == null)
                {

                    if (running && !paused)
                        emu.Pause();

                    rootContent = this.Content;
                    emuDock.DockedControl = null;
                    //grdEmulator.Children.Remove(emu);
                    this.Content = emu;

                    if (running && !paused)
                        emu.Resume();

                    emu.Focus();
                }
                else
                {
                    if (running && !paused)
                        emu.Pause();

                    this.Content = rootContent;
                    emuDock.DockedControl = emu;
                    //grdEmulator.Children.Add(emu);

                    if (running && !paused)
                        emu.Resume();

                    rootContent = null;
                    emu.Focus();
                }
            }
            else
            {
                floatW.WindowState = floatW.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
            }
        }

        private void Emu_ExceptionTrapped(object? sender, ExceptionEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                outLog.Writer.WriteLine($"Exception in emulator: {e.TrappedException.Message} - {e.TrappedException.StackTrace}");
                outDock.Select();
            });
        }

        private void StopEmulator(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Cleanup();
            UnblockEditors();
            varsView.EndEdit();
            statesView.Clear();
            flagsView.Clear();
        }
        private async void PauseEmulator(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            emu.Pause();
            emu.TurboEnabled = false;
            EmulatorInfo.IsPaused = true;
            EmulatorInfo.CanPause = false;
            EmulatorInfo.CanResume = true;
            EmulatorInfo.CanStep = EmulatorInfo.IsDebugging;

            if (EmulatorInfo.IsDebugging)
            {
                ClearBreakLines();
                currentBp = await ShowBreakLines(emu.Registers.PC, true);
                regView.Update();
                flagsView.Update(emu.Registers.F);
                varsView.BeginEdit();
                statesView.Update(emu.TStates);
            }
        }
        private void ResumeEmulator(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LoadBreakpoints(LoadedBreakpoints.User);

            if (currentBp != null)
            {
                var bps = userBreakpoints.FirstOrDefault(b => b.Address == currentBp.Address);

                if (bps != null)
                    bps.Executed = true;
            }

            ClearBreakLines();
            emu.Resume();
            varsView.EndEdit();

            EmulatorInfo.IsPaused = false;
            EmulatorInfo.CanPause = true;
            EmulatorInfo.CanResume = false;
            EmulatorInfo.CanStep = false;
        }
        private void TurboModeEmulator(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!emu.Running)
                return;

            emu.TurboEnabled = !emu.TurboEnabled;
        }
        private void BasicStepEmulator(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ClearBreakLines();

            var textEditors = openDocuments.Where(doc => doc is ZXTextEditor).Cast<ZXTextEditor>();

            foreach (var edit in textEditors)
                edit.BreakLine = null;

            if (EmulatorInfo.LoadedBreakpoints != LoadedBreakpoints.Basic)
            {
                LoadBreakpoints(LoadedBreakpoints.Basic);

                if (currentBp != null)
                {
                    var bps = basicBreakpoints.FirstOrDefault(b => b.Address == currentBp.Address);

                    if (bps != null)
                        bps.Executed = true;
                }
            }

            EmulatorInfo.CanPause = true;
            EmulatorInfo.IsPaused = false;
            EmulatorInfo.CanResume = false;
            EmulatorInfo.CanStep = false;

            varsView.EndEdit();
            emu.Resume();
        }
        private async void AssemblerStepEmulator(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ClearBreakLines();

            LoadBreakpoints(LoadedBreakpoints.None);

            varsView.EndEdit();
            emu.Step();
            emu.RefreshScreen();

            var bp = disassemblyBreakpoints.Where(bp => bp.Address == emu.Registers.PC).FirstOrDefault();

            if (bp != null)
            {
                currentBp = bp;
                var disLine = bp.Tag as ZXCodeLine;
                if (disLine != null)
                {
                    var edit = await OpenFile(disLine.File) as ZXTextEditor;
                    if (edit != null)
                    {
                        if (string.IsNullOrWhiteSpace(edit.Text))
                            edit.Text = loadedProgram?.Disassembly?.Content;

                        edit.BreakLine = disLine.LineNumber + 1;
                    }
                }
            }
            else
            {
                bp = romBreakpoints.Where(bp => bp.Address == emu.Registers.PC).FirstOrDefault();
                if (bp != null)
                {
                    currentBp = bp;
                    var disLine = bp.Tag as ZXCodeLine;
                    if (disLine != null)
                    {
                        var edit = await OpenFile(disLine.File) as ZXTextEditor;
                        if (edit != null)
                        {
                            if (string.IsNullOrWhiteSpace(edit.Text))
                                edit.Text = emu.ModelDefinition?.RomDissasembly;

                            edit.BreakLine = disLine.LineNumber + 1;
                        }
                    }
                }
            }

            regView.Update();
            flagsView.Update(emu.Registers.F);
            varsView.BeginEdit();
            statesView.Update(emu.TStates);
        }
        private void Emu_ProgramReady(object? sender, EventArgs e)
        {
            emu.Pause();

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                EmulatorInfo.IsDebugging = loadedProgram?.Debug ?? false;
                EmulatorInfo.IsRunning = true;
                EmulatorInfo.CanPause = true;
                EmulatorInfo.IsPaused = true;
                EmulatorInfo.CanResume = false;
                EmulatorInfo.CanStep = false;

                if (ZXOptions.Current.Cls)
                {
                    byte[] mem = new byte[0x1800];
                    emu.Memory.SetContents(0x4000, mem);
                }

                if (EmulatorInfo.IsDebugging && loadedProgram?.Variables != null)
                    varsView.Initialize(loadedProgram.Variables, emu.Memory, emu.Registers);

                LoadBreakpoints(LoadedBreakpoints.User);

                regView.Registers = emu.Registers;

                EmulatorInfo.IsPaused = false;
                emu.Resume();
            });
        }
        private void Emu_Breakpoint(object? sender, BreakpointEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                emu.TurboEnabled = false;
                emu.RefreshScreen();

                ClearBreakLines();

                var line = e.Breakpoint.Tag as ZXCodeLine;

                if (line == null)
                    return;

                await ShowBreakLines(e.Breakpoint.Address, line.FileType == ZXFileType.Basic);

                EmulatorInfo.CanResume = true;
                EmulatorInfo.CanStep = true;
                EmulatorInfo.CanPause = false;
                EmulatorInfo.IsPaused = true;

                currentBp = e.Breakpoint;
                varsView.BeginEdit();
                regView.Update();
                flagsView.Update(emu.Registers.F);
                statesView.Update(emu.TStates);
                outLog.Writer.WriteLine($"Breakpoint: file {Path.GetFileName(line.File)}, line {line.LineNumber + 1}, address {line.Address}");

            });
        }
        private void BreakpointManager_BreakPointRemoved(object? sender, System.EventArgs e)
        {
            if (!EmulatorInfo.IsDebugging)
                return;

            UpdateUserBreakpoints();
        }
        private void BreakpointManager_BreakPointAdded(object? sender, System.EventArgs e)
        {
            if (!EmulatorInfo.IsDebugging)
                return;

            UpdateUserBreakpoints();
        }
        private void UpdateUserBreakpoints()
        {
            userBreakpoints.Clear();

            foreach (var userBp in BreakpointManager.AllBreakPoints)
            {
                ZXCodeLine? line;

                if (userBp.File == ZXConstants.DISASSEMBLY_DOC)
                    line = loadedProgram?.DisassemblyMap?.Lines.Where(l => l.LineNumber == userBp.Line - 1).FirstOrDefault();
                else if (userBp.File == ZXConstants.ROM_DOC)
                    line = romLines.Where(l => l.LineNumber == userBp.Line - 1).FirstOrDefault();
                else
                    line = loadedProgram?.ProgramMap?.Lines
                        .Where(l => l.LineNumber == userBp.Line - 1 && l.File.ToLower() == userBp.File.ToLower())
                        .FirstOrDefault();

                if (line == null)
                    continue;

                var bp = new Breakpoint { Id = ZXConstants.USER_BREAKPOINT, Tag = line, Address = line.Address };
                userBreakpoints.Add(bp);
            }

            if (EmulatorInfo.LoadedBreakpoints == LoadedBreakpoints.User)
                LoadBreakpoints(LoadedBreakpoints.User);
        }
        void LoadBreakpoints(LoadedBreakpoints BreakType)
        {
            if (EmulatorInfo.IsDebugging)
            {
                if (EmulatorInfo.IsRunning && !EmulatorInfo.IsPaused)
                    emu.Pause();

                EmulatorInfo.LoadedBreakpoints = BreakType;

                switch (BreakType)
                {
                    case LoadedBreakpoints.User:
                        emu.UpdateBreakpoints(userBreakpoints);
                        break;
                    case LoadedBreakpoints.Basic:
                        emu.UpdateBreakpoints(basicBreakpoints);
                        break;
                    case LoadedBreakpoints.ASM:
                        emu.UpdateBreakpoints(disassemblyBreakpoints);
                        break;
                    default:
                        emu.UpdateBreakpoints(null);
                        break;

                }

                if (EmulatorInfo.IsRunning && !EmulatorInfo.IsPaused)
                {
                    varsView.EndEdit();
                    emu.Resume();
                }
            }
        }
        private async void CheckSpectrumModel()
        {
            var model = cbModel.SelectedIndex;

            if (model == -1)
                throw new ArgumentException("Unknown spectrum model!");

            ZXSpectrumModel sModel = (ZXSpectrumModel)model;

            if (emu.ModelDefinition?.Model != sModel)
            {
                emu.SetModel(sModel);
                memView.Initialize(emu.Memory);
                CreateRomBreakpoints();
                await CloseDocumentByFile(ZXConstants.DISASSEMBLY_DOC);
                _player.Datacorder = emu.Datacorder;
            }
        }

        private void CreateRomBreakpoints()
        {
            if (emu.ModelDefinition == null)
                throw new ArgumentException("Unknown spectrum model!");

            romLines.Clear();
            romBreakpoints.Clear();

            foreach (var codeLine in emu.ModelDefinition.RomDissasemblyMap)
            {
                Breakpoint bp = new Breakpoint { Address = codeLine.Address };
                var zLine = new ZXCodeLine(ZXFileType.Assembler, ZXConstants.ROM_DOC, codeLine.Line, codeLine.Address);
                bp.Tag = zLine;
                romLines.Add(zLine);
                romBreakpoints.Add(bp);
            }

            var userBps = userBreakpoints.Where(bp => ((ZXCodeLine?)bp.Tag)?.File == ZXConstants.ROM_DOC).ToArray();

            foreach (var bp in userBps)
                userBreakpoints.Remove(bp);
        }

        #endregion

        #region Build actions
        private async void Build(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbcPath) || string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbasmPath))
            {
                await this.ShowError("Missing configuration.", "Paths for ZXBASM and ZXBC are not configured, you need to configure these before building.");
                return;
            }

            if (!SaveAllFiles())
            {
                await this.ShowError("Error saving files.", "One or more of the modified files cannot be saved, try to save them manually and check that none are open in another software.");
                return;
            }
            outDock.Select();
            outLog.Clear();
            Cleanup();
            EmulatorInfo.CanDebug = false;
            EmulatorInfo.CanRun = false;
            BlockEditors();
            _ = Task.Run(() =>
            {
                if (peExplorer.RootPath != null)
                    ZXProjectBuilder.Build(outLog.Writer);

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UnblockEditors();
                    EmulatorInfo.CanDebug = FileInfo.ProjectLoaded;
                    EmulatorInfo.CanRun = FileInfo.ProjectLoaded;
                });
            });
        }
        private async void BuildAndRun(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbcPath) || string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbasmPath))
            {
                await this.ShowError("Missing configuration.", "Paths for ZXBASM and ZXBC are not configured, you need to configure these before building.");
                return;
            }

            if (!SaveAllFiles())
            {
                await this.ShowError("Error saving files.", "One or more of the modified files cannot be saved, try to save them manually and check that none are open in another software.");
                return;
            }
            outDock.Select();
            outLog.Clear();
            Cleanup();
            EmulatorInfo.CanDebug = false;
            EmulatorInfo.CanRun = false;
            CheckSpectrumModel();

            _ = Task.Run(() =>
            {
                var program = peExplorer.RootPath == null ? null : ZXProjectBuilder.Build(outLog.Writer);

                var project = ZXProjectManager.Current;
                var settings = project.GetProjectSettings();

                if (settings.NextMode && program != null)
                {
                    string errorMsg = "";
                    // Next mode
                    var emulatorPath = ZXOptions.Current.NextEmulatorPath;
                    if (string.IsNullOrEmpty(emulatorPath))
                    {
                        errorMsg = "There is no emulator configured for Next. Please configure an emulator from the Tools -> Options menu.";
                    }
                    else
                    {
                        // Cleaning...
                        {
                            outLog.Writer.WriteLine("Cleaning temp files...");
                            var file = Path.Combine(project.ProjectPath, Path.GetFileNameWithoutExtension(settings.MainFile) + ".bin");
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                            file = Path.Combine(project.ProjectPath, "nex.cfg");
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                            file = Path.Combine(project.ProjectPath, "sysvars.inc");
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                            file = Path.Combine(project.ProjectPath, Path.GetFileNameWithoutExtension(settings.MainFile) + ".nex");
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                        }

                        try
                        {
                            var emulatorName = Path.GetFileNameWithoutExtension(emulatorPath);
                            var nextDrive = Path.Combine(project.ProjectPath, "nextdrive");
                            switch (emulatorName.ToLower())
                            {
                                case "cspect":
                                    {
                                        outLog.Writer.WriteLine("Launching CSpect...");
                                        Process process = new Process();
                                        process.StartInfo.FileName = emulatorPath;
                                        process.StartInfo.Arguments = string.Format(
                                            "-zxnext -tv -w3 -brk -r -mmc=\"{0}\" \"{1}\"",
                                                nextDrive,
                                                Path.Combine(nextDrive, Path.GetFileNameWithoutExtension(settings.MainFile) + ".nex"));
                                        process.StartInfo.WorkingDirectory = project.ProjectPath;
                                        process.StartInfo.UseShellExecute = true;
                                        process.StartInfo.CreateNoWindow = false;
                                        outLog.Writer.WriteLine(process.StartInfo.FileName+" "+process.StartInfo.Arguments);
                                        process.Start();
                                        process.WaitForExit();
                                    }
                                    break;
                                case "zesarux":
                                    {
                                        outLog.Writer.WriteLine("Launching ZEsarUX...");
                                        Process process = new Process();
                                        process.StartInfo.FileName = emulatorPath;
                                        process.StartInfo.Arguments = string.Format(
                                            "--noconfigfile --zoom 1  --machine TBBlue --realvideo --enabletimexvideo --tbblue-fast-boot-mode --enable-esxdos-handler --esxdos-root-dir \"{0}\" \"{1}\" --snap-no-change-machine",
                                                nextDrive,
                                                Path.Combine(nextDrive, Path.GetFileNameWithoutExtension(settings.MainFile) + ".nex"));
                                        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(emulatorPath);
                                        process.StartInfo.UseShellExecute = true;
                                        process.StartInfo.CreateNoWindow = false;
                                        outLog.Writer.WriteLine(process.StartInfo.FileName + " " + process.StartInfo.Arguments);
                                        process.Start();
                                        process.WaitForExit();
                                    }
                                    break;
                                default:
                                    errorMsg = "There is no valid emulator configured for Next. Please configure an emulator (CSpect or ZEsarUX) from the Tools -> Options menu.";
                                    break;
                            }
                        } catch(Exception ex)
                        {
                            errorMsg = "Error executing emulator";
                        }                        
                    }

                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            await this.ShowError("Error", errorMsg);
                        }
                        UnblockEditors();
                        EmulatorInfo.CanDebug = FileInfo.ProjectLoaded;
                        EmulatorInfo.CanRun = FileInfo.ProjectLoaded;
                    });
                }
                else if (program != null)
                {
                    // No Next mode :)
                    loadedProgram = program;
                    var disas = openDocuments.FirstOrDefault(e => e.DocumentPath == ZXConstants.DISASSEMBLY_DOC) as ZXTextEditor;

                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        if (disas != null)
                        {
                            if (loadedProgram.Disassembly == null)
                            {
                                var parent = disas.Parent as TabItem;
                                if (parent != null)
                                {
                                    parent.IsSelected = true;
                                    CloseFile(null, e);
                                }
                            }
                            else
                                disas.Text = loadedProgram.Disassembly.Content;
                        }

                        if (!emu.InjectProgram(program.Org, program.Binary, program.Banks?.ToArray(), true))
                        {
                            await this.ShowError("Error", "Cannot inject program! Check program size and address.");
                        }
                        else
                        {
                            emuDock.Select();
                            emu.Focus();
                        }
                        EmulatorInfo.CanDebug = FileInfo.ProjectLoaded;
                        EmulatorInfo.CanRun = FileInfo.ProjectLoaded;
                    });
                }
                else
                {
                    // Compiler error
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        UnblockEditors();
                        EmulatorInfo.CanDebug = FileInfo.ProjectLoaded;
                        EmulatorInfo.CanRun = FileInfo.ProjectLoaded;
                    });
                }
            });
        }
        private async void BuildAndDebug(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbcPath) || string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbasmPath))
            {
                await this.ShowError("Missing configuration.", "Paths for ZXBASM and ZXBC are not configured, you need to configure these before building.");
                return;
            }

            if (!SaveAllFiles())
            {
                await this.ShowError("Error saving files.", "One or more of the modified files cannot be saved, try to save them manually and check that none are open in another software.");
                return;
            }
            outDock.Select();
            outLog.Clear();
            Cleanup();
            BlockEditors();
            EmulatorInfo.CanDebug = false;
            EmulatorInfo.CanRun = false;
            CheckSpectrumModel();

            _ = Task.Run(() =>
            {
                var program = peExplorer.RootPath == null ? null : ZXProjectBuilder.BuildDebug(outLog.Writer);

                if (program != null)
                {
                    basicBreakpoints.Clear();
                    if (program.ProgramMap != null)
                        basicBreakpoints.AddRange(program.ProgramMap.Lines.Select(l => new Breakpoint { Address = l.Address, Temporary = false, Id = ZXConstants.BASIC_BREAKPOINT, Tag = l }).ToList());

                    disassemblyBreakpoints.Clear();
                    if (program.DisassemblyMap != null)
                        disassemblyBreakpoints.AddRange(program.DisassemblyMap.Lines.Select(l => new Breakpoint { Address = l.Address, Temporary = false, Id = ZXConstants.ASSEMBLER_BREAKPOINT, Tag = l }).ToList());

                    loadedProgram = program;

                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var currentTab = editTabs.FirstOrDefault(e => e.IsSelected);

                        var disas = await OpenFile(ZXConstants.DISASSEMBLY_DOC) as ZXTextEditor;

                        if (disas != null)
                        {

                            var oldText = disas.Text;

                            if (disas.Text != loadedProgram?.Disassembly?.Content)
                            {
                                disas.Text = loadedProgram?.Disassembly?.Content;
                                disas.InvalidateArrange();
                                disas.InvalidateMeasure();
                                disas.InvalidateVisual();
                            }
                        }

                        var rom = await OpenFile(ZXConstants.ROM_DOC) as ZXTextEditor;

                        if (rom != null)
                        {
                            if (rom.Text != emu.ModelDefinition?.RomDissasembly)
                            {
                                rom.Text = emu.ModelDefinition?.RomDissasembly;
                                rom.InvalidateArrange();
                                rom.InvalidateMeasure();
                                rom.InvalidateVisual();
                            }
                        }

                        if (currentTab != null)
                            currentTab.IsSelected = true;

                        UpdateUserBreakpoints();

                        if (!emu.InjectProgram(program.Org, program.Binary, program.Banks?.ToArray(), true))
                        {
                            await this.ShowError("Error", "Cannot inject program! Check program size and address.");
                        }
                        else
                        {
                            emuDock.Select();
                            emu.Focus();
                        }
                        EmulatorInfo.CanDebug = FileInfo.ProjectLoaded;
                        EmulatorInfo.CanRun = FileInfo.ProjectLoaded;
                    });
                }
                else
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        UnblockEditors();
                        EmulatorInfo.CanDebug = FileInfo.ProjectLoaded;
                        EmulatorInfo.CanRun = FileInfo.ProjectLoaded;
                    });
            });
        }
        private async void Export(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

            if (ZXProjectManager.Current == null)
            {
                await this.ShowError("Cannot export", "No project has been open, cannot export.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbcPath) || string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbasmPath))
            {
                await this.ShowError("Missing configuration.", "Paths for ZXBASM and ZXBC are not configured, you need to configure these before building.");
                return;
            }

            ZXExportOptions? opts = ZXProjectManager.Current.GetExportOptions();

            var dlg = new ZXExportDialog();
            dlg.ExportOptions = opts;
            var res = await dlg.ShowDialog<bool>(this);

            if (!res || dlg.ExportOptions == null)
                return;

            opts = dlg.ExportOptions;

            if (!SaveAllFiles())
            {
                await this.ShowError("Error saving files.", "One or more of the modified files cannot be saved, try to save them manually and check that none are open in another software.");
                return;
            }

            ZXProjectManager.Current.SaveExportOptions(opts);

            string file = opts.OutputPath;

            outDock.Select();
            outLog.Clear();
            Cleanup();
            BlockEditors();
            EmulatorInfo.CanDebug = false;
            EmulatorInfo.CanRun = false;
            _ = Task.Run(() =>
            {
                if (peExplorer.RootPath != null)
                    ZXProjectBuilder.Export(opts, outLog.Writer);

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UnblockEditors();
                    EmulatorInfo.CanDebug = FileInfo.ProjectLoaded;
                    EmulatorInfo.CanRun = FileInfo.ProjectLoaded;
                });
            });

        }
        #endregion

        #region Project actions
        private async void ConfigureProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

            if (ZXProjectManager.Current == null)
            {
                await this.ShowError("No project", $"No project has been loaded, aborting.");
                return;
            }

            var dlg = new ZXBuildSettingsDialog();
            dlg.Settings = ZXProjectManager.Current.GetProjectSettings();

            if (await dlg.ShowDialog<bool>(this))
            {
                if (!ZXProjectManager.Current.SaveProjectSettings(dlg.Settings))
                {
                    await this.ShowError("Error saving file", $"Unexpected error trying to save the configuration file.");
                }
            }
        }
        #endregion

        #region Editor control
        private void TcEditors_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var activated = e.AddedItems.OfType<TabItem>().FirstOrDefault();
            var deactivated = e.RemovedItems.OfType<TabItem>().FirstOrDefault();

            if (deactivated != null)
            {
                var editor = deactivated.Content as ZXDocumentEditorBase;

                if (editor != null)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        Dispatcher.UIThread.Invoke(() => editor.Deactivated());
                    });
                }

            }

            if (activated != null)
            {
                var editor = activated.Content as ZXDocumentEditorBase;

                if (editor != null)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        Dispatcher.UIThread.Invoke(() => editor.Activated());
                    });
                }
            }
        }

        private void BtnRemoveBreakpoints_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var textEditors = openDocuments.Where(doc => doc is ZXTextEditor).Cast<ZXTextEditor>();

            foreach (var editor in textEditors)
                editor.ClearBreakpoints();

            BreakpointManager.ClearBreakpoints();
            UpdateUserBreakpoints();
        }

        private void BtnFontIncrease_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            foreach (var tab in editTabs)
            {
                if (tab.IsSelected)
                {
                    var editor = tab.Content as ZXTextEditor;
                    if (editor != null)
                    {
                        editor.FontIncrease();
                        Task.Run(async () =>
                        {
                            await Task.Delay(100);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                editor.Focus();
                                editor.FocusText();
                            });

                        });
                    }
                }
            }
        }

        private void BtnFontDecrease_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            foreach (var tab in editTabs)
            {
                if (tab.IsSelected)
                {
                    var editor = tab.Content as ZXTextEditor;
                    if (editor != null)
                    {
                        editor.FontDecrease();
                        Task.Run(async () =>
                        {
                            await Task.Delay(100);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                editor.Focus();
                                editor.FocusText();
                            });

                        });
                    }
                }
            }
        }

        private void BtnUncomment_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            foreach (var tab in editTabs)
            {
                if (tab.IsSelected)
                {
                    var editor = tab.Content as ZXTextEditor;
                    if (editor != null)
                    {
                        editor.UncommentSelection();
                        Task.Run(async () =>
                        {
                            await Task.Delay(100);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                editor.Focus();
                                editor.FocusText();
                            });

                        });
                    }
                }
            }
        }

        private void BtnComment_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            foreach (var tab in editTabs)
            {
                if (tab.IsSelected)
                {
                    var editor = tab.Content as ZXTextEditor;
                    if (editor != null)
                    {
                        editor.CommentSelection();
                        Task.Run(async () =>
                        {
                            await Task.Delay(100);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                editor.Focus();
                                editor.FocusText();
                            });

                        });
                    }
                }
            }
        }

        private void BtnExpand_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            foreach (var tab in editTabs)
            {
                if (tab.IsSelected)
                {
                    var editor = tab.Content as ZXTextEditor;
                    if (editor != null)
                    {
                        editor.Expand();
                        Task.Run(async () =>
                        {
                            await Task.Delay(100);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                editor.Focus();
                                editor.FocusText();
                            });

                        });
                    }
                    return;
                }
            }
        }

        private void BtnCollapse_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            foreach (var tab in editTabs)
            {
                if (tab.IsSelected)
                {
                    var editor = tab.Content as ZXTextEditor;
                    if (editor != null)
                    {
                        editor.Collapse();
                        Task.Run(async () =>
                        {
                            await Task.Delay(100);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                editor.Focus();
                                editor.FocusText();
                            });

                        });

                    }
                    return;
                }
            }
        }

        #endregion

        #region Global actions
        private async void ConfigureGlobalSettings(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dlg = new ZXOptionsDialog();
            await dlg.ShowDialog(this);
            emu.AntiAlias = ZXOptions.Current.AntiAlias;
        }

        private async void RestoreLayout(object? sender, RoutedEventArgs e)
        {
            if (!(await this.ShowConfirm("Restore layout", "Are you sure you want to restore the layout to its initial configuration?")))
                return;

            ZXLayoutPersister.ResetLayout();
            skipLayout = true;

            await this.ShowInfo("Restore layout", "Layout has been reset, restart the application to apply the changes.");
        }

        private void PlayLayout(object? sender, RoutedEventArgs e)
        {
            grdMain.RowDefinitions = RowDefinitions.Parse("Auto,Auto,2*,4,*");
            grdMain.ColumnDefinitions = ColumnDefinitions.Parse("0*,4,2*,4,0*");
        }

        private void DebugLayout(object? sender, RoutedEventArgs e)
        {
            grdMain.RowDefinitions = RowDefinitions.Parse("Auto,Auto,2*,4,*");
            grdMain.ColumnDefinitions = ColumnDefinitions.Parse("0*,4,2.6*,4,*");
        }

        private void ToolsLayout(object? sender, RoutedEventArgs e)
        {
            grdMain.RowDefinitions = RowDefinitions.Parse("Auto,Auto,2*,4,*");
            grdMain.ColumnDefinitions = ColumnDefinitions.Parse("0.6*,4,2*,4,*");
        }

        private void ExplorerLayout(object? sender, RoutedEventArgs e)
        {
            grdMain.RowDefinitions = RowDefinitions.Parse("Auto,Auto,*,4,0*");
            grdMain.ColumnDefinitions = ColumnDefinitions.Parse("0.6*,4,3*,4,0*");
        }

        private void FullLayout(object? sender, RoutedEventArgs e)
        {
            grdMain.RowDefinitions = RowDefinitions.Parse("Auto,Auto,*,4,0*");
            grdMain.ColumnDefinitions = ColumnDefinitions.Parse("0*,4,*,4,0*");
        }


        #endregion

        #region General functions
        private void BtnMapKeyboard_Click(object? sender, RoutedEventArgs e)
        {
            emu.EnableKeyMapping = btnMapKeyboard.IsChecked ?? false;
        }

        private void OpenUrl(string Url)
        {

            if (string.IsNullOrWhiteSpace(Url))
                return;

            try
            {
                ProcessStartInfo processInfo = new()
                {
                    FileName = Url,
                    UseShellExecute = true
                };

                Process.Start(processInfo);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", Url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", Url);
                }
                else
                {
                    return;
                }
            }
        }

        private async void DumpRegisters(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var select = await StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Select output file",
                DefaultExtension = ".json",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Json file (*.json)") { Patterns = new[] { "*.json" } },
                    new FilePickerFileType("CSV file (*.csv)") { Patterns = new[] { "*.csv" } },
                }
            });

            if (select == null)
                return;

            string file = Path.GetFullPath(select.Path.LocalPath);
            string ext = Path.GetExtension(file).ToLower();

            if (ext != ".json" && ext != ".csv")
            {
                await this.ShowError("Invalid file", "Select a .json or .csv file.");
                return;
            }

            if (ext == ".json")
            {
                var regs = new
                {
                    A = emu.Registers.A.ToString("X2"),
                    F = emu.Registers.F.ToString("X2"),
                    B = emu.Registers.B.ToString("X2"),
                    C = emu.Registers.C.ToString("X2"),
                    D = emu.Registers.D.ToString("X2"),
                    E = emu.Registers.E.ToString("X2"),
                    H = emu.Registers.H.ToString("X2"),
                    L = emu.Registers.L.ToString("X2"),
                    IXH = emu.Registers.IXH.ToString("X2"),
                    IXL = emu.Registers.IXL.ToString("X2"),
                    IYH = emu.Registers.IYH.ToString("X2"),
                    IYL = emu.Registers.IYL.ToString("X2"),
                    AF = ((ushort)emu.Registers.AF).ToString("X4"),
                    BC = ((ushort)emu.Registers.BC).ToString("X4"),
                    DE = ((ushort)emu.Registers.DE).ToString("X4"),
                    HL = ((ushort)emu.Registers.HL).ToString("X4"),
                    IX = ((ushort)emu.Registers.IX).ToString("X4"),
                    IY = ((ushort)emu.Registers.IY).ToString("X4"),
                    PC = ((ushort)emu.Registers.PC).ToString("X4"),
                    SP = ((ushort)emu.Registers.SP).ToString("X4"),

                    Ap = emu.Registers.Alternate.A.ToString("X2"),
                    Fp = emu.Registers.Alternate.F.ToString("X2"),
                    Bp = emu.Registers.Alternate.B.ToString("X2"),
                    Cp = emu.Registers.Alternate.C.ToString("X2"),
                    Dp = emu.Registers.Alternate.D.ToString("X2"),
                    Ep = emu.Registers.Alternate.E.ToString("X2"),
                    Hp = emu.Registers.Alternate.H.ToString("X2"),
                    Lp = emu.Registers.Alternate.L.ToString("X2"),
                    AFp = ((ushort)emu.Registers.Alternate.AF).ToString("X4"),
                    BCp = ((ushort)emu.Registers.Alternate.BC).ToString("X4"),
                    DEp = ((ushort)emu.Registers.Alternate.DE).ToString("X4"),
                    HLp = ((ushort)emu.Registers.Alternate.HL).ToString("X4"),
                };

                try
                {
                    File.WriteAllText(file, JsonConvert.SerializeObject(regs, Formatting.Indented));
                }
                catch (Exception ex)
                {
                    await this.ShowError("Error saving file", $"Unexpected error trying to save the register dump file: {ex.Message} - {ex.StackTrace}");
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Register, Value");
                sb.AppendLine($"A, {emu.Registers.A.ToString("X2")}");
                sb.AppendLine($"F, {emu.Registers.F.ToString("X2")}");
                sb.AppendLine($"B, {emu.Registers.B.ToString("X2")}");
                sb.AppendLine($"C, {emu.Registers.C.ToString("X2")}");
                sb.AppendLine($"D, {emu.Registers.D.ToString("X2")}");
                sb.AppendLine($"E, {emu.Registers.E.ToString("X2")}");
                sb.AppendLine($"H, {emu.Registers.H.ToString("X2")}");
                sb.AppendLine($"L, {emu.Registers.L.ToString("X2")}");
                sb.AppendLine($"IXH, {emu.Registers.IXH.ToString("X2")}");
                sb.AppendLine($"IXL, {emu.Registers.IXL.ToString("X2")}");
                sb.AppendLine($"IYH, {emu.Registers.IYH.ToString("X2")}");
                sb.AppendLine($"IYL, {emu.Registers.IYL.ToString("X2")}");
                sb.AppendLine($"AF, {((ushort)emu.Registers.AF).ToString("X4")}");
                sb.AppendLine($"BC, {((ushort)emu.Registers.BC).ToString("X4")}");
                sb.AppendLine($"DE, {((ushort)emu.Registers.DE).ToString("X4")}");
                sb.AppendLine($"HL, {((ushort)emu.Registers.HL).ToString("X4")}");
                sb.AppendLine($"IX, {((ushort)emu.Registers.IX).ToString("X4")}");
                sb.AppendLine($"IY, {((ushort)emu.Registers.IY).ToString("X4")}");
                sb.AppendLine($"PC, {((ushort)emu.Registers.PC).ToString("X4")}");
                sb.AppendLine($"SP, {((ushort)emu.Registers.SP).ToString("X4")}");
                sb.AppendLine($"Ap, {emu.Registers.Alternate.A.ToString("X2")}");
                sb.AppendLine($"Fp, {emu.Registers.Alternate.F.ToString("X2")}");
                sb.AppendLine($"Bp, {emu.Registers.Alternate.B.ToString("X2")}");
                sb.AppendLine($"Cp, {emu.Registers.Alternate.C.ToString("X2")}");
                sb.AppendLine($"Dp, {emu.Registers.Alternate.D.ToString("X2")}");
                sb.AppendLine($"Ep, {emu.Registers.Alternate.E.ToString("X2")}");
                sb.AppendLine($"Hp, {emu.Registers.Alternate.H.ToString("X2")}");
                sb.AppendLine($"Lp, {emu.Registers.Alternate.L.ToString("X2")}");
                sb.AppendLine($"AFp, {((ushort)emu.Registers.Alternate.AF).ToString("X4")}");
                sb.AppendLine($"BCp, {((ushort)emu.Registers.Alternate.BC).ToString("X4")}");
                sb.AppendLine($"DEp, {((ushort)emu.Registers.Alternate.DE).ToString("X4")}");
                sb.AppendLine($"HLp, {((ushort)emu.Registers.Alternate.HL).ToString("X4")}");

                try
                {
                    File.WriteAllText(file, sb.ToString());
                }
                catch (Exception ex)
                {
                    await this.ShowError("Error saving file", $"Unexpected error trying to save the regsiter dump file: {ex.Message} - {ex.StackTrace}");

                }
            }
        }
        private async void DumpMemory(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dlg = new ZXDumpMemoryDialog();
            dlg.Initialize(emu.Memory);
            await dlg.ShowDialog(this);
        }
        void Cleanup()
        {
            emu.TurboEnabled = false;
            emu.Stop();
            loadedProgram = null;
            basicBreakpoints.Clear();
            disassemblyBreakpoints.Clear();
            userBreakpoints.Clear();

            varsView.EndEdit();
            statesView.Clear();
            flagsView.Clear();
            currentBp = null;
            emu.UpdateBreakpoints(null);
            EmulatorInfo.IsRunning = false;
            EmulatorInfo.IsPaused = false;
            EmulatorInfo.IsDebugging = false;
            EmulatorInfo.CanPause = false;
            EmulatorInfo.CanResume = false;
            EmulatorInfo.CanStep = false;
            ClearBreakLines();

        }
        private async Task<Breakpoint?> ShowBreakLines(ushort PC, bool PreferBasic)
        {
            Breakpoint? returnbp = null;

            if (PreferBasic)
            {
                var rombp = romBreakpoints.LastOrDefault(b => b.Address == PC);

                if (rombp != null)
                {
                    returnbp = rombp;
                    await HighlightBreakpoint(rombp);
                }

                var disbp = disassemblyBreakpoints.LastOrDefault(b => b.Address == PC);

                if (disbp != null)
                {
                    returnbp = disbp;
                    await HighlightBreakpoint(disbp);
                }
                var basbp = basicBreakpoints.LastOrDefault(b => b.Address == PC);

                if (basbp != null)
                {
                    returnbp = basbp;
                    await HighlightBreakpoint(basbp);
                }
            }
            else
            {
                var basbp = basicBreakpoints.LastOrDefault(b => b.Address == PC);

                if (basbp != null)
                {
                    returnbp = basbp;
                    await HighlightBreakpoint(basbp);
                }

                var rombp = romBreakpoints.LastOrDefault(b => b.Address == PC);

                if (rombp != null)
                {
                    returnbp = rombp;
                    await HighlightBreakpoint(rombp);
                }

                var disbp = disassemblyBreakpoints.LastOrDefault(b => b.Address == PC);

                if (disbp != null)
                {
                    returnbp = disbp;
                    await HighlightBreakpoint(disbp);
                }
            }

            return returnbp;
        }
        private async Task HighlightBreakpoint(Breakpoint bp)
        {
            var line = bp.Tag as ZXCodeLine;

            if (line != null)
            {
                var edit = await OpenFile(line.File) as ZXTextEditor;
                if (edit != null)
                    edit.BreakLine = line.LineNumber + 1;
            }
        }
        private void ClearBreakLines()
        {
            var textEditors = openDocuments.Where(doc => doc is ZXTextEditor).Cast<ZXTextEditor>();

            foreach (var edit in textEditors)
                edit.BreakLine = null;
        }
        private void BlockEditors()
        {
            var textEditors = openDocuments.Where(doc => doc is ZXTextEditor).Cast<ZXTextEditor>();

            foreach (var edit in textEditors)
                edit.Readonly = true;
        }
        private void UnblockEditors()
        {
            var textEditors = openDocuments.Where(doc => doc is ZXTextEditor).Cast<ZXTextEditor>();

            foreach (var edit in textEditors)
            {
                if (edit.DocumentPath != ZXConstants.DISASSEMBLY_DOC && edit.DocumentPath != ZXConstants.ROM_DOC)
                    edit.Readonly = false;
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (openDocuments.Any(e => e.Modified) && !skipCloseCheck && e.CloseReason != WindowCloseReason.OSShutdown)
            {
                if (openDocuments.Any(e => e.Modified))
                {
                    e.Cancel = true;
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var resConfirm = await this.ShowConfirm("Modified documents", "Some documents have been modified but not saved, if you close the project all the changes will be lost, are you sure you want to close the project?");

                        if (resConfirm)
                        {
                            foreach (var doc in openDocuments)
                                doc.CloseDocument(outLog.Writer, true);

                            ZXProjectManager.CloseProject();

                            skipCloseCheck = true;
                            Close();
                        }
                    });
                }
            }

            emu.Stop();

            if (!skipLayout)
                ZXLayoutPersister.SaveLayout(grdMain, dockLeft, dockRight, dockBottom);

            base.OnClosing(e);

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            ZXFloatController.Dispose();
        }

        #endregion

        #region Global keyb handling

        private void GlobalKeyUp(object? sender, KeyEventArgs e)
        {
            var cmd = ZXKeybMapper.GetCommandId(KeybSourceId, e.Key, e.KeyModifiers);

            if (cmd != null && _shortcuts.ContainsKey(cmd.Value))
                _shortcuts[cmd.Value]();
        }

        #endregion

        #region Application control

        private async void ExitApplication(object? sender, Avalonia.Interactivity.RoutedEventArgs? e)
        {
            Close();
        }

        #endregion
    }

    public enum PreferredSourceType
    {
        Basic,
        Disassembly,
        ROM
    }

    public partial class FileInfoProvider : ObservableObject
    {
        [ObservableProperty]
        bool projectLoaded;
        [ObservableProperty]
        bool fileLoaded;
        [ObservableProperty]
        bool fileSystemObjectSelected;

    }
    public partial class EmulatorInfoProvider : ObservableObject
    {
        [ObservableProperty]
        bool isRunning;
        [ObservableProperty]
        bool isPaused;
        [ObservableProperty]
        bool isDebugging;
        [ObservableProperty]
        bool canPause;
        [ObservableProperty]
        bool canResume;
        [ObservableProperty]
        bool canStep;
        [ObservableProperty]
        bool canRun;
        [ObservableProperty]
        bool canDebug;
        public LoadedBreakpoints LoadedBreakpoints { get; set; }
    }

    public enum LoadedBreakpoints
    {
        None,
        ASM,
        Basic,
        User
    }

    class RomLine
    {
        public int Line { get; set; }
        public ushort Address { get; set; }
    }
}
