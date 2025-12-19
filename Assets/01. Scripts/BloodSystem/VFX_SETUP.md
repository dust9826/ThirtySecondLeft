# VFX Graph 피 파티클 설정 가이드

이 문서는 Blood System과 함께 사용할 VFX Graph 피 파티클을 만드는 방법을 설명합니다.

## VFX Graph 패키지 설치

1. Window > Package Manager 열기
2. "Visual Effect Graph" 검색 및 설치
3. Unity 재시작

## 피 파티클 VFX Graph 만들기

### 1. VFX Graph 에셋 생성

1. `Assets/BloodSystem/VFX/` 폴더에서 우클릭
2. Create > Visual Effects > Visual Effect Graph
3. 이름을 `BloodParticles.vfx`로 변경

### 2. VFX Graph 설정

VFX Graph를 더블클릭하여 에디터를 엽니다.

#### System 구성

**Initialize Particle (파티클 초기화)**
- **Capacity**: 100-200 (최대 파티클 개수)
- **Bounds**: Manual, Size (2, 2, 2) (파티클이 생성될 영역)
- **Set Lifetime Random**: Min 0.5, Max 1.5 (각 파티클이 0.5~1.5초 동안 살아있음)
- **Set Velocity**: Force 연결 (아래 Exposed Properties 참조)
  - 또는 **Set Velocity Random**: A = Force * 0.8, B = Force * 1.2

**Spawn (파티클 생성) - 중요!**

Spawn 블록 설정:
1. Spawn 컨텍스트 우클릭 → **"Set Rate"** → **"Burst"** 선택
   - **주의**: "Constant"가 아니라 **"Burst"**여야 합니다!
2. Burst 설정:
   - **Count**: Random 모드
     - Min: 80
     - Max: 120
   - **Delay**: 0
3. Spawn 컨텍스트 Inspector:
   - **Loop**: Off (체크 해제) ← 한 번만 터지고 끝
   - **Loop Duration**: 1 (무시됨, Loop가 꺼져 있으므로)

**문제 해결**: 파티클이 1개만 나온다면?
→ Rate가 "Constant"로 되어 있을 가능성이 높습니다. **"Burst"**로 변경하세요!

**Update Particle (파티클 업데이트 - 매 프레임)**
- **Gravity**: (0, -9.8, 0) - 중력 (아래로 떨어짐)
- **Linear Drag**: 2.0 - 공기 저항 (시간에 따라 느려짐)
  - 0 = 저항 없음, 3+ = 빠르게 멈춤
- **Age**: 자동으로 증가 (파티클의 나이)
  - Age >= Lifetime이 되면 파티클 소멸

**Output Particle Quad (렌더링)**
- **Blend Mode**: Additive (발광 효과) 또는 Alpha (반투명)
- **Size Random**: Min 0.05, Max 0.2
- **Color over Lifetime**:
  - 0% (태어났을 때): 밝은 빨강 (0.6, 0.05, 0.05, 1)
  - 100% (죽을 때): 어두운 빨강 (0.3, 0.02, 0.02, 0) ← 알파 0으로 페이드아웃

### 3. Force 적용하기 (중요!)

BloodEmitter가 충돌 힘을 VFX에 전달하려면 Exposed Property를 만들어야 합니다.

**Step 1: Blackboard에서 Force 프로퍼티 만들기**
1. VFX Graph 에디터 왼쪽 **Blackboard** 패널 열기
2. **"+"** 버튼 클릭
3. **Vector3** 선택
4. 이름을 **"Force"**로 변경
5. **"Exposed"** 체크박스 ON (중요!)
6. 기본값: (1, 1, 0) 정도

**Step 2: Initialize Particle에 Force 연결**
1. **Initialize Particle** 컨텍스트 선택
2. 우클릭 → **"Create Block"**
3. **"Set Velocity"** 검색 및 추가
4. Velocity 필드 옆 **작은 동그라미** 클릭
5. Blackboard에서 **"Force"** 를 드래그해서 연결

이제 C# 코드(BloodEmitter.cs:98)에서 `visualEffect.SetVector3("Force", force);`로 힘을 전달할 수 있습니다!

**추가 옵션 (선택사항)**
- `ParticleCount` (int): 파티클 개수를 런타임에 조절

### 4. VFX Prefab 만들기

1. 씬에 빈 GameObject 생성
2. Visual Effect 컴포넌트 추가
3. Asset Template에 `BloodParticles.vfx` 할당
4. Prefab으로 저장: `Assets/BloodSystem/VFX/BloodParticles.prefab`

## Particle System 대안 (간단한 방법)

VFX Graph를 사용하지 않으려면 Particle System을 사용할 수 있습니다:

### Particle System 설정

1. 빈 GameObject 생성 → 이름: `BloodParticles`
2. Particle System 컴포넌트 추가
3. 다음과 같이 설정:

**Main**
- Duration: 0.5
- Looping: OFF
- Start Lifetime: 0.3 ~ 0.8
- Start Speed: 2 ~ 5
- Start Size: 0.05 ~ 0.15
- Start Color: 빨강 (진함)
- Gravity Modifier: 1
- Max Particles: 100

**Emission**
- Rate over Time: 0
- Bursts: Time 0, Count 80-120

**Shape**
- Shape: Cone
- Angle: 30
- Radius: 0.1
- Emit from: Base

**Color over Lifetime**
- 그라디언트: 밝은 빨강 → 어두운 빨강 → 투명

**Size over Lifetime**
- 커브: 1.0 → 0.0 (점점 작아짐)

**Renderer**
- Render Mode: Billboard
- Material: Default Particle Material (또는 커스텀 피 파티클 머티리얼)

4. Prefab으로 저장

## BloodEmitter에 연결

1. 적 GameObject에 `BloodEmitter` 컴포넌트 추가
2. Blood VFX Prefab 필드에 위에서 만든 Prefab 할당
3. 나머지 설정 조정

## 테스트

1. 적이 벽/바닥에 충돌하는 상황 만들기
2. `BloodEmitter.EmitFromCollision(collision)` 호출
3. 피 파티클이 정상적으로 생성되는지 확인

## 최적화 팁

- VFX Graph는 GPU 파티클이므로 100개 이상도 성능 문제 없음
- Particle System은 CPU 파티클이므로 50개 이하 권장
- 파티클은 충돌 감지 없이 시각 전용으로 사용 (충돌은 BloodRaycaster가 처리)
- Auto-destroy를 위해 파티클 Lifetime을 짧게 유지 (1초 이하)
