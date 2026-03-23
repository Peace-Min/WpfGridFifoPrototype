using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace WpfGridFifoPrototype.Models
{
    public class SourceItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Attr { get; set; }
    }

    public class DetailItem : BindableBase
    {
        private string _y;
        public string Y
        {
            get => _y;
            set
            {
                if (SetProperty(ref _y, value, nameof(Y)))
                    RaisePropertyChanged(nameof(IsEmpty));
            }
        }

        private string _attr;
        public string Attr
        {
            get => _attr;
            set => SetProperty(ref _attr, value, nameof(Attr));
        }

        public bool IsEmpty => string.IsNullOrEmpty(Y);

        private string _roleLabel;
        public string RoleLabel
        {
            get => _roleLabel;
            set => SetProperty(ref _roleLabel, value, nameof(RoleLabel));
        }

        private bool _canMoveUp;
        public bool CanMoveUp
        {
            get => _canMoveUp;
            set => SetProperty(ref _canMoveUp, value, nameof(CanMoveUp));
        }

        private bool _canMoveDown;
        public bool CanMoveDown
        {
            get => _canMoveDown;
            set => SetProperty(ref _canMoveDown, value, nameof(CanMoveDown));
        }

        private long _insertTimestamp;
        public long InsertTimestamp
        {
            get => _insertTimestamp;
            set => SetProperty(ref _insertTimestamp, value, nameof(InsertTimestamp));
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value, nameof(IsSelected));
        }

        public void ClearContent()
        {
            Y = null;
            Attr = null;
            InsertTimestamp = 0;
        }

        public void AssignFrom(DetailItem source)
        {
            Y = source.Y;
            Attr = source.Attr;
            InsertTimestamp = source.InsertTimestamp;
        }
    }

    public class TargetRow : BindableBase
    {
        private int _no;
        public int No
        {
            get => _no;
            set => SetProperty(ref _no, value, nameof(No));
        }

        private string _label = "New Target";
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value, nameof(Label));
        }

        public ObservableCollection<DetailItem> Details { get; set; } = new ObservableCollection<DetailItem>();

        private string _color;
        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value, nameof(Color));
        }
    }
}
