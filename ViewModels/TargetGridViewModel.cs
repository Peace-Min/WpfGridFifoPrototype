using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DevExpress.Mvvm;
using WpfGridFifoPrototype.Models;
using WpfGridFifoPrototype.PrototypeSupport;

namespace WpfGridFifoPrototype.ViewModels
{
    public class TargetGridViewModel : ViewModelBase
    {
        #region Construction
        private readonly ChartColorService _chartColorService = new ChartColorService();
        private readonly string[] _roleLabels = { "X", "Y", "Z" };
        private readonly ObservableCollection<DetailItem> _selectedDetails = new ObservableCollection<DetailItem>();
        private bool _isUpdatingSelection;
        private int _dimensionMode = 2;

        public TargetGridViewModel()
        {
            InitializeCommands();
            InitializeState();
            AddTargetRow();
        }
        #endregion

        #region State
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
                    SetSelectedDetailCore(null);
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

                if (_isUpdatingSelection || value == null)
                    return;

                _isUpdatingSelection = true;
                try
                {
                    var ownerRow = FindOwnerRow(value);
                    if (ownerRow != null)
                        SelectedTargetRow = ownerRow;
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
            private set => SetValue(value);
        }
        
        public DelegateCommand AddTargetRowCommand { get; private set; }

        public DelegateCommand RemoveSelectedTargetRowCommand { get; private set; }

        public DelegateCommand ClearSelectedDetailCommand { get; private set; }

        public DelegateCommand<TargetRow> DeleteTargetRowCommand { get; private set; }

        public DelegateCommand<DetailItem> RemoveDetailCommand { get; private set; }

        public DelegateCommand<DetailItem> MoveDetailUpCommand { get; private set; }

        public DelegateCommand<DetailItem> MoveDetailDownCommand { get; private set; }

        public DelegateCommand<DetailItem> SelectDetailCommand { get; private set; }
        #endregion

        #region Public API
        public void AddTargetRow()
        {
            int nextNo = TargetRows.Any() ? TargetRows.Max(targetRow => targetRow.No) + 1 : 1;
            string groupTitle = $"New Group {nextNo}";
            var row = CreateTargetRow(nextNo, groupTitle);

            TargetRows.Add(row);
            SelectTargetRow(row);
        }

        public void UpdateTargetRow(
            TargetRow row,
            string groupTitle = null,
            Color? color = null,
            LineSeriesType? lineSeriesType = null)
        {
            var targetRow = row ?? SelectedTargetRow;
            if (targetRow == null || !TargetRows.Contains(targetRow))
                return;

            if (groupTitle != null)
                targetRow.GroupTitle = groupTitle;

            if (color.HasValue)
                targetRow.Color = color.Value;

            if (lineSeriesType.HasValue)
                targetRow.LineSeriesType = lineSeriesType.Value;
        }

        public void RemoveSelectedTargetRow()
        {
            var rowsToRemove = _selectedDetails
                .Select(FindOwnerRow)
                .Where(row => row != null)
                .Distinct()
                .ToList();

            if (rowsToRemove.Count == 0 && SelectedTargetRow != null)
                rowsToRemove.Add(SelectedTargetRow);

            foreach (var row in rowsToRemove)
                RemoveTargetRow(row);

            ClearSelectedDetailsCache();
        }

        public void RemoveTargetRow(TargetRow row)
        {
            if (row == null || !TargetRows.Contains(row))
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

        public void SelectTargetRow(TargetRow row)
        {
            if (row == null || !TargetRows.Contains(row))
                return;

            SelectedTargetRow = row;
        }

        public void AddToTarget(UserAnalItemData source)
        {
            AddOrReplaceDetailFromSource(source);
        }

        public void UpdateDetail(DetailItem detail, UserAnalItemData source)
        {
            if (detail == null || source == null)
                return;

            detail.UpdateFromSource(source);

            var ownerRow = FindOwnerRow(detail);
            if (ownerRow != null)
                UpdateRoleAndStatus(ownerRow);
        }

        public void RemoveSelectedDetail()
        {
            ClearDetailCore(SelectedDetail, clearSelectionAfter: true);
        }

        public void RemoveDetail(DetailItem detail)
        {
            ClearDetailCore(detail, clearSelectionAfter: ReferenceEquals(SelectedDetail, detail));
        }

        public void MoveDetail(DetailItem detail, int direction)
        {
            MoveDetailCore(detail, direction);
        }

        public void SelectDetail(DetailItem detail)
        {
            if (detail == null)
                return;

            if (RemoveSelectedDetailFromCache(detail))
            {
                if (ReferenceEquals(SelectedDetail, detail))
                    SetSelectedDetailCore(_selectedDetails.LastOrDefault());

                return;
            }

            AddSelectedDetailToCache(detail);
            SelectedDetail = detail;
        }

        public void ClearSelectedDetail()
        {
            ClearSelectedDetailsCache();
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

                    errorMsg = $"{row.GroupTitle} : {detail.RoleLabel} not selected";
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
        #endregion

        #region Internal Logic
        private void InitializeCommands()
        {
            AddTargetRowCommand = new DelegateCommand(AddTargetRow);
            RemoveSelectedTargetRowCommand = new DelegateCommand(RemoveSelectedTargetRow);
            ClearSelectedDetailCommand = new DelegateCommand(ClearSelectedDetail);
            DeleteTargetRowCommand = new DelegateCommand<TargetRow>(DeleteTargetRow);
            RemoveDetailCommand = new DelegateCommand<DetailItem>(RemoveDetail);
            MoveDetailUpCommand = new DelegateCommand<DetailItem>(MoveDetailUp);
            MoveDetailDownCommand = new DelegateCommand<DetailItem>(MoveDetailDown);
            SelectDetailCommand = new DelegateCommand<DetailItem>(SelectDetail);
        }

        private void InitializeState()
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

        private void DeleteTargetRow(TargetRow row)
        {
            RemoveTargetRow(row);
        }

        private void MoveDetailUp(DetailItem detail)
        {
            MoveDetail(detail, -1);
        }

        private void MoveDetailDown(DetailItem detail)
        {
            MoveDetail(detail, 1);
        }

        private TargetRow CreateTargetRow(int no, string groupTitle)
        {
            var row = new TargetRow
            {
                No = no,
                GroupTitle = groupTitle,
                Color = _chartColorService.GenerateUniqueColor(TargetRows),
                LineSeriesType = LineSeriesType.Line
            };

            InitializeSlots(row);
            return row;
        }

        private void InitializeSlots(TargetRow row)
        {
            for (int index = 0; index < DimensionMode; index++)
                row.Details.Add(new DetailItem());

            UpdateRoleAndStatus(row);
        }

        private void UpdateRoleAndStatus(TargetRow row)
        {
            for (int index = 0; index < row.Details.Count; index++)
            {
                row.Details[index].RoleLabel = index < _roleLabels.Length ? _roleLabels[index] : $"D{index + 1}";
                row.Details[index].CanMoveUp = index > 0;
                row.Details[index].CanMoveDown = index < row.Details.Count - 1;
            }
        }

        private void AddOrReplaceDetailFromSource(UserAnalItemData source)
        {
            if (source == null)
                return;

            if (SelectedTargetRow == null)
            {
                MessageBox.Show(
                    "No target row is selected. Add a row before assigning data.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var details = SelectedTargetRow.Details;
            bool alreadyExists = details.Any(item => !string.IsNullOrEmpty(item.QualifiedName) && item.QualifiedName == source.QualifiedName);
            if (alreadyExists)
                return;

            var targetSlot = ResolveTargetSlotForAdd(details);
            if (targetSlot == null)
                return;

            targetSlot.UpdateFromSource(source);
            UpdateRoleAndStatus(SelectedTargetRow);
            ApplySelectionToRow(SelectedTargetRow, targetSlot);
        }

        private void ClearDetailCore(DetailItem detail, bool clearSelectionAfter)
        {
            if (detail == null)
                return;

            var ownerRow = FindOwnerRow(detail);
            var selectionSnapshot = CaptureSelectionSnapshot(ownerRow);

            detail.Clear();

            if (!selectionSnapshot.ShouldApply)
                return;

            var nextDetail = clearSelectionAfter ? null : selectionSnapshot.SelectedDetailInRow;
            ApplySelectionToRow(ownerRow, nextDetail);
        }

        private void MoveDetailCore(DetailItem detail, int direction)
        {
            if (detail == null)
                return;

            var ownerRow = FindOwnerRow(detail);
            if (ownerRow == null)
                return;

            int oldIndex = ownerRow.Details.IndexOf(detail);
            int newIndex = oldIndex + direction;
            if (newIndex < 0 || newIndex >= ownerRow.Details.Count)
                return;

            var targetSlot = ownerRow.Details[newIndex];
            var selectionSnapshot = CaptureSelectionSnapshot(ownerRow);
            var nextDetail = ResolveSelectedDetailAfterSwap(selectionSnapshot.SelectedDetailInRow, detail, targetSlot);

            detail.SwapPayloadWith(targetSlot);

            if (!selectionSnapshot.ShouldApply)
                return;

            ApplySelectionToRow(ownerRow, nextDetail);
        }

        private DetailItem ResolveTargetSlotForAdd(IList<DetailItem> details)
        {
            if (details == null || details.Count == 0)
                return null;

            var selectedSlot = SelectedDetail != null && details.Contains(SelectedDetail)
                ? SelectedDetail
                : null;

            if (selectedSlot != null && selectedSlot.IsEmpty)
                return selectedSlot;

            var emptySlot = details.FirstOrDefault(detail => detail.IsEmpty);
            if (emptySlot != null)
                return emptySlot;

            if (selectedSlot != null)
                return selectedSlot;

            return details
                .OrderBy(detail => detail.InsertTimestamp)
                .FirstOrDefault();
        }

        private SelectionSnapshot CaptureSelectionSnapshot(TargetRow row)
        {
            if (row == null)
                return SelectionSnapshot.None;

            bool hasSelectedDetailInRow = SelectedDetail != null && ReferenceEquals(FindOwnerRow(SelectedDetail), row);
            if (hasSelectedDetailInRow)
                return new SelectionSnapshot(true, SelectedDetail);

            bool hasSelectedRowOnly = ReferenceEquals(SelectedTargetRow, row) && SelectedDetail == null;
            if (hasSelectedRowOnly)
                return new SelectionSnapshot(true, null);

            return SelectionSnapshot.None;
        }

        private DetailItem ResolveSelectedDetailAfterSwap(DetailItem currentSelection, DetailItem sourceSlot, DetailItem targetSlot)
        {
            if (currentSelection == null)
                return null;

            if (ReferenceEquals(currentSelection, sourceSlot))
                return targetSlot;

            if (ReferenceEquals(currentSelection, targetSlot))
                return sourceSlot;

            return currentSelection;
        }

        private void ApplySelectionToRow(TargetRow row, DetailItem detail)
        {
            if (row != null && !ReferenceEquals(SelectedTargetRow, row))
                SelectedTargetRow = row;

            if (detail == null)
            {
                SetSelectedDetailCore(null);
                return;
            }

            SelectedDetail = detail;
        }

        private void SetSelectedDetailCore(DetailItem detail)
        {
            SetValue(detail, nameof(SelectedDetail));
        }

        private void AddSelectedDetailToCache(DetailItem detail)
        {
            if (detail == null || _selectedDetails.Contains(detail))
                return;

            _selectedDetails.Add(detail);
            detail.IsSelected = true;
        }

        private bool RemoveSelectedDetailFromCache(DetailItem detail)
        {
            if (detail == null || !_selectedDetails.Remove(detail))
                return false;

            detail.IsSelected = false;
            return true;
        }

        private void ClearSelectedDetailsCache()
        {
            foreach (var detail in _selectedDetails.ToList())
                detail.IsSelected = false;

            _selectedDetails.Clear();
        }

        private TargetRow FindOwnerRow(DetailItem detail)
        {
            if (detail == null)
                return null;

            return TargetRows.FirstOrDefault(row => row.Details.Contains(detail));
        }
        #endregion

        private sealed class SelectionSnapshot
        {
            public static readonly SelectionSnapshot None = new SelectionSnapshot(false, null);

            public SelectionSnapshot(bool shouldApply, DetailItem selectedDetailInRow)
            {
                ShouldApply = shouldApply;
                SelectedDetailInRow = selectedDetailInRow;
            }

            public bool ShouldApply { get; }

            public DetailItem SelectedDetailInRow { get; }
        }
    }
}
