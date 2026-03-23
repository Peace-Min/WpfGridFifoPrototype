using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using DevExpress.Mvvm;
using WpfGridFifoPrototype.PrototypeSupport;

namespace WpfGridFifoPrototype.Models
{
    public class DetailItem : BindableBase
    {
        public bool IsEmpty => string.IsNullOrEmpty(AttributeLabel);

        public bool CanMoveUp { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanMoveDown { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSelected { get => GetValue<bool>(); set => SetValue(value); }

        public string QualifiedName { get => GetValue<string>(); set => SetValue(value); }

        public string ObjectName { get => GetValue<string>(); set => SetValue(value); }

        public string AttributeName { get => GetValue<string>(); set => SetValue(value); }

        public string ObjectLabel { get => GetValue<string>(); set => SetValue(value); }

        public string AttributeLabel
        {
            get => GetValue<string>();
            set
            {
                SetValue(value);
                RaisePropertyChanged(nameof(IsEmpty));
            }
        }

        public string DisplayUnit { get => GetValue<string>(); set => SetValue(value); }

        public string RoleLabel { get => GetValue<string>(); set => SetValue(value); }

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

        public void CopyPayloadFrom(DetailItem source)
        {
            if (source == null)
            {
                Clear();
                return;
            }

            QualifiedName = source.QualifiedName;
            ObjectName = source.ObjectName;
            AttributeName = source.AttributeName;
            ObjectLabel = source.ObjectLabel;
            AttributeLabel = source.AttributeLabel;
            DisplayUnit = source.DisplayUnit;
            InsertTimestamp = source.InsertTimestamp;
        }

        public void SwapPayloadWith(DetailItem other)
        {
            if (other == null || ReferenceEquals(this, other))
                return;

            var qualifiedName = QualifiedName;
            var objectName = ObjectName;
            var attributeName = AttributeName;
            var objectLabel = ObjectLabel;
            var attributeLabel = AttributeLabel;
            var displayUnit = DisplayUnit;
            var insertTimestamp = InsertTimestamp;

            CopyPayloadFrom(other);

            other.QualifiedName = qualifiedName;
            other.ObjectName = objectName;
            other.AttributeName = attributeName;
            other.ObjectLabel = objectLabel;
            other.AttributeLabel = attributeLabel;
            other.DisplayUnit = displayUnit;
            other.InsertTimestamp = insertTimestamp;
        }
    }

    public class TargetRow : BindableBase, IColored
    {
        public int No { get => GetValue<int>(); set => SetValue(value); }

        public string GroupTitle { get => GetValue<string>(); set => SetValue(value); }

        public Color Color { get => GetValue<Color>(); set => SetValue(value); }

        public LineSeriesType LineSeriesType { get => GetValue<LineSeriesType>(); set => SetValue(value); }

        public ObservableCollection<DetailItem> Details { get; set; } = new ObservableCollection<DetailItem>();
    }
}
