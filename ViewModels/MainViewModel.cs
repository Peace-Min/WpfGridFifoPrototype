using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using WpfGridFifoPrototype.Models;

namespace WpfGridFifoPrototype.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<SourceItem> SourceItems { get; }
        public ObservableCollection<TargetRow> TargetRows { get; }

        private TargetRow _selectedTargetRow;
        public TargetRow SelectedTargetRow
        {
            get => _selectedTargetRow;
            set => SetProperty(ref _selectedTargetRow, value, nameof(SelectedTargetRow));
        }

        private int _dimensionMode = 2; // 기본 2D (X, Y)
        public int DimensionMode
        {
            get => _dimensionMode;
            set
            {
                if (SetProperty(ref _dimensionMode, value, nameof(DimensionMode)))
                    OnDimensionModeChanged();
            }
        }
        public DelegateCommand<SourceItem> AddToTargetCommand { get; }
        public DelegateCommand<DetailItem> RemoveDetailCommand { get; }
        public DelegateCommand AddTargetRowCommand { get; }
        public DelegateCommand<TargetRow> DeleteTargetRowCommand { get; }
        public DelegateCommand<DetailItem> MoveDetailUpCommand { get; }
        public DelegateCommand<DetailItem> MoveDetailDownCommand { get; }

        public MainViewModel()
        {
            SourceItems = new ObservableCollection<SourceItem>();
            TargetRows = new ObservableCollection<TargetRow>();

            // 초기 데이터
            SourceItems.Add(new SourceItem { Id = 1, Name = "PL-10", Attr = "Main Engine Temp" });
            SourceItems.Add(new SourceItem { Id = 2, Name = "PL-20", Attr = "Fuel Pressure" });
            SourceItems.Add(new SourceItem { Id = 3, Name = "AG-50", Attr = "Altitude Sensor" });

            TargetRows.Add(CreateInitialRow(1, "Main Monitor", "Active"));
            TargetRows.Add(CreateInitialRow(2, "Sub Monitor", "Standby"));

            SelectedTargetRow = TargetRows.FirstOrDefault();

            AddToTargetCommand = new DelegateCommand<SourceItem>(AddToTarget);
            RemoveDetailCommand = new DelegateCommand<DetailItem>(RemoveDetail);
            AddTargetRowCommand = new DelegateCommand(AddTargetRow);
            DeleteTargetRowCommand = new DelegateCommand<TargetRow>(DeleteTargetRow);
            MoveDetailUpCommand = new DelegateCommand<DetailItem>(MoveDetailUp);
            MoveDetailDownCommand = new DelegateCommand<DetailItem>(MoveDetailDown);
        }

        private void OnDimensionModeChanged()
        {
            foreach (var row in TargetRows)
            {
                AdjustSlots(row);
            }
        }

        private TargetRow CreateInitialRow(int no, string label, string color)
        {
            var row = new TargetRow { No = no, Label = label, Color = color };
            AdjustSlots(row);
            return row;
        }

        private void AdjustSlots(TargetRow row)
        {
            // 차원에 맞춰 슬롯 개수 조정
            while (row.Details.Count < DimensionMode) row.Details.Add(new DetailItem());
            while (row.Details.Count > DimensionMode) row.Details.RemoveAt(row.Details.Count - 1);
            UpdateRoleAndStatus(row);
        }

        private void UpdateRoleAndStatus(TargetRow row)
        {
            string[] labels = { "X", "Y", "Z" };
            for (int i = 0; i < row.Details.Count; i++)
            {
                row.Details[i].RoleLabel = labels[i];
                row.Details[i].CanMoveUp = i > 0;
                row.Details[i].CanMoveDown = i < row.Details.Count - 1;
            }
        }

        private void AddToTarget(SourceItem source)
        {
            if (source == null || SelectedTargetRow == null) return;

            var details = SelectedTargetRow.Details;
            int emptyIdx = -1;
            for (int i = 0; i < details.Count; i++)
            {
                if (details[i].IsEmpty) { emptyIdx = i; break; }
            }

            if (emptyIdx != -1)
            {
                // 빈 자리가 있으면 채움
                details[emptyIdx] = new DetailItem { Y = source.Name, Attr = source.Attr };
            }
            else
            {
                // 가득 찼으면 FIFO (전체 Shift Up)
                for (int i = 0; i < details.Count - 1; i++)
                {
                    details[i] = details[i + 1];
                }
                details[details.Count - 1] = new DetailItem { Y = source.Name, Attr = source.Attr };
            }
            UpdateRoleAndStatus(SelectedTargetRow);
        }

        private void MoveDetailUp(DetailItem item)
        {
            MoveItem(item, -1);
        }

        private void MoveDetailDown(DetailItem item)
        {
            MoveItem(item, 1);
        }

        private void MoveItem(DetailItem item, int direction)
        {
            if (item == null) return;
            var row = TargetRows.FirstOrDefault(r => r.Details.Contains(item));
            if (row == null) return;

            int oldIdx = row.Details.IndexOf(item);
            int newIdx = oldIdx + direction;

            if (newIdx >= 0 && newIdx < row.Details.Count)
            {
                var targetItem = row.Details[newIdx];
                row.Details[oldIdx] = targetItem;
                row.Details[newIdx] = item;
                UpdateRoleAndStatus(row);
            }
        }

        private void AddTargetRow()
        {
            int nextNo = TargetRows.Any() ? TargetRows.Max(r => r.No) + 1 : 1;
            var newRow = CreateInitialRow(nextNo, $"New Group {nextNo}", "Standby");
            TargetRows.Add(newRow);
            SelectedTargetRow = newRow;
        }

        private void RemoveDetail(DetailItem detail)
        {
            if (detail == null) return;
            foreach (var row in TargetRows)
            {
                int idx = row.Details.IndexOf(detail);
                if (idx >= 0)
                {
                    row.Details[idx] = new DetailItem();
                    UpdateRoleAndStatus(row);
                    break;
                }
            }
        }

        private void DeleteTargetRow(TargetRow row)
        {
            if (row != null) TargetRows.Remove(row);
        }

    }
}
