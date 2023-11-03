using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ZXBasicStudio.Classes;
using System.Linq;

namespace ZXBasicStudio.Dialogs
{
    public partial class ZXBuildSettingsDialog : ZXWindowBase
    {
        ZXBuildSettings _settings = new ZXBuildSettings();
        public ZXBuildSettings Settings { get { return _settings; } set { _settings = value; UpdateUI(); } }
        public ZXBuildSettingsDialog()
        {
            InitializeComponent();
            btnAccept.Click += BtnAccept_Click;
            btnCancel.Click += BtnCancel_Click;
            ckSinclair.IsCheckedChanged += CkSinclair_IsCheckedChanged;
        }

        private void CkSinclair_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if ((ckSinclair.IsChecked ?? false) == true)
            {
                cbArrayBase.IsEnabled = false;
                cbStringBase.IsEnabled = false;
                ckStrictBool.IsEnabled = false;
                ckCase.IsEnabled = false;

                cbArrayBase.SelectedIndex = 1;
                cbStringBase.SelectedIndex = 1;
                ckStrictBool.IsChecked = true;
                ckCase.IsChecked = true;
            }
            else
            {
                cbArrayBase.IsEnabled = true;
                cbStringBase.IsEnabled = true;
                ckStrictBool.IsEnabled = true;
                ckCase.IsEnabled = true;
            }
        }

        private void BtnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close(false);
        }

        private void BtnAccept_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            UpdateSettings();
            this.Close(true);
        }

        void UpdateUI()
        {
            if (_settings != null)
            {
                txtFile.Text = _settings.MainFile ?? "";
                cbOptimization.SelectedIndex = _settings.OptimizationLevel ?? -1;
                nudOrg.Value = _settings.Origin ?? 32768;

                ckSinclair.IsChecked = _settings.SinclairMode;
                if (!_settings.SinclairMode)
                {
                    cbArrayBase.SelectedIndex = _settings.ArrayBase ?? -1;
                    cbStringBase.SelectedIndex = _settings.StringBase ?? -1;
                    ckStrictBool.IsChecked = _settings.StrictBool;
                    ckCase.IsChecked = _settings.IgnoreCase;
                }

                nudHeap.Value = _settings.HeapSize ?? 4768;
                ckBreak.IsChecked = _settings.EnableBreak;
                ckExplicit.IsChecked = _settings.Explicit;
                txtDefines.Text = _settings.Defines == null || _settings.Defines.Length == 0 ? "" : string.Join(", ", _settings.Defines);
                ckStrict.IsChecked = _settings.StrictBool;
                ckHeaderless.IsChecked = _settings.Headerless;
                ckNext.IsChecked = _settings.NextMode;
            }
            else
            {
                txtFile.Text = "";
                cbOptimization.SelectedIndex =  -1;
                nudOrg.Value = 32768;
                cbArrayBase.SelectedIndex = -1;
                cbStringBase.SelectedIndex = -1;
                ckSinclair.IsChecked = false;
                nudHeap.Value = 4768;
                ckStrictBool.IsChecked = false;
                ckBreak.IsChecked = false;
                ckExplicit.IsChecked = false;
                txtDefines.Text = "";
                ckCase.IsChecked = false;
                ckStrict.IsChecked = false;
                ckHeaderless.IsChecked = false;
                ckNext.IsChecked = false;
            }
        }

        void UpdateSettings()
        {
            if (_settings == null)
                _settings = new ZXBuildSettings();

            _settings.MainFile = string.IsNullOrWhiteSpace(txtFile.Text) ? null : txtFile.Text;
            _settings.OptimizationLevel = cbOptimization.SelectedIndex == -1 ? null : cbOptimization.SelectedIndex;
            _settings.Origin = nudOrg.Value == null || nudOrg.Value == 32768 ? null : (ushort)nudOrg.Value;
            _settings.ArrayBase = cbArrayBase.SelectedIndex == -1 ? null : cbArrayBase.SelectedIndex;
            _settings.StringBase = cbStringBase.SelectedIndex == -1 ? null : cbStringBase.SelectedIndex;
            _settings.SinclairMode = ckSinclair.IsChecked ?? false;
            _settings.HeapSize = (int?)(nudHeap.Value == 4768 ? null : nudHeap.Value);
            _settings.StrictBool = ckStrictBool.IsChecked ?? false;
            _settings.EnableBreak = ckBreak.IsChecked ?? false;
            _settings.Explicit = ckExplicit.IsChecked ?? false;
            _settings.Defines = string.IsNullOrWhiteSpace(txtDefines.Text) ? null : txtDefines.Text.Split(",").Select(s => s.Trim()).ToArray();
            _settings.IgnoreCase = ckCase.IsChecked ?? false;
            _settings.StrictBool = ckStrict.IsChecked ?? false;
            _settings.Headerless = ckHeaderless.IsChecked ?? false;
            _settings.NextMode = ckNext.IsChecked ?? false;
        }
    }
}
