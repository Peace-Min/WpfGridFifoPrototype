using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using WpfGridFifoPrototype.Models;

namespace WpfGridFifoPrototype.PrototypeSupport
{
    public class UserAnalItemData
    {
        public string QualifiedName { get; set; }
        public string ObjectName { get; set; }
        public string AttributeName { get; set; }
        public string ObjectLabel { get; set; }
        public string AttributeLabel { get; set; }
        public string Unit { get; set; }
    }

    public class ChartComponentConfig
    {
        public int DimensionMode { get; set; }
        public ObservableCollection<TargetRow> TargetRows { get; set; } = new ObservableCollection<TargetRow>();
    }

    public interface IColored
    {
        Color Color { get; set; }
    }

    public enum LineSeriesType
    {
        [Description("Line")]
        Line,

        [Description("Spline")]
        Spline,

        [Description("Step")]
        StepLine
    }

    public static class EnumUtil
    {
        public static string GetDescription(Enum value)
        {
            if (value == null)
                return string.Empty;

            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var attribute = member?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }
    }

    public class ChartColorService
    {
        private readonly Color[] _palette =
        {
            Color.FromRgb(59, 130, 246),
            Color.FromRgb(16, 185, 129),
            Color.FromRgb(244, 63, 94),
            Color.FromRgb(251, 191, 36),
            Color.FromRgb(139, 92, 246),
            Color.FromRgb(14, 165, 233)
        };

        public Color GenerateUniqueColor(IEnumerable<IColored> coloredItems)
        {
            var usedColors = new HashSet<Color>((coloredItems ?? Enumerable.Empty<IColored>()).Select(item => item.Color));
            var availableColor = _palette.FirstOrDefault(color => !usedColors.Contains(color));
            return availableColor == default(Color)
                ? _palette[usedColors.Count % _palette.Length]
                : availableColor;
        }
    }
}
