using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using WpfGridFifoPrototype.Models;

namespace WpfGridFifoPrototype.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<SourceItem> SourceItems { get; }
        public ObservableCollection<TargetRow> TargetRows { get; }
        public ObservableCollection<int> ModeOptions { get; } = new ObservableCollection<int> { 2, 3 };

        private readonly string[] _roleLabels = { "X", "Y", "Z" };
        private bool _isUpdatingSelection;

        private TargetRow _selectedTargetRow;
        public TargetRow SelectedTargetRow
        {
            get => _selectedTargetRow;
            set
            {
                if (!SetProperty(ref _selectedTargetRow, value, nameof(SelectedTargetRow)))
                    return;

                if (_isUpdatingSelection)
                    return;

                _isUpdatingSelection = true;
                try
                {
                    SelectDefaultDetailForRow(value);
                }
                finally
                {
                    _isUpdatingSelection = false;
                }
            }
        }

        private DetailItem _selectedDetail;
        public DetailItem SelectedDetail
        {
            get => _selectedDetail;
            set
            {
                if (ReferenceEquals(_selectedDetail, value))
                    return;

                SetSelectedDetailCore(value);

                if (_isUpdatingSelection)
                    return;

                _isUpdatingSelection = true;
                try
                {
                    SelectedTargetRow = FindOwningRow(value);
                }
                finally
                {
                    _isUpdatingSelection = false;
                }
            }
        }

        public int SelectedDetailIndex =>
            SelectedTargetRow == null || SelectedDetail == null
                ? -1
                : SelectedTargetRow.Details.IndexOf(SelectedDetail);

        private int _dimensionMode = 2;
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
        public DelegateCommand<DetailItem> SelectDetailCommand { get; }
        public DelegateCommand SubmitOkCommand { get; }

        public MainViewModel()
        {
            SourceItems = new ObservableCollection<SourceItem>();
            TargetRows = new ObservableCollection<TargetRow>();

            SourceItems.Add(new SourceItem { Id = 1, Name = "PL-10", Attr = "Main Engine Temp" });
            SourceItems.Add(new SourceItem { Id = 2, Name = "PL-20", Attr = "Fuel Pressure" });
            SourceItems.Add(new SourceItem { Id = 3, Name = "AG-50", Attr = "Altitude Sensor" });

            AddTargetRow(1, "Main Monitor", "Active");
            AddTargetRow(2, "Sub Monitor", "Standby");

            AddToTargetCommand = new DelegateCommand<SourceItem>(AddToTarget);
            RemoveDetailCommand = new DelegateCommand<DetailItem>(RemoveDetail);
            AddTargetRowCommand = new DelegateCommand(AddTargetRow);
            DeleteTargetRowCommand = new DelegateCommand<TargetRow>(DeleteTargetRow);
            MoveDetailUpCommand = new DelegateCommand<DetailItem>(MoveDetailUp);
            MoveDetailDownCommand = new DelegateCommand<DetailItem>(MoveDetailDown);
            SelectDetailCommand = new DelegateCommand<DetailItem>(SelectDetail);
            SubmitOkCommand = new DelegateCommand(OnSubmitOk);
        }

        private void OnDimensionModeChanged()
        {
            TargetRows.Clear();
            SelectedTargetRow = null;
            SetSelectedDetailCore(null);
            AddTargetRow();
        }

        private void AddTargetRow()
        {
            int nextNo = TargetRows.Any() ? TargetRows.Max(r => r.No) + 1 : 1;
            AddTargetRow(nextNo, $"New Group {nextNo}", "Standby");
        }

        private void AddTargetRow(int no, string label, string color)
        {
            var row = new TargetRow
            {
                No = no,
                Label = label,
                Color = color
            };

            InitializeSlots(row);
            TargetRows.Add(row);
            SelectedTargetRow = row;
        }

        private void InitializeSlots(TargetRow row)
        {
            for (int i = 0; i < DimensionMode; i++)
                row.Details.Add(new DetailItem());

            UpdateRoleAndStatus(row);
        }

        private void UpdateRoleAndStatus(TargetRow row)
        {
            for (int i = 0; i < row.Details.Count; i++)
            {
                row.Details[i].RoleLabel = i < _roleLabels.Length ? _roleLabels[i] : $"D{i + 1}";
                row.Details[i].CanMoveUp = i > 0;
                row.Details[i].CanMoveDown = i < row.Details.Count - 1;
            }
        }

        private void AddToTarget(SourceItem source)
        {
            if (source == null)
                return;

            if (SelectedTargetRow == null)
            {
                MessageBox.Show(
                    "Select a target group before adding a source item.",
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var details = SelectedTargetRow.Details;
            DetailItem targetSlot = null;

            if (SelectedDetail != null && details.Contains(SelectedDetail))
                targetSlot = SelectedDetail;

            if (targetSlot == null)
                targetSlot = details.FirstOrDefault(d => d.IsEmpty);

            if (targetSlot == null)
                targetSlot = details.OrderBy(d => d.InsertTimestamp).First();

            UpdateSlot(targetSlot, source);
            UpdateRoleAndStatus(SelectedTargetRow);
            SelectedDetail = targetSlot;
        }

        private void UpdateSlot(DetailItem item, SourceItem source)
        {
            if (item.Y == source.Name && item.Attr == source.Attr)
                return;

            item.Y = source.Name;
            item.Attr = source.Attr;
            item.InsertTimestamp = DateTime.Now.Ticks;
        }

        private void MoveItem(DetailItem item, int direction)
        {
            if (item == null)
                return;

            var row = FindOwningRow(item);
            if (row == null)
                return;

            int oldIndex = row.Details.IndexOf(item);
            int newIndex = oldIndex + direction;

            if (newIndex < 0 || newIndex >= row.Details.Count)
                return;

            var targetSlot = row.Details[newIndex];

            string tempY = targetSlot.Y;
            string tempAttr = targetSlot.Attr;
            long tempTimestamp = targetSlot.InsertTimestamp;

            targetSlot.Y = item.Y;
            targetSlot.Attr = item.Attr;
            targetSlot.InsertTimestamp = item.InsertTimestamp;

            item.Y = tempY;
            item.Attr = tempAttr;
            item.InsertTimestamp = tempTimestamp;
        }

        private void MoveDetailUp(DetailItem item) => MoveItem(item, -1);

        private void MoveDetailDown(DetailItem item) => MoveItem(item, 1);

        private void SelectDetail(DetailItem detail)
        {
            if (detail == null)
                return;

            SelectedDetail = detail;
        }

        private void RemoveDetail(DetailItem detail)
        {
            if (detail == null)
                return;

            SelectedDetail = detail;
            detail.ClearContent();
        }

        private void DeleteTargetRow(TargetRow row)
        {
            if (row == null)
                return;

            bool removedSelectedRow = ReferenceEquals(SelectedTargetRow, row);
            bool removedSelectedDetail = SelectedDetail != null && row.Details.Contains(SelectedDetail);

            TargetRows.Remove(row);

            if (removedSelectedRow || removedSelectedDetail)
            {
                SelectedTargetRow = TargetRows.FirstOrDefault();
                if (SelectedTargetRow == null)
                    SetSelectedDetailCore(null);
            }
        }

        private void OnSubmitOk()
        {
            if (!ValidateAssignments())
                return;

            MessageBox.Show(
                "All assignments are complete.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private bool ValidateAssignments()
        {
            foreach (var row in TargetRows)
            {
                foreach (var detail in row.Details)
                {
                    if (!detail.IsEmpty)
                        continue;

                    MessageBox.Show(
                        $"{row.Label} : ({detail.RoleLabel}) is empty.",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void SelectDefaultDetailForRow(TargetRow row)
        {
            if (row == null)
            {
                SetSelectedDetailCore(null);
                return;
            }

            if (SelectedDetail != null && row.Details.Contains(SelectedDetail))
                return;

            SetSelectedDetailCore(row.Details.FirstOrDefault());
        }

        private void SetSelectedDetailCore(DetailItem detail)
        {
            if (ReferenceEquals(_selectedDetail, detail))
                return;

            if (_selectedDetail != null)
                _selectedDetail.IsSelected = false;

            _selectedDetail = detail;

            if (_selectedDetail != null)
                _selectedDetail.IsSelected = true;

            RaisePropertyChanged(nameof(SelectedDetail));
            RaisePropertyChanged(nameof(SelectedDetailIndex));
        }

        private TargetRow FindOwningRow(DetailItem detail)
        {
            if (detail == null)
                return null;

            return TargetRows.FirstOrDefault(row => row.Details.Contains(detail));
        }
    }
}
