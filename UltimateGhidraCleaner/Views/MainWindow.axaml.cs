using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using UltimateGhidraCleaner.ViewModels;

namespace UltimateGhidraCleaner.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        //DataContext = new MainWindowViewModel();
    }
}