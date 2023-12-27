using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace ZXBasicStudio;

public partial class ZXAboutDialog : Window
{
    public ZXAboutDialog()
    {
        InitializeComponent();

        btnClose.Click += BtnClose_Click;

        var name = System.Reflection.Assembly.GetExecutingAssembly().GetName();

        if(name == null || name.Version == null)
        {
            txtBuild.Text = "Unknown build";
            txtDate.Text = "Unknown date";
            return;
        }

        DateTime buildDate = new DateTime(2000, 1, 1).AddDays(name.Version.Build);

        txtBuild.Text = $"Build {name.Version.ToString()}";
        txtDate.Text = buildDate.ToString("yyyy-MM-dd");
    }

    private void BtnClose_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}