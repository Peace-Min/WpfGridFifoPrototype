using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using DevExpress.Mvvm;

namespace WpfGridFifoPrototype.Models
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
        [Description("직선")]
        Line,

        [Description("곡선")]
        Spline,

        [Description("계단형")]
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
            return _palette.FirstOrDefault(color => !usedColors.Contains(color)) == default(Color)
                ? _palette[usedColors.Count % _palette.Length]
                : _palette.First(color => !usedColors.Contains(color));
        }
    }

    /// <summary>
    /// 그룹정보 BindingModel.
    /// </summary>
    public class DetailItem : BindableBase
    {
        /// <summary>
        /// Row 데이터 할당 여부 판단.
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(AttributeLabel);

        /// <summary>
        /// 상단 이동 가능 여부.
        /// </summary>
        public bool CanMoveUp { get => GetValue<bool>(); set => SetValue(value); }

        /// <summary>
        /// 하단 이동 가능 여부.
        /// </summary>
        public bool CanMoveDown { get => GetValue<bool>(); set => SetValue(value); }

        /// <summary>
        /// 선택 여부.
        /// </summary>
        public bool IsSelected { get => GetValue<bool>(); set => SetValue(value); }

        /// <summary>
        /// 트리 구조 식별자.
        /// </summary>
        public string QualifiedName { get => GetValue<string>(); set => SetValue(value); }

        /// <summary>
        /// 객체 명.
        /// </summary>
        public string ObjectName { get => GetValue<string>(); set => SetValue(value); }

        /// <summary>
        /// 속성 명.
        /// </summary>
        public string AttributeName { get => GetValue<string>(); set => SetValue(value); }

        /// <summary>
        /// 객체 라벨명.
        /// </summary>
        public string ObjectLabel { get => GetValue<string>(); set => SetValue(value); }

        /// <summary>
        /// 속성 라벨명.
        /// </summary>
        public string AttributeLabel
        {
            get => GetValue<string>();
            set
            {
                SetValue(value);
                RaisePropertyChanged(nameof(IsEmpty));
            }
        }

        /// <summary>
        /// 객체 전시 단위.
        /// </summary>
        public string DisplayUnit { get => GetValue<string>(); set => SetValue(value); }

        /// <summary>
        /// 해당 Row 축 전시명 (X,Y,Z).
        /// </summary>
        public string RoleLabel { get => GetValue<string>(); set => SetValue(value); }

        /// <summary>
        /// 데이터 할당 시간 (FIFO 로직에 사용됨).
        /// </summary>
        public long InsertTimestamp { get => GetValue<long>(); set => SetValue(value); }

        public void UpdateFromSource(UserAnalItemData source)
        {
            if (source == null)
                return;

            if (QualifiedName == source.QualifiedName)
                return;

            InsertTimestamp = DateTime.Now.Ticks;
            ObjectName = source.ObjectName;
            AttributeName = source.AttributeName;
            QualifiedName = source.QualifiedName;
            ObjectLabel = source.ObjectLabel;
            AttributeLabel = source.AttributeLabel;
            DisplayUnit = source.Unit;
        }

        public void Clear()
        {
            InsertTimestamp = 0;
            QualifiedName = null;
            ObjectName = null;
            AttributeName = null;
            ObjectLabel = null;
            AttributeLabel = null;
            DisplayUnit = null;
        }

        public DetailItemSnapshot ToSnapshot()
        {
            return new DetailItemSnapshot
            {
                QualifiedName = QualifiedName,
                ObjectName = ObjectName,
                AttributeName = AttributeName,
                ObjectLabel = ObjectLabel,
                AttributeLabel = AttributeLabel,
                DisplayUnit = DisplayUnit,
                InsertTimestamp = InsertTimestamp
            };
        }

        public void ApplySnapshot(DetailItemSnapshot snapshot)
        {
            QualifiedName = snapshot.QualifiedName;
            ObjectName = snapshot.ObjectName;
            AttributeName = snapshot.AttributeName;
            ObjectLabel = snapshot.ObjectLabel;
            AttributeLabel = snapshot.AttributeLabel;
            DisplayUnit = snapshot.DisplayUnit;
            InsertTimestamp = snapshot.InsertTimestamp;
        }
    }

    public class DetailItemSnapshot
    {
        public string QualifiedName { get; set; }
        public string ObjectName { get; set; }
        public string AttributeName { get; set; }
        public string ObjectLabel { get; set; }
        public string AttributeLabel { get; set; }
        public string DisplayUnit { get; set; }
        public long InsertTimestamp { get; set; }
    }

    /// <summary>
    /// 차트정보 BindingModel.
    /// </summary>
    public class TargetRow : BindableBase, IColored
    {
        /// <summary>
        /// 순번.
        /// </summary>
        public int No { get => GetValue<int>(); set => SetValue(value); }

        /// <summary>
        /// 그룹 명.
        /// </summary>
        public string GroupTitle { get => GetValue<string>(); set => SetValue(value); }

        /// <summary>
        /// 시리즈 색상.
        /// </summary>
        public Color Color { get => GetValue<Color>(); set => SetValue(value); }

        /// <summary>
        /// 시리즈 종류.
        /// </summary>
        public LineSeriesType LineSeriesType { get => GetValue<LineSeriesType>(); set => SetValue(value); }

        /// <summary>
        /// 그룹 정보 컬렉션.
        /// </summary>
        public ObservableCollection<DetailItem> Details { get; set; } = new ObservableCollection<DetailItem>();
    }
}
