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
        public string Y { get; set; } // 데이터명
        public string Attr { get; set; } // 속성
        public bool IsEmpty => string.IsNullOrEmpty(Y);

        private string _roleLabel; // X, Y, Z 등 시각적 라벨
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
    }

    public class TargetRow : BindableBase
    {
        public int No { get; set; }

        private string _label = "New Target";
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value, nameof(Label));
        }

        public ObservableCollection<DetailItem> Details { get; set; } = new ObservableCollection<DetailItem>();
        public string Color { get; set; }
    }
}
