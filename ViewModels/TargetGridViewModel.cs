using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using WpfGridFifoPrototype.Models;

namespace WpfGridFifoPrototype.ViewModels
{
    public class TargetGridViewModel : ViewModelBase
    {
        private readonly ChartColorService _chartColorService = new ChartColorService();
        private readonly string[] _roleLabels = { "X", "Y", "Z" };
        private bool _isUpdatingSelection;
        private int _dimensionMode = 2;

        public TargetGridViewModel()
        {
            Initialize();
            AddTargetRow();
        }

        public int DimensionMode
        {
            get => _dimensionMode;
            set
            {
                if (!SetProperty(ref _dimensionMode, value, nameof(DimensionMode)))
                    return;

                ClearDisplay();
            }
        }

        public TargetRow SelectedTargetRow
        {
            get => GetValue<TargetRow>();
            set
            {
                if (ReferenceEquals(GetValue<TargetRow>(), value))
                    return;

                SetValue(value);

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

        public DetailItem SelectedDetail
        {
            get => GetValue<DetailItem>();
            set
            {
                if (ReferenceEquals(GetValue<DetailItem>(), value))
                    return;

                SetSelectedDetailCore(value);

                if (_isUpdatingSelection)
                    return;

                _isUpdatingSelection = true;
                try
                {
                    SelectedTargetRow = FindOwnerRow(value);
                }
                finally
                {
                    _isUpdatingSelection = false;
                }
            }
        }

        public List<KeyValuePair<LineSeriesType, string>> LineSeriesTypeList
        {
            get => GetValue<List<KeyValuePair<LineSeriesType, string>>>();
            private set => SetValue(value);
        }

        public ObservableCollection<TargetRow> TargetRows
        {
            get => GetValue<ObservableCollection<TargetRow>>();
            set => SetValue(value);
        }

        public DelegateCommand AddTargetRowCommand => new DelegateCommand(AddTargetRow);
        public DelegateCommand RemoveSelectedTargetRowCommand => new DelegateCommand(RemoveSelectedTargetRow);
        public DelegateCommand<TargetRow> DeleteTargetRowCommand => new DelegateCommand<TargetRow>(DeleteTargetRow);
        public DelegateCommand<DetailItem> RemoveDetailCommand => new DelegateCommand<DetailItem>(RemoveDetail);
        public DelegateCommand<DetailItem> MoveDetailUpCommand => new DelegateCommand<DetailItem>(MoveDetailUp);
        public DelegateCommand<DetailItem> MoveDetailDownCommand => new DelegateCommand<DetailItem>(MoveDetailDown);
        public DelegateCommand<DetailItem> SelectDetailCommand => new DelegateCommand<DetailItem>(SelectDetail);

        private void Initialize()
        {
            TargetRows = new ObservableCollection<TargetRow>();
            LineSeriesTypeList = Enum
                .GetValues(typeof(LineSeriesType))
                .Cast<LineSeriesType>()
                .Select(item => new KeyValuePair<LineSeriesType, string>(item, EnumUtil.GetDescription(item)))
                .ToList();
        }

        private void ClearDisplay()
        {
            TargetRows.Clear();
            SelectedTargetRow = null;
            SetSelectedDetailCore(null);
            AddTargetRow();
        }

        private void AddTargetRow(int no, string label)
        {
            var uniqueColor = _chartColorService.GenerateUniqueColor(TargetRows);
            var row = new TargetRow
            {
                No = no,
                GroupTitle = label,
                Color = uniqueColor,
                LineSeriesType = LineSeriesType.Line
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

        public void AddTargetRow()
        {
            int nextNo = TargetRows.Any() ? TargetRows.Max(r => r.No) + 1 : 1;
            AddTargetRow(nextNo, $"New Group {nextNo}");
        }

        public void AddToTarget(UserAnalItemData source)
        {
            if (source == null)
                return;

            if (SelectedTargetRow == null)
            {
                MessageBox.Show(
                    "데이터를 추가할 타겟 그룹이 없습니다. 먼저 로우를 추가하세요.",
                    "경고",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var details = SelectedTargetRow.Details;

            if (details.Any(item => !string.IsNullOrEmpty(item.QualifiedName) && item.QualifiedName == source.QualifiedName))
                return;

            DetailItem targetSlot = null;

            if (SelectedDetail != null && details.Contains(SelectedDetail))
                targetSlot = SelectedDetail;

            if (targetSlot == null || !targetSlot.IsEmpty)
                targetSlot = details.FirstOrDefault(detail => detail.IsEmpty);

            if (targetSlot == null)
                targetSlot = details.OrderBy(detail => detail.InsertTimestamp).First();

            targetSlot.UpdateFromSource(source);
            UpdateRoleAndStatus(SelectedTargetRow);
            SelectedDetail = targetSlot;
        }

        public void RemoveSelectedTargetRow()
        {
            if (SelectedTargetRow == null)
                return;

            TargetRows.Remove(SelectedTargetRow);
            SelectedTargetRow = TargetRows.FirstOrDefault();
            if (SelectedTargetRow == null)
                SetSelectedDetailCore(null);
        }

        public bool ValidateAssignments(out string errorMsg)
        {
            errorMsg = string.Empty;

            foreach (var row in TargetRows)
            {
                foreach (var detail in row.Details)
                {
                    if (!detail.IsEmpty)
                        continue;

                    errorMsg = $"{row.GroupTitle} : {detail.RoleLabel} 미 선택";
                    return false;
                }
            }

            return true;
        }

        public void LoadConfig(ChartComponentConfig source)
        {
            return;
        }

        public void SeriesColorChanged(object obj)
        {
            return;
        }

        private void DeleteTargetRow(TargetRow row)
        {
            if (row == null)
                return;

            bool removedSelectedRow = ReferenceEquals(SelectedTargetRow, row);
            bool removedSelectedDetail = SelectedDetail != null && row.Details.Contains(SelectedDetail);

            TargetRows.Remove(row);

            if (!removedSelectedRow && !removedSelectedDetail)
                return;

            SelectedTargetRow = TargetRows.FirstOrDefault();
            if (SelectedTargetRow == null)
                SetSelectedDetailCore(null);
        }

        private void RemoveDetail(DetailItem detail)
        {
            if (detail == null)
                return;

            SelectedDetail = detail;
            detail.Clear();
        }

        private void MoveDetailUp(DetailItem item) => MoveItem(item, -1);

        private void MoveDetailDown(DetailItem item) => MoveItem(item, 1);

        private void MoveItem(DetailItem item, int direction)
        {
            if (item == null)
                return;

            var row = FindOwnerRow(item);
            if (row == null)
                return;

            int oldIndex = row.Details.IndexOf(item);
            int newIndex = oldIndex + direction;

            if (newIndex < 0 || newIndex >= row.Details.Count)
                return;

            var targetSlot = row.Details[newIndex];
            var sourceSnapshot = item.ToSnapshot();
            var targetSnapshot = targetSlot.ToSnapshot();

            targetSlot.ApplySnapshot(sourceSnapshot);
            item.ApplySnapshot(targetSnapshot);

            SelectedDetail = targetSlot;
        }

        private void SelectDetail(DetailItem detail)
        {
            if (detail == null)
                return;

            SelectedDetail = detail;
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
            var current = GetValue<DetailItem>();
            if (ReferenceEquals(current, detail))
                return;

            if (current != null)
                current.IsSelected = false;

            SetValue(detail, nameof(SelectedDetail));

            if (detail != null)
                detail.IsSelected = true;
        }

        private TargetRow FindOwnerRow(DetailItem detail)
        {
            if (detail == null)
                return null;

            return TargetRows.FirstOrDefault(row => row.Details.Contains(detail));
        }
    }
}
