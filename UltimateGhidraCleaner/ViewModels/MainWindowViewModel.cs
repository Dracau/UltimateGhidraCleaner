using System;
using System.Collections.Generic;
using System.IO;
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
    [ObservableProperty] private bool _shouldCleanupNamespaces = true;
    [ObservableProperty] private bool _shouldRemoveCasts = true;
    [ObservableProperty] private bool _shouldReplaceThis = true;
    [ObservableProperty] private bool _shouldCommentLABGotos = true;
    [ObservableProperty] private bool _shouldReplaceHashes = true;

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
        if (ShouldCleanupNamespaces) buffer = CleanupNamespaces(buffer);
        if (ShouldRemoveCasts) buffer = RemoveCasts(buffer);
        if (ShouldReplaceThis) buffer = ThisBecomesFighter(buffer);
        if (ShouldCommentLABGotos) buffer = CommentLABgotos(buffer);
        if (ShouldReplaceHashes) buffer = ReplaceHashes(buffer);
        
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
                start = j;
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
        return input.Replace(" & 1U", "").Replace(" & 1", "").Replace(" != 0","");
    }
    
    private string CleanupNamespaces(string input)
    {
        return input.Replace("lib::L2CValue::", "")
            .Replace("app::lua_bind::", "")
            .Replace("app::sv_battle_object::", "")
            .Replace("lib::L2CAgent::", "")
            .Replace("lua2cpp::L2CFighterBase::", "") //Maybe a separate function for fighter stuff
            .Replace("lua2cpp::L2CFighterCommon::", "");
    }

    private string RemoveCasts(string input)
    {
        return input.Replace("operator.cast.to.bool", "")
            .Replace("as_integer","")
            .Replace("(bool)", "")
            .Replace("(float)", "")
            .Replace("(BattleObjectModuleAccessor **)", "")
            .Replace("(L2CValue *)","")
            .Replace("(L2CValue)","")
            .Replace("((L2CAgent *))","");
    }

    private string ThisBecomesFighter(string input)
    {
        return input.Replace("this->", "fighter.").Replace("this", "fighter");
    }

    private string CommentLABgotos(string input)
    {
        return input.Replace("\nLAB","\n//LAB").Replace("goto LAB", "//goto LAB");
    }

    private static string labelsPath = "C:\\Users\\supre\\Desktop\\Smash Mods\\Outils\\Ghidra Reqs\\ParamLabels.csv";
    private static string allowedCharsInHashes = "0123456789ABCDEFabcdef";
    
    Dictionary<string, string> hashDictionary = new Dictionary<string, string>();
    private string ReplaceHashes(string input)
    {
        if (hashDictionary.Count == 0)
        {
            string[] lines = File.ReadAllLines(labelsPath);
            foreach (string line in lines)
            {
                string[] lineSplits = line.Split(',');
                hashDictionary.Add(lineSplits[0],lineSplits[1]);
            }
        }
        
        for (int i = 0; i < input.Length; i++)
        {
            i = input.IndexOf("0x",i);
            if(i == -1) break;
            int hashLength = 2;
            for (int j = i + 2; j < input.Length; j++)
            {
                if(!allowedCharsInHashes.Contains(input[j])) break;
                hashLength++;
            }
            string hash = input.Substring(i, hashLength);
            
            if(hashDictionary.ContainsKey(hash)) input = input.Replace(hash,$"hash40(\"{hashDictionary[hash]}\")");
        }

        return input;
    }
}