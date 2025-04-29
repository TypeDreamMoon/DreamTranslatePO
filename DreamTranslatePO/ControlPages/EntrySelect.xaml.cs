using CommunityToolkit.WinUI.Controls;
using DreamTranslatePO.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace DreamTranslatePO.ControlPages;

public sealed partial class EntrySelect : Page
{
    private int start = 0;
    private int end = 0;
    private bool isUpdatingRange = false;
    
    public EntrySelect()
    {
        InitializeComponent();
    }

    public void SetSliderRange(int min, int max)
    {
        start = min;
        end = max;
        ValueRangeSelector.Minimum = min;
        ValueRangeSelector.Maximum = max;
        MinNumberBox.Value = min;
        MinNumberBox.Minimum = min;
        MinNumberBox.Maximum = max;
        MaxNumberBox.Value = max;
        MaxNumberBox.Minimum = min;
        MaxNumberBox.Maximum = max;
    }

    public int GetSliderRangeMinimum()
    {
        return start;
    }

    public int GetSliderRangeMaximum()
    {
        return end;
    }

    private void RangeSelector_OnValueChanged(object? sender, RangeChangedEventArgs e)
    {
        // 如果正在更新范围，则跳过
        if (isUpdatingRange) return;

        try
        {
            isUpdatingRange = true;

            start = (int)ValueRangeSelector.RangeStart;
            if (MinNumberBox != null)
            {
                MinNumberBox.Value = start;
            }

            end = (int)ValueRangeSelector.RangeEnd;
            if (MaxNumberBox != null)
            {
                MaxNumberBox.Value = end;
            }
        }
        finally
        {
            isUpdatingRange = false;
        }
    }

    private void MinNumberBox_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        // 如果正在更新范围，则跳过
        if (isUpdatingRange) return;

        try
        {
            isUpdatingRange = true;

            if (ValueRangeSelector != null)
            {
                ValueRangeSelector.RangeStart = MinNumberBox.Value;
            }
        }
        finally
        {
            isUpdatingRange = false;
        }
    }

    private void MaxNumberBox_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        // 如果正在更新范围，则跳过
        if (isUpdatingRange) return;

        try
        {
            isUpdatingRange = true;

            if (ValueRangeSelector != null)
            {
                ValueRangeSelector.RangeEnd = MaxNumberBox.Value;
            }
        }
        finally
        {
            isUpdatingRange = false;
        }
    }

}