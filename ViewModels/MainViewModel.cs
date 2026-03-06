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
        
        // 차원 선택 옵션을 뷰모델에서 관리 (2차원, 3차원 한정)
        public ObservableCollection<int> ModeOptions { get; } = new ObservableCollection<int> { 2, 3 };

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

            SourceItems.Add(new SourceItem { Id = 1, Name = "PL-10", Attr = "Main Engine Temp" });
            SourceItems.Add(new SourceItem { Id = 2, Name = "PL-20", Attr = "Fuel Pressure" });
            SourceItems.Add(new SourceItem { Id = 3, Name = "AG-50", Attr = "Altitude Sensor" });

            // 초기 뷰 설정: 시작 시에는 두 개의 로우(그룹)를 기본으로 둠
            AddTargetRow(1, "Main Monitor", "Active");
            AddTargetRow(2, "Sub Monitor", "Standby");

            AddToTargetCommand = new DelegateCommand<SourceItem>(AddToTarget);
            RemoveDetailCommand = new DelegateCommand<DetailItem>(RemoveDetail);
            AddTargetRowCommand = new DelegateCommand(() => AddTargetRow()); // 인자 없는 오버로드 호출
            DeleteTargetRowCommand = new DelegateCommand<TargetRow>(DeleteTargetRow);
            MoveDetailUpCommand = new DelegateCommand<DetailItem>(MoveDetailUp);
            MoveDetailDownCommand = new DelegateCommand<DetailItem>(MoveDetailDown);
        }

        private void OnDimensionModeChanged()
        {
            // [기획 반영] 차원(Dimension)이 변경될 때마다 혼선 방지를 위해 TargetRows 전체 클리어(초기화)
            TargetRows.Clear();
            SelectedTargetRow = null;

            // 전체 클리어 후, 사용자가 바로 다시 추가할 수 있도록 빈 그룹 1개 기본 생성
            AddTargetRow();
        }

        /// <summary>
        /// 새로운 빈 타겟 로우를 추가할 때 사용
        /// </summary>
        private void AddTargetRow()
        {
            int nextNo = TargetRows.Any() ? TargetRows.Max(r => r.No) + 1 : 1;
            AddTargetRow(nextNo, $"New Group {nextNo}", "Standby");
        }

        /// <summary>
        /// 지정된 속성으로 타겟 로우를 생성하여 추가
        /// </summary>
        private void AddTargetRow(int no, string label, string color)
        {
            var row = new TargetRow { No = no, Label = label, Color = color };
            InitializeSlots(row);
            TargetRows.Add(row);
            
            // 새 그룹 추가 시 해당 그룹으로 자동 포커스
            SelectedTargetRow = row;
        }

        private void InitializeSlots(TargetRow row)
        {
            // 현재 설정된 차원(DimensionMode)만큼 슬롯을 생성
            for (int i = 0; i < DimensionMode; i++)
            {
                row.Details.Add(new DetailItem());
            }
            UpdateRoleAndStatus(row);
        }

        private void UpdateRoleAndStatus(TargetRow row)
        {
            // 최대 3차원 (X, Y, Z) 기반 확정 배열
            string[] labels = { "X", "Y", "Z" }; 
            for (int i = 0; i < row.Details.Count; i++)
            {
                row.Details[i].RoleLabel = labels[i]; // DimensionMode가 최대 3이므로 IndexError 발생 안함
                row.Details[i].CanMoveUp = i > 0;
                row.Details[i].CanMoveDown = i < row.Details.Count - 1;
            }
        }

        private void AddToTarget(SourceItem source)
        {
            if (source == null) return;
            
            // 엣지 케이스 방어: 타겟 로우 미존재 시 피드백 제공
            if (SelectedTargetRow == null)
            {
                MessageBox.Show("데이터를 추가할 타겟(모니터링 대상) 그룹이 없습니다.\n'로우 추가' 버튼을 눌러 그룹을 먼저 생성해주세요.", 
                                "경고", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var details = SelectedTargetRow.Details;
            int emptyIdx = -1;
            for (int i = 0; i < details.Count; i++)
            {
                if (details[i].IsEmpty) { emptyIdx = i; break; }
            }

            if (emptyIdx != -1)
            {
                // 인스턴스를 갈아끼우지 않고 내부 속성 교체로 MVVM 성능 유지
                details[emptyIdx].Y = source.Name;
                details[emptyIdx].Attr = source.Attr;
            }
            else
            {
                // FIFO 처리 (인스턴스 유지 후 데이터 Shift Up 처리)
                for (int i = 0; i < details.Count - 1; i++)
                {
                    details[i].AssignFrom(details[i + 1]);
                }
                details[details.Count - 1].Y = source.Name;
                details[details.Count - 1].Attr = source.Attr;
            }
            UpdateRoleAndStatus(SelectedTargetRow);
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
                var targetSlot = row.Details[newIdx];

                // 새로운 인스턴스를 만들지 않고 내부 데이터만 안전하게 교환(Swap)
                string tempY = targetSlot.Y;
                string tempAttr = targetSlot.Attr;

                targetSlot.Y = item.Y;
                targetSlot.Attr = item.Attr;

                item.Y = tempY;
                item.Attr = tempAttr;
            }
        }

        private void MoveDetailUp(DetailItem item) => MoveItem(item, -1);
        private void MoveDetailDown(DetailItem item) => MoveItem(item, 1);

        private void RemoveDetail(DetailItem detail)
        {
            if (detail == null) return;
            // 인스턴스 덮어씌움 방지 - 값만 Clear 처리
            detail.ClearContent();
        }

        private void DeleteTargetRow(TargetRow row)
        {
            if (row != null) 
            {
                TargetRows.Remove(row);
                // 엣지 케이스 방어: 삭제 후 미아현상을 막기 위해 첫 번째 항목(없으면 null) 수동 포커싱 
                SelectedTargetRow = TargetRows.FirstOrDefault();
            }
        }
    }
}
