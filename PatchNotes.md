# Helena-Engine Patch Notes

---

## v1.1.0 — Difficulty System

### Overview

난이도 조절 기능을 추가했습니다. 완전 초보자도 즐길 수 있는 **Martin** 수준부터 엔진 최대 강도인 **Maximum** 까지, 총 **11단계** 난이도를 제공합니다.

---

### 새 명령어: `difficulty`

```
difficulty [show | list | set <level> | <level>]
```

| 예시 | 설명 |
|------|------|
| `difficulty` | 현재 난이도 표시 |
| `difficulty show` | 현재 난이도 표시 |
| `difficulty list` | 전체 난이도 목록 표시 |
| `difficulty set martin` | 이름으로 설정 |
| `difficulty martin` | 단축 설정 (동일) |
| `difficulty 0` | 번호(0-10)로 설정 |
| `difficulty set 4` | 번호로 설정 (동일) |

이름은 대소문자를 구분하지 않습니다 (`MARTIN`, `Martin`, `martin` 모두 동작).

---

### 난이도 레벨 표

| 번호 | 이름 | 설명 | MaxN | Temperature |
|:----:|------|------|:----:|:-----------:|
| 0 | **Martin** | 완전 초보 (Chess.com Martin 수준) | 6 | 400 cp |
| 1 | **Beginner** | 입문 (~500 ELO) | 5 | 260 cp |
| 2 | **Novice** | 초보 (~800-1000 ELO) | 4 | 170 cp |
| 3 | **Intermediate** | 중급 (~1100-1300 ELO) | 3 | 110 cp |
| 4 | **Club** | 클럽 플레이어 (~1400-1600 ELO) | 3 | 70 cp |
| 5 | **Advanced** | 상급자 (~1700-1900 ELO) | 2 | 45 cp |
| 6 | **Expert** | 전문가 (~2000-2100 ELO) | 2 | 26 cp |
| 7 | **Candidate Master** | 후보 마스터 (~2200 ELO) | 1 | 15 cp |
| 8 | **Master** | 마스터 (~2300-2400 ELO) | 1 | 8 cp |
| 9 | **IM** | 국제 마스터 (~2400-2500 ELO) | 1 | 4 cp |
| 10 | **Maximum** | 엔진 최대 강도 (항상 최선의 수) | 0 | — |

기본값은 **Maximum** (변경 없음, 기존 동작과 100% 동일).

---

### 작동 원리

#### 핵심 알고리즘

Maximum 이외의 난이도에서는, 수를 결정할 때 다음 단계를 거칩니다:

1. **모든 합법수 평가**  
   루트 위치의 모든 합법수에 대해 **Quiescence Search** 로 각 수의 평가치를 계산합니다.

2. **순위 정렬**  
   평가치 내림차순으로 정렬합니다. (`rank 0` = 최선, `rank 1` = 두 번째, …)

3. **후보 수 제한 (MaxN)**  
   상위 `MaxN + 1` 개 수만 후보로 인정합니다.  
   그 이하 순위의 수는 **확률 0** — 절대 선택되지 않습니다.  
   이 제한 덕분에 아무리 낮은 난이도에서도 완전히 황당한 수가 나오지 않습니다.

4. **Softmax 가중치 계산**  
   각 후보 수의 선택 가중치:
   ```
   weight[i] = exp( −evalDiff[i] / Temperature )
   evalDiff[i] = max(0, topScore − score[i])   (단위: centipawn)
   ```
   - **큰 평가치 차이** → 가중치가 급격히 낮아짐 → 그 수를 두는 일이 드묾  
   - **작은 평가치 차이** → 가중치가 비슷함 → 자연스럽게 선택 가능  

5. **확률적 선택**  
   가중치에 비례한 확률로 후보 중 하나를 뽑습니다.

#### 예시

> 최선의 수 이후 평가: **+0.50** (+50 cp)  
> 두 번째 수 이후 평가: **−5.30** (−530 cp)  
> → evalDiff = 580 cp

| 난이도 | Temperature | weight (2nd) | P(1st) | P(2nd) |
|--------|-------------|-------------|--------|--------|
| Martin | 400 | exp(−580/400) ≈ 0.234 | 81% | 19% |
| Club | 70 | exp(−580/70) ≈ 0.000 | ≈100% | ≈0% |
| IM | 4 | exp(−580/4) ≈ 0 | ≈100% | ≈0% |

> 최선의 수 이후 평가: **+0.50** (+50 cp)  
> 두 번째 수 이후 평가: **+0.40** (+40 cp)  
> → evalDiff = 10 cp

| 난이도 | Temperature | weight (2nd) | P(1st) | P(2nd) |
|--------|-------------|-------------|--------|--------|
| Martin | 400 | exp(−10/400) ≈ 0.975 | 51% | 49% |
| Club | 70 | exp(−10/70) ≈ 0.867 | 54% | 46% |
| IM | 4 | exp(−10/4) ≈ 0.082 | 92% | 8% |

사용자 요구사항대로:
- 평가치 차이가 **클 때**: 낮은 난이도에서도 나쁜 수의 확률이 자연스럽게 낮아집니다.
- 평가치 차이가 **작을 때**: 낮은 난이도에서 두 번째 수를 둘 확률이 높습니다.
- **MaxN 제한**: 항상 "그나마 좋은 수들" 중에서만 고르므로 완전한 운 게임이 되지 않습니다.

---

### 기술 세부사항

- **평가 방식**: Quiescence Search (QSearch)  
  메인 탐색과 동일한 캡처/체크 처리 로직을 사용합니다. 얕지만 빠르고, 기물 교환 후의 실제 상황을 반영합니다. 메인 탐색의 TT가 이미 채워진 상태이므로 QSearch는 거의 즉시 완료됩니다.

- **성능 영향**: Maximum 이외 난이도에서 수를 결정할 때 모든 합법수에 대해 QSearch를 실행합니다. 일반적인 포지션(~30수)에서 수 밀리초 수준의 추가 비용이 발생하며, 실용적으로 무시 가능한 수준입니다.

- **오프닝 북**: 오프닝 북은 모든 난이도에서 동일하게 동작합니다. 북에서 수를 찾으면 난이도 시스템을 거치지 않습니다. 북을 끄려면 `book toggle`을 사용하세요.

- **UCI 투명성**: 최선의 수가 아닌 수를 선택할 경우, `info string` 으로 선택된 순위와 평가치 차이를 출력합니다.  
  예: `info string [Club] rank 2 selected: e2e4 (eval diff: 12 cp)`

---

### 변경된 파일

| 파일 | 변경 내용 |
|------|----------|
| `src/Engine/DifficultySettings.cs` | **신규** — `DifficultyLevel` enum, `DifficultyConfig` struct, `DifficultySettings` static table |
| `src/Engine/Engine.cs` | `ApplyDifficulty()`, `SetDifficulty()`, `GetDifficulty()` 추가; IDDFS 출력부 수정 |
| `src/Engine/EnginePlayer.cs` | `SetDifficulty()`, `GetDifficulty()` 래퍼 추가 |
| `src/Program/UCI.cs` | `difficulty` 명령어 전체 (파싱·출력·도움말) 추가 |

---

## v1.0.0 — Initial Release (by kms)

- Helena-Engine 체스 엔진 초기 버전
- 알파-베타 탐색 (Negamax + Aspiration Windows + LMR + SEE)
- Iterative Deepening (IDDFS)
- Transposition Table (64 MB)
- Move Ordering (Hash Move, Killer Moves, History Heuristic, MVVLVA, Pawn Attacks)
- Static Evaluation (PSQT + Tuned Evaluation)
- Quiescence Search
- Opening Book 지원
- UCI+ 프로토콜 지원
