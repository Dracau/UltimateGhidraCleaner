using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UltimateGhidraCleaner.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string? _input, _output;
    [ObservableProperty] private bool _shouldFixIndent = true;
    [ObservableProperty] private bool _shouldRemoveDestructors = true;
    [ObservableProperty] private bool _shouldRemoveImpl = true;
    [ObservableProperty] private bool _shouldReplaceUnderscores = true;
    [ObservableProperty] private bool _shouldCleanupConditions = true;

    //[ObservableProperty] private bool _shouldCommentLABFUNGotos = true;

    [RelayCommand]
    public void Clean()
    {
        if(Input == null) return;
        
        string buffer = Input;

        if (ShouldFixIndent) buffer = FixIndent(buffer);
        if (ShouldRemoveDestructors) buffer = RemoveDestructors(buffer);
        if (ShouldRemoveImpl) buffer = RemoveImpl(buffer);
        if (ShouldReplaceUnderscores) buffer = ReplaceUnderscores(buffer);
        if (ShouldCleanupConditions) buffer = CleanupConditions(buffer);
        
        Output = buffer;
    }

    private Tuple<string, int> RemoveLine(string input, int index)
    {
        int start = 0;
        int end = 0;
            
        for (int j = index; j > 0; j--)
        {
            if (input[j] == '\n')
            {
                start = j-1;
                break;
            }
        }
            
        for (int j = index; j < input.Length; j++)
        {
            if (input[j] == '\n')
            {
                end = j;
                break;
            }
        }

        input = input.Remove(start, end-start);
        return new Tuple<string, int>(input, start);
    }

    private string FixIndent(string input)
    {
        return input.Replace("  ", "\t");
    }

    private string RemoveDestructors(string input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            if(input[i] != '~') continue;
            Tuple<string, int> tuple = RemoveLine(input, i);
            input = tuple.Item1;
            i = tuple.Item2;
        }
        return input;
    }

    private string RemoveImpl(string input)
    {
        return input.Replace("_impl", "");
    }

    private string ReplaceUnderscores(string input)
    {
        return input.Replace("__", "::");
    }

    private string CleanupConditions(string input)
    {
        return input.Replace(" & 1U", "").Replace(" & 1", "");
    }
}