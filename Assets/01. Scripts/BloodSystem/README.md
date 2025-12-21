# Unity 2D URP Blood System

Unity 6.3 LTS + URP 2D 환경을 위한 피 시스템입니다. 적이 벽/바닥에 충돌하여 죽을 때 피가 터지며 환경 오브젝트에 영구적으로 피가 묻습니다.

## 주요 기능

- **실시간 피 렌더링**: 스프라이트와 타일맵에 실시간으로 피가 묻습니다
- **URP 2D Light 호환**: 2D 라이팅과 완벽하게 통합됩니다
- **젖은 효과**: Specular + Fresnel로 피의 젖은 느낌 표현
- **영구적 피**: 피는 영구적으로 남으며 수동으로 초기화 가능
- **확장 가능**: IBloodable 인터페이스로 모든 오브젝트에 적용 가능
- **고성능**: R8 RenderTexture + PropertyBlock으로 메모리 효율적

## 폴더 구조

```
Assets/BloodSystem/
├── Shaders/              # 셰이더 파일
│   ├── Blood_Sprite.shader
│   ├── Blood_Tilemap.shader
│   └── SplatBlit.shader
├── Scripts/              # C# 스크립트
│   ├── IBloodable.cs
│   ├── BloodManager.cs
│   ├── BloodableSprite.cs
│   ├── BloodableTilemap.cs
│   ├── BloodRaycaster.cs
│   └── BloodEmitter.cs
├── Editor/               # 에디터 도구
│   └── SplatTextureGenerator.cs
├── VFX/                  # VFX 파티클 (사용자가 만듦)
├── Textures/Splatters/   # 스플래터 텍스처
└── Materials/            # 머티리얼
```

## 빠른 시작

### 1. 초기 설정

#### A. 스플래터 텍스처 생성
1. Unity 메뉴: `Tools > Blood System > Generate Splatter Textures`
2. 텍스처 4개가 `Assets/BloodSystem/Textures/Splatters/`에 생성됩니다

#### B. 머티리얼 생성

**스프라이트용 머티리얼**
1. `Assets/BloodSystem/Materials/`에 새 Material 생성
2. 이름: `BloodSpriteMaterial`
3. Shader: `BloodSystem/Blood_Sprite` 선택
4. Blood Color: (0.4, 0.05, 0.05, 1) - 진한 빨강

**타일맵용 머티리얼**
1. 새 Material 생성
2. 이름: `BloodTilemapMaterial`
3. Shader: `BloodSystem/Blood_Tilemap` 선택
4. Blood Color: (0.4, 0.05, 0.05, 1)

> **참고**: SplatBlit 머티리얼은 만들 필요 없습니다. BloodManager가 런타임에 자동으로 생성합니다.

#### C. BloodManager 설정
1. 빈 GameObject 생성 → 이름: `BloodManager`
2. `BloodManager` 컴포넌트 추가
3. 설정:
   - Splat Textures: 생성된 스플래터 텍스처들 할당 (배열)
   - Splat Blit Shader: `SplatBlit` 셰이더 할당

### 2. 오브젝트에 적용

#### 스프라이트에 피 적용
1. 피가 묻을 스프라이트 GameObject 선택 (예: 벽, 바닥)
2. `BloodableSprite` 컴포넌트 추가
3. Blood Material: `BloodSpriteMaterial` 할당

#### 타일맵에 피 적용
1. 타일맵 GameObject 선택
2. `BloodableTilemap` 컴포넌트 추가
3. Blood Material: `BloodTilemapMaterial` 할당
4. Pixels Per Unit: 16 (타일 해상도에 맞게 조정)

#### 적 오브젝트 설정
1. 적 GameObject 선택
2. `BloodEmitter` 컴포넌트 추가
3. 설정:
   - Blood VFX Prefab: VFX Graph 또는 Particle System Prefab 할당 (선택사항)
   - Immediate Splat Size: 0.5
   - Ray Count: 10
   - Raycast Splat Size: 0.3
   - Raycast Layer Mask: 피가 묻을 레이어 선택

### 3. 코드에서 사용

```csharp
using BloodSystem;

// 적이 충돌하여 죽을 때
private void OnCollisionEnter2D(Collision2D collision)
{
    if (shouldDie)
    {
        BloodEmitter emitter = GetComponent<BloodEmitter>();
        if (emitter != null)
        {
            // 방법 1: Collision2D 직접 전달
            emitter.EmitFromCollision(collision);

            // 방법 2: 수동으로 설정
            ContactPoint2D contact = collision.GetContact(0);
            float force = collision.relativeVelocity.magnitude;
            emitter.EmitFromContact(contact, force);

            // 방법 3: 완전 수동
            Vector2 contactPoint = contact.point;
            Vector2 impactForce = -contact.normal * force;
            emitter.Emit(contactPoint, impactForce);
        }

        Destroy(gameObject);
    }
}
```

### 4. VFX 파티클 설정 (선택사항)

VFX Graph 또는 Particle System으로 피 파티클을 만들어 시각 효과를 추가할 수 있습니다.
자세한 내용은 `VFX_SETUP.md`를 참조하세요.

## 시스템 동작 원리

### 피 효과 발생 흐름
1. `BloodEmitter.Emit()` 호출
2. 3가지 동시 처리:
   - **즉각 스플래터**: 충돌 지점에 큰 피 자국
   - **VFX 파티클**: 시각 전용 파티클 (100개+)
   - **Blood Raycast**: 5~15개 Ray로 피 묻을 위치 결정

### Raycast 방향 분포
- **부채꼴 60~70%**: 충돌 힘 반대 방향 ±60° 범위
- **랜덤 30~40%**: 360° 전체 중 랜덤
- Ray 길이는 충돌 힘에 비례

### 피 렌더링
- 각 IBloodable 오브젝트는 자체 RenderTexture(R8 포맷) 보유
- PropertyBlock으로 머티리얼 인스턴싱
- 셰이더에서 원본 색상과 피 색상을 블렌딩
- Specular + Fresnel로 젖은 느낌 표현

## 고급 기능

### 수동으로 피 추가
```csharp
// 특정 위치에 피 추가
BloodManager.Instance.AddBloodAtPoint(worldPosition, size: 0.5f);

// 회전 및 텍스처 지정
BloodManager.Instance.AddBloodAtPoint(
    worldPosition,
    size: 0.5f,
    rotation: Mathf.PI / 4,
    splatIndex: 2
);
```

### 피 초기화
```csharp
// 특정 오브젝트의 피만 제거
BloodableSprite bloodable = wall.GetComponent<BloodableSprite>();
bloodable.ClearBlood();

// 모든 피 제거
BloodManager.Instance.ClearAllBlood();
```

### 커스텀 IBloodable 구현
```csharp
using BloodSystem;

public class CustomBloodable : MonoBehaviour, IBloodable
{
    public void AddBlood(Vector2 worldPos, Texture2D splatTexture, float size, float rotation)
    {
        // 피 추가 로직
    }

    public bool ContainsWorldPoint(Vector2 worldPos)
    {
        // 영역 체크
        return bounds.Contains(worldPos);
    }

    public RenderTexture GetBloodMaskRT() => bloodMaskRT;
    public void ClearBlood() { /* 초기화 로직 */ }
    public Bounds GetWorldBounds() => bounds;
}
```

## 성능 최적화

- **RenderTexture 해상도**: 픽셀아트는 1:1 매핑으로 충분
- **Raycast 개수**: 5~15개 권장 (더 많으면 과도한 피)
- **VFX 파티클**: VFX Graph(GPU) 사용 시 100개+ 가능, Particle System(CPU)은 50개 이하 권장
- **PropertyBlock**: 머티리얼 인스턴싱으로 Draw Call 최소화
- **R8 포맷**: 피 마스크는 흑백이므로 R8로 메모리 75% 절감

## 문제 해결

### 피가 안 보여요
- BloodManager가 씬에 있는지 확인
- 스플래터 텍스처가 할당되어 있는지 확인
- 머티리얼 셰이더가 올바른지 확인 (Blood_Sprite 또는 Blood_Tilemap)
- Raycast Layer Mask가 올바른지 확인

### 피가 이상한 위치에 생겨요
- Atlas UV 문제: SpriteUVMin/Max가 올바르게 설정되었는지 확인
- Tilemap Bounds 문제: Gizmo로 Bounds 확인

### 성능이 느려요
- RenderTexture 해상도 낮추기 (BloodableTilemap의 pixelsPerUnit 감소)
- Raycast 개수 줄이기
- VFX 파티클 개수 줄이기

## 라이선스

이 시스템은 ThirtySecondLeft 프로젝트용으로 제작되었습니다.
자유롭게 수정 및 확장하여 사용하세요.
