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
using CoreSpectrum.Debug;
using CoreSpectrum.Enums;
using HarfBuzzSharp;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
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
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Controls;
using ZXBasicStudio.Controls.DockSystem;
using ZXBasicStudio.Dialogs;

namespace ZXBasicStudio
{
    public partial class MainWindow : ZXWindowBase, IObserver<RawInputEventArgs>
    {
        //TODO: Añadir lista de proyectos recientes al menú

        List<ZXTextEditor> openEditors = new List<ZXTextEditor>();
        ObservableCollection<TabItem> editTabs = new ObservableCollection<TabItem>();

        ZXProgram? loadedProgram;
        List<Breakpoint> basicBreakpoints = new List<Breakpoint>();
        List<Breakpoint> disassemblyBreakpoints = new List<Breakpoint>();
        List<Breakpoint> romBreakpoints = new List<Breakpoint>();
        List<Breakpoint> userBreakpoints = new List<Breakpoint>();
        List<ZXCodeLine> romLines = new List<ZXCodeLine>();
        Breakpoint? currentBp;
        string romDisassembly;
        public ObservableCollection<TabItem> EditItems { get; set; }

        public FileInfoProvider FileInfo { get; set; } = new FileInfoProvider();
        public EmulatorInfoProvider EmulatorInfo { get; set; } = new EmulatorInfoProvider();

        protected override bool PersistBounds { get { return true; } }

        private object? rootContent = null;

        private bool skipLayout;

        public MainWindow()
        {
            InitializeComponent();
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
            mnuCreateProject.Click += CreateProject;
            mnuCreateFolder.Click += CreateFolder;
            mnuCreateFile.Click += CreateFile;
            mnuSaveFile.Click += SaveFile;
            mnuCloseProject.Click += CloseProject;
            mnuCloseFile.Click += CloseFile;
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
            btnCollapse.Click += BtnCollapse_Click;
            btnExpand.Click += BtnExpand_Click;
            btnComment.Click += BtnComment_Click;
            btnUncomment.Click += BtnUncomment_Click;
            btnRemoveBreakpoints.Click += BtnRemoveBreakpoints_Click;
            btnTurbo.Click += TurboModeEmulator;
            btnBorderless.Click += Borderless;
            btnDirectScreen.Click += DirectScreen;
            btnFullLayout.Click += FullLayout;
            btnExplorerLayout.Click += ExplorerLayout;
            btnToolsLayout.Click += ToolsLayout;
            btnDebugLayout.Click += DebugLayout;
            btnPlayLayout.Click += PlayLayout;
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

            InputManager.Instance.PreProcess.Subscribe(this);

            regView.Registers = emu.Registers;
            memView.Initialize(emu.Memory);
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager("ZXBasicStudio.Resources.ZXSpectrum", typeof(ZXEmulator).Assembly);

            romDisassembly = resources.GetObject("romDisassembly") as string;
            string romMapB = resources.GetObject("romDisassemblyMap") as string;

            var romMap = JsonConvert.DeserializeObject<RomLine[]>(romMapB);

            foreach (var codeLine in romMap)
            {
                Breakpoint bp = new Breakpoint { Address = codeLine.Address };
                var zLine = new ZXCodeLine(ZXFileType.Assembler, ZXConstants.ROM_DOC, codeLine.Line, codeLine.Address);
                bp.Tag = zLine;
                romLines.Add(zLine);
                romBreakpoints.Add(bp);
            }

            ZXLayoutPersister.RestoreLayout(grdMain, dockLeft, dockRight, dockBottom);
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
                confirm = await ShowConfirm("Delete file", $"Are you sure you want to delete the file \"{Path.GetFileName(path)}\"?");
            else
                confirm = await ShowConfirm("Delete folder", $"Are you sure you want to delete the folder \"{Path.GetFileName(path)}\" and all its content?");

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
                await ShowError("Delete error", $"Unexpected error trying to delete the {(isFile ? "file" : "directory")}: {ex.Message} - {ex.StackTrace}");
            }
        }

        private async void Rename(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var path = peExplorer.SelectedPath;
            if (path == null)
                return;

            bool isFile = File.Exists(path);

            if (!isFile && openEditors.Any(e => e.FileName.ToLower().StartsWith(path.ToLower())))
            {
                await ShowError("Open documents", "There are open documents in the selected folder, close any document in the folder before renaming it.");
                return;
            }

            string? newName = null;
            if (isFile)
                newName = await ShowInput("Rename file", "Select the new name for the file", "File name", Path.GetFileName(path));
            else
                newName = await ShowInput("Rename folder", "Select the new name for the folder", "Folder name", Path.GetFileName(path));

            if (newName == null)
                return;
            try
            {
                if (isFile)
                    File.Move(path, Path.Combine(Path.GetDirectoryName(path), newName));
                else
                    Directory.Move(path, Path.Combine(Path.GetDirectoryName(path), newName));
            }
            catch (Exception ex)
            {
                await ShowError("Rename error", $"Unexpected error trying to rename the {(isFile ? "file" : "directory")}: {ex.Message} - {ex.StackTrace}");
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

            var editor = tab.Content as ZXTextEditor;

            if (editor == null)
                return;

            if (editor.Modified)
            {
                var res = await ShowConfirm("Modified", "This document has been modified, if you close it now you will lose the changes, are you sure you want to close it?");

                if (!res)
                    return;
            }

            openEditors.Remove(editor);
            editTabs.Remove(tab);

            if (openEditors.Count == 0)
                FileInfo.FileLoaded = false;
        }

        private async void CloseProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (openEditors.Any(e => e.Modified))
            {
                var resConfirm = await ShowConfirm("Modified documents", "Some documents have been modified but not saved, if you close the project all the changes will be lost, are you sure you want to close the project?");

                if (!resConfirm)
                    return;
            }

            openEditors.Clear();
            editTabs.Clear();
            peExplorer.OpenProjectFolder(null);
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

            var editor = activeTab.Content as ZXTextEditor;

            if (editor == null)
                return;

            if (!editor.SaveDocument())
            {
                await ShowError("Error", "Cannot save the file, check if another program is blocking it.");
                return;
            }
        }

        private void SaveAllFiles(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SaveAllFiles();
        }

        private bool SaveAllFiles()
        {
            foreach (var edit in openEditors)
            {
                if (edit.Modified)
                    if (!edit.SaveDocument())
                        return false;
            }

            return true;
        }

        private void OpenFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var path = peExplorer.SelectedPath;

            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!File.Exists(path))
                return;

            OpenFile(path);
        }

        private async void OpenNewFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            var basicFilter = new FileDialogFilter { Name = "ZX Basic file" };
            basicFilter.Extensions.AddRange(ZXExtensions.ZXBasicFiles);
            var asmFilter = new FileDialogFilter { Name = "ZX Assembler file" };
            asmFilter.Extensions.AddRange(ZXExtensions.ZXAssemblerFiles);
            dlg.Filters.Add(basicFilter);
            dlg.Filters.Add(asmFilter);
            dlg.AllowMultiple = false;
            var file = (await dlg.ShowAsync(this))?.FirstOrDefault();

            if (file == null)
                return;

            OpenFile(file);
        }

        private async void OpenProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (FileInfo.ProjectLoaded)
            {
                if (openEditors.Any(e => e.Modified))
                {
                    var resConfirm = await ShowConfirm("Warning!", "There are unsaved documents, opening a new project will discard the changes, are you sure you want to open a new project?");

                    if (!resConfirm)
                        return;
                }
            }

            var openDlg = new OpenFolderDialog();

            var res = await openDlg.ShowAsync(this);

            if (!string.IsNullOrWhiteSpace(res))
            {
                Cleanup();
                BreakpointManager.ClearBreakpoints();
                peExplorer.OpenProjectFolder(res);
                editTabs.Clear();
                openEditors.Clear();
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
                path = peExplorer.RootPath;

            if (File.Exists(path))
                path = Path.GetDirectoryName(path);

            var fileName = await ShowInput("New file", "Enter the name of the file to be created.", "File:");

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var finalPath = Path.Combine(path, fileName);
            try
            {
                File.Create(finalPath).Dispose();
            }
            catch (Exception ex)
            {
                await ShowError("Create error", $"Unexpected error trying to create the file: {ex.Message} - {ex.StackTrace}");
                return;
            }
            OpenFile(finalPath);
            await Task.Delay(100);
            peExplorer.SelectPath(finalPath);
        }

        private async void CreateFolder(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var path = peExplorer.SelectedPath;

            if (path == null)
                path = peExplorer.RootPath;

            if (File.Exists(path))
                path = Path.GetDirectoryName(path);

            var fileName = await ShowInput("New folder", "Enter the name of the folder to be created.", "Folder:");

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var finalPath = Path.Combine(path, fileName);

            try
            {
                Directory.CreateDirectory(finalPath);
            }
            catch (Exception ex)
            {
                await ShowError("Create error", $"Unexpected error trying to create the directory: {ex.Message} - {ex.StackTrace}");
            }
        }

        private async void CreateProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

            if (openEditors.Any(e => e.Modified))
            {
                var resConfirm = await ShowConfirm("Warning!", "Current project has pending changes, creating a new project will discard those changes. Do you want to continue?");

                if (!resConfirm)
                    return;
            }

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

            var settingsFile = Path.Combine(selFolder, ZXConstants.BUILDSETTINGS_FILE);
            var content = JsonConvert.SerializeObject(dlg.Settings);
            try
            {
                File.WriteAllText(settingsFile, content);
            }
            catch (Exception ex)
            {
                await ShowError("Create error", $"Unexpected error trying to create the project settings file: {ex.Message} - {ex.StackTrace}");
                return;
            }
            peExplorer.OpenProjectFolder(selFolder);
            editTabs.Clear();
            openEditors.Clear();
            FileInfo.ProjectLoaded = true;
            FileInfo.FileLoaded = false;
            FileInfo.FileSystemObjectSelected = false;
            EmulatorInfo.CanDebug = true;
            EmulatorInfo.CanRun = true;
        }

        ZXTextEditor? OpenFile(string file)
        {
            try
            {
                var opened = openEditors.FirstOrDefault(ef => Path.GetFullPath(file) == Path.GetFullPath(ef.FileName));

                if (opened != null)
                {
                    var tab = editTabs.First(t => t.Content == opened);
                    tab.IsSelected = true;
                    return opened;
                }

                ZXTextEditor editor = null;
                ZXGraphics.ui.Main graphicsEditor = null;

                if (file.IsZXAssembler() || file == ZXConstants.DISASSEMBLY_DOC || file == ZXConstants.ROM_DOC)
                    editor = new ZXAssemblerEditor(file);
                else if (file.IsZXBasic())
                    editor = new ZXBasicEditor(file);
                else if (file.IsZXConfig())
                    editor = new ZXTextEditor(file);
                else if (file.IsZXGraphics())
                {
                    graphicsEditor = new ZXGraphics.ui.Main();
                    graphicsEditor.Initialize(file);
                }
                else
                    return null;

                if (editor != null)
                {
                    TabItem tItem = new TabItem();
                    tItem.Classes.Add("closeTab");
                    tItem.Tag = Path.GetFileName(file);
                    tItem.Content = editor;
                    editTabs.Add(tItem);
                    openEditors.Add(editor);

                    tItem.IsSelected = true;
                    editor.DocumentModified += EditorDocumentModified;
                    editor.DocumentSaved += EditorDocumentSaved;
                    FileInfo.FileLoaded = true;
                    peExplorer.SelectPath(file);

                    if (EmulatorInfo.IsRunning && !EmulatorInfo.IsPaused)
                        editor.Readonly = true;
                    else if (EmulatorInfo.IsRunning && EmulatorInfo.IsPaused)
                    {
                        var bp = basicBreakpoints.FirstOrDefault(bp => bp.Address == emu.Registers.PC);

                        if (bp != null)
                        {
                            var line = bp.Tag as ZXCodeLine;
                            if (line != null)
                            {
                                if (line.File == file)
                                    editor.BreakLine = line.LineNumber + 1;
                            }
                        }
                    }
                    return editor;
                }
                else if (graphicsEditor != null)
                {
                    TabItem tItem = new TabItem();
                    tItem.Classes.Add("closeTab");
                    tItem.Tag = Path.GetFileName(file);
                    tItem.Content = graphicsEditor;
                    editTabs.Add(tItem);
                    //openEditors.Add(editor);

                    tItem.IsSelected = true;
                    //editor.DocumentModified += EditorDocumentModified;
                    //editor.DocumentSaved += EditorDocumentSaved;
                    FileInfo.FileLoaded = true;
                    peExplorer.SelectPath(file);

                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex) { ShowError("Error loading file.", $"Error loading file {file}: {ex.Message} {ex.StackTrace}").RunSynchronously(); return null; }
        }

        private void EditorDocumentSaved(object? sender, System.EventArgs e)
        {
            var editor = (ZXTextEditor?)sender;
            if (editor == null)
                return;
            var tab = editor.Parent as TabItem;
            if (tab == null)
                return;
            tab.Tag = tab.Tag?.ToString()?.Replace("*", "");
        }

        private void EditorDocumentModified(object? sender, System.EventArgs e)
        {
            var editor = (ZXTextEditor?)sender;
            if (editor == null)
                return;
            var tab = editor.Parent as TabItem;
            if (tab == null)
                return;
            tab.Tag = tab.Tag?.ToString() + "*";
        }
        #endregion

        #region Emulator control
        private void DirectScreen(object? sender, RoutedEventArgs e)
        {
            emu.DirectMode = btnDirectScreen.IsChecked ?? false;
        }

        private void Borderless(object? sender, RoutedEventArgs e)
        {
            emu.Borderless = btnBorderless.IsChecked ?? false;
        }
        private void SwapFullScreen()
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
        }
        private void PauseEmulator(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
                currentBp = ShowBreakLines(emu.Registers.PC, true);
                regView.Update();
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

            foreach (var edit in openEditors)
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
        private void AssemblerStepEmulator(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
                    var edit = OpenFile(disLine.File);
                    if (edit != null)
                    {
                        if (string.IsNullOrWhiteSpace(edit.Text))
                            edit.Text = loadedProgram.Disassembly.Content;

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
                        var edit = OpenFile(disLine.File);
                        if (edit != null)
                        {
                            if (string.IsNullOrWhiteSpace(edit.Text))
                                edit.Text = romDisassembly;

                            edit.BreakLine = disLine.LineNumber + 1;
                        }
                    }
                }
            }

            regView.Update();
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

                if (EmulatorInfo.IsDebugging)
                    varsView.Initialize(loadedProgram.Variables, emu.Memory, emu.Registers);

                LoadBreakpoints(LoadedBreakpoints.User);

                EmulatorInfo.IsPaused = false;
                emu.Resume();
                //varsView.AllowEdit = false;
            });
        }
        private void Emu_Breakpoint(object? sender, BreakpointEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                emu.TurboEnabled = false;
                emu.RefreshScreen();

                ClearBreakLines();

                var line = e.Breakpoint.Tag as ZXCodeLine;

                if (line == null)
                    return;

                ShowBreakLines(e.Breakpoint.Address, line.FileType == ZXFileType.Basic);

                EmulatorInfo.CanResume = true;
                EmulatorInfo.CanStep = true;
                EmulatorInfo.CanPause = false;
                EmulatorInfo.IsPaused = true;

                currentBp = e.Breakpoint;
                varsView.BeginEdit();
                regView.Update();
                statesView.Update(emu.TStates);

                try
                {
                    outLog.Writer.WriteLine($"Breakpoint: file {Path.GetFileName(line.File)}, line {line.LineNumber + 1}, address {line.Address}");
                }
                catch (Exception ex)
                {
                    Console.Write("ERROR!");
                }
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
                    line = loadedProgram?.DisassemblyMap.Lines.Where(l => l.LineNumber == userBp.Line - 1).FirstOrDefault();
                else if (userBp.File == ZXConstants.ROM_DOC)
                    line = romLines.Where(l => l.LineNumber == userBp.Line - 1).FirstOrDefault();
                else
                    line = loadedProgram?.ProgramMap.Lines
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

        #endregion

        #region Build actions
        private async void Build(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbcPath) || string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbasmPath))
            {
                await ShowError("Missing configuration.", "Paths for ZXBASM and ZXBC are not configured, you need to configure these before building.");
                return;
            }

            if (!SaveAllFiles())
            {
                await ShowError("Error saving files.", "One or more of the modified files cannot be saved, try to save them manually and check that none are open in another software.");
                return;
            }
            outDock.Select();
            outLog.Clear();
            Cleanup();
            EmulatorInfo.CanDebug = false;
            EmulatorInfo.CanRun = false;
            BlockEditors();
            Task.Run(() =>
            {
                ZXProjectBuilder.Build(peExplorer.RootPath, outLog.Writer);
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
                await ShowError("Missing configuration.", "Paths for ZXBASM and ZXBC are not configured, you need to configure these before building.");
                return;
            }

            if (!SaveAllFiles())
            {
                await ShowError("Error saving files.", "One or more of the modified files cannot be saved, try to save them manually and check that none are open in another software.");
                return;
            }
            outDock.Select();
            outLog.Clear();
            Cleanup();
            EmulatorInfo.CanDebug = false;
            EmulatorInfo.CanRun = false;

            Task.Run(() =>
            {
                var program = ZXProjectBuilder.Build(peExplorer.RootPath, outLog.Writer);

                if (program != null)
                {
                    loadedProgram = program;
                    var disas = openEditors.FirstOrDefault(e => e.FileName == ZXConstants.DISASSEMBLY_DOC);

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (disas != null)
                        {
                            if (loadedProgram.Disassembly == null)
                            {
                                var parent = disas.Parent as TabItem;
                                parent.IsSelected = true;
                                CloseFile(null, e);
                            }
                            else
                                disas.Text = loadedProgram.Disassembly.Content;
                        }

                        if (!EmulatorInfo.IsRunning)
                            emu.Start();
                        emu.Pause();
                        emu.InjectProgram(program.Org, program.Binary, true);
                        emu.Resume();
                        emuDock.Select();
                        emu.Focus();
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
        private async void BuildAndDebug(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbcPath) || string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbasmPath))
            {
                await ShowError("Missing configuration.", "Paths for ZXBASM and ZXBC are not configured, you need to configure these before building.");
                return;
            }

            if (!SaveAllFiles())
            {
                await ShowError("Error saving files.", "One or more of the modified files cannot be saved, try to save them manually and check that none are open in another software.");
                return;
            }
            outDock.Select();
            outLog.Clear();
            Cleanup();
            BlockEditors();
            EmulatorInfo.CanDebug = false;
            EmulatorInfo.CanRun = false;
            Task.Run(() =>
            {
                var program = ZXProjectBuilder.BuildDebug(peExplorer.RootPath, outLog.Writer);

                if (program != null)
                {
                    basicBreakpoints.Clear();
                    basicBreakpoints.AddRange(program.ProgramMap.Lines.Select(l => new Breakpoint { Address = l.Address, Temporary = false, Id = ZXConstants.BASIC_BREAKPOINT, Tag = l }).ToList());

                    disassemblyBreakpoints.Clear();
                    disassemblyBreakpoints.AddRange(program.DisassemblyMap.Lines.Select(l => new Breakpoint { Address = l.Address, Temporary = false, Id = ZXConstants.ASSEMBLER_BREAKPOINT, Tag = l }).ToList());

                    loadedProgram = program;

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        var currentTab = editTabs.FirstOrDefault(e => e.IsSelected);

                        var disas = OpenFile(ZXConstants.DISASSEMBLY_DOC);

                        if (disas != null)
                        {
                            disas.Text = loadedProgram.Disassembly.Content;
                            disas.InvalidateArrange();
                            disas.InvalidateMeasure();
                            disas.InvalidateVisual();
                        }

                        var rom = OpenFile(ZXConstants.ROM_DOC);

                        if (rom != null)
                        {
                            rom.Text = romDisassembly;
                            rom.InvalidateArrange();
                            rom.InvalidateMeasure();
                            rom.InvalidateVisual();
                        }

                        if (currentTab != null)
                            currentTab.IsSelected = true;

                        UpdateUserBreakpoints();

                        if (!EmulatorInfo.IsRunning)
                            emu.Start();
                        emu.Pause();
                        emu.InjectProgram(program.Org, program.Binary, true);
                        emu.Resume();
                        emuDock.Select();
                        emu.Focus();
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
            if (string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbcPath) || string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbasmPath))
            {
                await ShowError("Missing configuration.", "Paths for ZXBASM and ZXBC are not configured, you need to configure these before building.");
                return;
            }

            ZXExportOptions? opts = null;
            string optsFile = Path.Combine(peExplorer.RootPath ?? "", ZXConstants.EXPORTSETTINGS_FILE);
            if (File.Exists(optsFile))
            {
                try
                {
                    opts = JsonConvert.DeserializeObject<ZXExportOptions>(File.ReadAllText(optsFile));
                }
                catch { }
            }

            var dlg = new ZXExportDialog();
            dlg.ExportOptions = opts;
            var res = await dlg.ShowDialog<bool>(this);

            if (!res || dlg.ExportOptions == null)
                return;

            opts = dlg.ExportOptions;

            if (!SaveAllFiles())
            {
                await ShowError("Error saving files.", "One or more of the modified files cannot be saved, try to save them manually and check that none are open in another software.");
                return;
            }

            File.WriteAllText(optsFile, JsonConvert.SerializeObject(opts));

            string file = opts.OutputPath;

            outDock.Select();
            outLog.Clear();
            Cleanup();
            BlockEditors();
            EmulatorInfo.CanDebug = false;
            EmulatorInfo.CanRun = false;
            Task.Run(() =>
            {
                ZXProjectBuilder.Export(peExplorer.RootPath, opts, outLog.Writer);
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
            var dlg = new ZXBuildSettingsDialog();
            string buildFile = Path.Combine(peExplorer.RootPath ?? "", ZXConstants.BUILDSETTINGS_FILE);
            if (File.Exists(buildFile))
            {
                var settings = JsonConvert.DeserializeObject<ZXBuildSettings>(File.ReadAllText(buildFile));
                dlg.Settings = settings;
            }
            else if (ZXOptions.Current.DefaultBuildSettings != null)
                dlg.Settings = ZXOptions.Current.DefaultBuildSettings.Clone();

            if (await dlg.ShowDialog<bool>(this))
            {
                try
                {
                    File.WriteAllText(buildFile, JsonConvert.SerializeObject(dlg.Settings));
                }
                catch (Exception ex)
                {
                    await ShowError("Error saving file", $"Unexpected error trying to save the configuration file: {ex.Message} - {ex.StackTrace}");
                }
            }
        }
        #endregion

        #region Editor control

        private void BtnRemoveBreakpoints_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            foreach (var editor in openEditors)
                editor.ClearBreakpoints();

            BreakpointManager.ClearBreakpoints();
            UpdateUserBreakpoints();
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
        }

        private async void RestoreLayout(object? sender, RoutedEventArgs e)
        {
            if (!(await ShowConfirm("Restore layout", "Are you sure you want to restore the layout to its initial configuration?")))
                return;

            ZXLayoutPersister.ResetLayout();
            skipLayout = true;

            await ShowInfo("Restore layout", "Layout has been reset, restart the application to apply the changes.");
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
                await ShowError("Invalid file", "Select a .json or .csv file.");
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
                    await ShowError("Error saving file", $"Unexpected error trying to save the register dump file: {ex.Message} - {ex.StackTrace}");
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
                    await ShowError("Error saving file", $"Unexpected error trying to save the regsiter dump file: {ex.Message} - {ex.StackTrace}");

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
        private Breakpoint? ShowBreakLines(ushort PC, bool PreferBasic)
        {
            Breakpoint? returnbp = null;

            if (PreferBasic)
            {
                var rombp = romBreakpoints.LastOrDefault(b => b.Address == PC);

                if (rombp != null)
                {
                    returnbp = rombp;
                    HighlightBreakpoint(rombp);
                }

                var disbp = disassemblyBreakpoints.LastOrDefault(b => b.Address == PC);

                if (disbp != null)
                {
                    returnbp = disbp;
                    HighlightBreakpoint(disbp);
                }
                var basbp = basicBreakpoints.LastOrDefault(b => b.Address == PC);

                if (basbp != null)
                {
                    returnbp = basbp;
                    HighlightBreakpoint(basbp);
                }
            }
            else
            {
                var basbp = basicBreakpoints.LastOrDefault(b => b.Address == PC);

                if (basbp != null)
                {
                    returnbp = basbp;
                    HighlightBreakpoint(basbp);
                }

                var rombp = romBreakpoints.LastOrDefault(b => b.Address == PC);

                if (rombp != null)
                {
                    returnbp = rombp;
                    HighlightBreakpoint(rombp);
                }

                var disbp = disassemblyBreakpoints.LastOrDefault(b => b.Address == PC);

                if (disbp != null)
                {
                    returnbp = disbp;
                    HighlightBreakpoint(disbp);
                }
            }

            return returnbp;
        }
        private void HighlightBreakpoint(Breakpoint bp)
        {
            var line = bp.Tag as ZXCodeLine;

            if (line != null)
            {
                var edit = OpenFile(line.File);
                if (edit != null)
                    edit.BreakLine = line.LineNumber + 1;
            }
        }
        private void ClearBreakLines()
        {
            foreach (var edit in openEditors)
                edit.BreakLine = null;
        }
        private void BlockEditors()
        {
            foreach (var edit in openEditors)
                edit.Readonly = true;
        }
        private void UnblockEditors()
        {
            foreach (var edit in openEditors)
            {
                if (edit.FileName != ZXConstants.DISASSEMBLY_DOC && edit.FileName != ZXConstants.ROM_DOC)
                    edit.Readonly = false;
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            emu.Stop();

            if (!skipLayout)
                ZXLayoutPersister.SaveLayout(grdMain, dockLeft, dockRight, dockBottom);

            base.OnClosing(e);

        }
        #endregion

        #region Global keyb handling
        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(RawInputEventArgs value)
        {
            if (value is RawKeyEventArgs)
            {
                RawKeyEventArgs args = (RawKeyEventArgs)value;

                if (EmulatorInfo.IsRunning)
                {
                    if (args.Key == Key.Enter && args.Modifiers == RawInputModifiers.Alt)
                    {

                        if (args.Type == RawKeyEventType.KeyUp)
                            SwapFullScreen();

                        return;
                    }

                    try
                    {
                        switch (args.Key)
                        {
                            case Key.LeftShift:
                            case Key.RightShift:

                                if (args.Type == RawKeyEventType.KeyUp)
                                    emu.SendKeyUp(CoreSpectrum.Enums.SpectrumKeys.Caps);
                                else
                                    emu.SendKeyDown(CoreSpectrum.Enums.SpectrumKeys.Caps);
                                break;
                            case Key.LeftCtrl:
                            case Key.RightCtrl:
                                if (args.Type == RawKeyEventType.KeyUp)
                                    emu.SendKeyUp(CoreSpectrum.Enums.SpectrumKeys.Sym);
                                else
                                    emu.SendKeyDown(CoreSpectrum.Enums.SpectrumKeys.Sym);
                                break;
                            case Key.Enter:
                                if (args.Type == RawKeyEventType.KeyUp)
                                    emu.SendKeyUp(CoreSpectrum.Enums.SpectrumKeys.Enter);
                                else
                                    emu.SendKeyDown(CoreSpectrum.Enums.SpectrumKeys.Enter);
                                break;
                            default:
                                if (Enum.TryParse<SpectrumKeys>(args.Key.ToString(), true, out var key))
                                {
                                    if (args.Type == RawKeyEventType.KeyUp)
                                        emu.SendKeyUp(key);
                                    else
                                        emu.SendKeyDown(key);
                                }
                                break;
                        }
                    }
                    catch { }

                }

                if (args.Type != RawKeyEventType.KeyUp)
                    return;

                switch (args.Key)
                {
                    case Key.F5:
                        if (EmulatorInfo.CanRun)
                            BuildAndRun(this, null);
                        value.Handled = true;
                        break;
                    case Key.F6:
                        if (EmulatorInfo.CanDebug)
                            BuildAndDebug(this, null);
                        value.Handled = true;
                        break;
                    case Key.F7:
                        if (EmulatorInfo.CanPause)
                            PauseEmulator(this, null);
                        value.Handled = true;
                        break;
                    case Key.F8:
                        if (EmulatorInfo.CanResume)
                            ResumeEmulator(this, null);
                        value.Handled = true;
                        break;
                    case Key.F9:
                        if (EmulatorInfo.CanStep)
                            AssemblerStepEmulator(this, null);
                        value.Handled = true;
                        break;
                    case Key.F10:
                        if (EmulatorInfo.CanStep)
                            BasicStepEmulator(this, null);
                        value.Handled = true;
                        break;
                    case Key.F11:
                        if (EmulatorInfo.IsRunning)
                            StopEmulator(this, null);
                        value.Handled = true;
                        break;

                    case Key.F12:
                        if (EmulatorInfo.IsRunning)
                            TurboModeEmulator(this, null);
                        value.Handled = true;
                        break;
                }
            }
        }
        #endregion
    }

    public enum PreferredSourceType
    {
        Basic,
        Disassembly,
        ROM
    }

    public class FileInfoProvider : AvaloniaObject
    {
        StyledProperty<bool> ProjectLoadedProperty = StyledProperty<bool>.Register<FileInfoProvider, bool>("ProjectLoaded", false);
        StyledProperty<bool> FileLoadedProperty = StyledProperty<bool>.Register<FileInfoProvider, bool>("FileLoaded", false);
        StyledProperty<bool> FileSystemObjectSelectedProperty = StyledProperty<bool>.Register<FileInfoProvider, bool>("FileSystemObjectSelected", false);

        public bool ProjectLoaded
        {
            get { return GetValue<bool>(ProjectLoadedProperty); }
            set { SetValue<bool>(ProjectLoadedProperty, value); }
        }
        public bool FileLoaded
        {
            get { return GetValue<bool>(FileLoadedProperty); }
            set { SetValue<bool>(FileLoadedProperty, value); }
        }
        public bool FileSystemObjectSelected
        {
            get { return GetValue<bool>(FileSystemObjectSelectedProperty); }
            set { SetValue<bool>(FileSystemObjectSelectedProperty, value); }
        }
    }
    public class EmulatorInfoProvider : AvaloniaObject
    {
        StyledProperty<bool> IsRunningProperty = StyledProperty<bool>.Register<EmulatorInfoProvider, bool>("IsRunning", false);
        StyledProperty<bool> IsPausedProperty = StyledProperty<bool>.Register<EmulatorInfoProvider, bool>("IsPaused", false);
        StyledProperty<bool> IsDebuggingProperty = StyledProperty<bool>.Register<EmulatorInfoProvider, bool>("IsDebugging", false);
        StyledProperty<bool> CanPauseProperty = StyledProperty<bool>.Register<EmulatorInfoProvider, bool>("CanPause", false);
        StyledProperty<bool> CanResumeProperty = StyledProperty<bool>.Register<EmulatorInfoProvider, bool>("CanResume", false);
        StyledProperty<bool> CanStepProperty = StyledProperty<bool>.Register<EmulatorInfoProvider, bool>("CanStep", false);

        StyledProperty<bool> CanRunProperty = StyledProperty<bool>.Register<EmulatorInfoProvider, bool>("CanRun", false);
        StyledProperty<bool> CanDebugProperty = StyledProperty<bool>.Register<EmulatorInfoProvider, bool>("CanDebug", false);

        public bool IsRunning
        {
            get { return GetValue<bool>(IsRunningProperty); }
            set { SetValue<bool>(IsRunningProperty, value); }
        }
        public bool IsPaused
        {
            get { return GetValue<bool>(IsPausedProperty); }
            set { SetValue<bool>(IsPausedProperty, value); }
        }
        public bool IsDebugging
        {
            get { return GetValue<bool>(IsDebuggingProperty); }
            set { SetValue<bool>(IsDebuggingProperty, value); }
        }
        public bool CanPause
        {
            get { return GetValue<bool>(CanPauseProperty); }
            set { SetValue<bool>(CanPauseProperty, value); }
        }
        public bool CanResume
        {
            get { return GetValue<bool>(CanResumeProperty); }
            set { SetValue<bool>(CanResumeProperty, value); }
        }
        public bool CanStep
        {
            get { return GetValue<bool>(CanStepProperty); }
            set { SetValue<bool>(CanStepProperty, value); }
        }
        public bool CanRun
        {
            get { return GetValue<bool>(CanRunProperty); }
            set { SetValue<bool>(CanRunProperty, value); }
        }
        public bool CanDebug
        {
            get { return GetValue<bool>(CanDebugProperty); }
            set { SetValue<bool>(CanDebugProperty, value); }
        }
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
