using System.Collections.ObjectModel;
using System.Windows;
using DevExpress.Mvvm;
using WpfGridFifoPrototype.Models;

namespace WpfGridFifoPrototype.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            TargetGrid = new TargetGridViewModel();
            SourceItems = new ObservableCollection<UserAnalItemData>
            {
                new UserAnalItemData
                {
                    QualifiedName = "Plant.MainEngine.Temp",
                    ObjectName = "MainEngine",
                    AttributeName = "Temp",
                    ObjectLabel = "메인 엔진",
                    AttributeLabel = "온도",
                    Unit = "C"
                },
                new UserAnalItemData
                {
                    QualifiedName = "Plant.MainEngine.Pressure",
                    ObjectName = "MainEngine",
                    AttributeName = "Pressure",
                    ObjectLabel = "메인 엔진",
                    AttributeLabel = "압력",
                    Unit = "bar"
                },
                new UserAnalItemData
                {
                    QualifiedName = "Plant.Tank.Level",
                    ObjectName = "Tank",
                    AttributeName = "Level",
                    ObjectLabel = "탱크",
                    AttributeLabel = "레벨",
                    Unit = "%"
                },
                new UserAnalItemData
                {
                    QualifiedName = "Plant.Altitude.Sensor",
                    ObjectName = "Altitude",
                    AttributeName = "Sensor",
                    ObjectLabel = "고도 센서",
                    AttributeLabel = "측정값",
                    Unit = "m"
                }
            };
        }

        public ObservableCollection<int> ModeOptions { get; } = new ObservableCollection<int> { 2, 3 };

        public ObservableCollection<UserAnalItemData> SourceItems { get; }

        public TargetGridViewModel TargetGrid { get; }

        public DelegateCommand<UserAnalItemData> AddToTargetCommand => new DelegateCommand<UserAnalItemData>(AddToTarget);

        public DelegateCommand ValidateCommand => new DelegateCommand(Validate);

        private void AddToTarget(UserAnalItemData source)
        {
            TargetGrid.AddToTarget(source);
        }

        private void Validate()
        {
            if (TargetGrid.ValidateAssignments(out var errorMessage))
            {
                MessageBox.Show("모든 축이 선택되었습니다.", "확인", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show(errorMessage, "검증 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
