# WpfGridFifoPrototype

WPF 및 DevExpress GridControl을 활용한 다차원(2D/3D) FIFO 데이터 모니터링 프로토타입입니다.

## 🚀 주요 기능

### 1. 다차원 모니터링 모드 지원
- **2D 모드**: X, Y 축 데이터를 관리합니다.
- **3D 모드**: X, Y, Z 축 데이터를 관리하며, UI가 동적으로 확장됩니다.
- 상단 콤보박스를 통해 실시간으로 모드를 전환할 수 있습니다.

### 2. 범용 FIFO(First-In, First-Out) 알고리즘
- 특정 로우가 가득 찼을 때 새 데이터를 추가하면, 가장 오래된 데이터(상단)를 밀어내고 새로운 데이터를 마지막 슬롯에 추가합니다.
- 슬롯 개수(Dimension)에 관계없이 동작하는 유연한 구조입니다.

### 3. 지능형 슬롯 이동 및 관리
- **상하 이동**: ▲/▼ 버튼을 통해 슬롯 간 데이터를 스왑할 수 있습니다.
- **위치 인식형 버튼**: 최상단 슬롯은 아래로만, 최하단 슬롯은 위로만, 중간 슬롯은 양방향 이동 버튼이 노출됩니다.
- **개별 삭제**: 각 슬롯의 '✕' 버튼으로 데이터를 비울 수 있습니다.

### 4. 동적 UI 구성
- `ItemsControl`을 사용하여 데이터 모델의 차원 설정에 따라 슬롯 UI를 자동 생성합니다.
- X(Blue), Y(Red), Z(Green) 각 역할별 고유 테마 컬러가 적용됩니다.

## 🛠 기술 스택
- .NET Framework 4.7.2
- DevExpress WPF UI Controls
- MVVM Pattern (DevExpress MVVM Framework)

## 📦 시작하기

1. 리포지토리를 클론합니다.
   ```bash
   git clone https://github.com/Peace-Min/WpfGridFifoPrototype.git
   ```
2. Visual Studio에서 `.sln` 파일을 엽니다.
3. NuGet 패키지(DevExpress 포함)를 복원합니다.
4. 빌드 후 실행합니다.

---
본 프로젝트는 사용자의 요구사항에 맞춰 독자적으로 설계 및 구현되었습니다.
