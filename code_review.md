# Helena Engine — 코드 리뷰

> 분석 기준: Chess Programming Wiki 및 표준 체스 엔진 관행(Alpha-Beta, SEE, TT, Tapered Eval 등)  
> 최초 분석: 2026-05-25 | BUG 수정: 2026-05-25 | CLEAN 수정: 2026-05-25  
> BUG-01~07 수정 완료 (BUG-08은 의도적 구현) | CLEAN-01~11 전체 수정 완료

---

## 목차

1. [프로젝트 구조 요약](#1-프로젝트-구조-요약)
2. [🔴 치명적 논리 오류](#2--치명적-논리-오류)
3. [🟠 중간 수준 버그](#3--중간-수준-버그)
4. [🟡 마이너 이슈 및 코드 품질](#4--마이너-이슈-및-코드-품질)
5. [🟢 잘 된 부분](#5--잘-된-부분)

---

## 1. 프로젝트 구조 요약

| 경로 | 역할 |
|------|------|
| `src/Core/Base/` | 전역 타입 정의, 좌표/스퀘어/상수 |
| `src/Core/Board/` | Board, Move, Piece, Bitboard, Zobrist |
| `src/Core/MoveGen/` | 합법 수 생성 (Magic Bitboards) |
| `src/Engine/` | 탐색(Negamax/IDDFS), 평가(Tapered Eval), TT, SEE, 수 정렬 |
| `src/Program/` | UCI 인터페이스, 메인 엔트리 |
| `texel-tuner-main/` | C++ Texel Tuner (helena.cpp가 C# 엔진의 평가 파라미터 조율) |

**언어:** C# (.NET 9), 체스 표현: 0x88 아님, `ulong` 비트보드 + Magic Bitboards  
**탐색:** Iterative Deepening + Negamax + Alpha-Beta + Aspiration Windows + LMR + SEE  
**평가:** Tapered Eval (Midgame/Endgame 보간), PSQT (Texel Tuning 적용)

---

## 2. 🔴 치명적 논리 오류

### ✅ BUG-01 · `MoveFlag.IsCapture()`가 앙파상(EP)을 포함하지 않음 — **수정됨**

**파일:** `src/Core/Board/Move.cs:76-79`

```csharp
// 현재 코드 (버그)
public static bool IsCapture(ushort flag)
{
    return (flag == Capture) || (flag > 11);
}
```

`EP = 5`이므로 `flag == Capture`(=4) 와 `flag > 11` 모두 불만족 → `IsCapture(EP) = false`.  
앙파상은 명백히 포획 수임에도 `false`를 반환한다.

**파급 범위:**

| 사용처 | 코드 | 결과 |
|--------|------|------|
| `Engine.cs:278` | `bool isCapture = MoveFlag.IsCapture(...)` | EP가 **조용한 수**로 분류됨 |
| `Engine.cs:336` | `if (!isCapture)` → Killer 등록 | EP가 **Killer Move**에 잘못 등록됨 |
| `Engine.cs:343` | `if (!isCapture)` → History 업데이트 | EP가 **History 휴리스틱**에 포함됨 |
| `Engine.cs:286` | `!isCapture` → LMR 적용 여부 | EP에 **LMR이 적용**되어 탐색 깊이 감소 |
| `MoveOrdering.cs:93` | `if (MoveFlag.IsCapture(move.Flag))` → 캡처 스코어 | EP가 **조용한 수처럼 낮은 점수** 부여 |

특히 `MoveOrdering.cs:95`에는 `move.Flag == MoveFlag.EP` 처리 코드가 있지만, 외부 `if (IsCapture(...))` 조건을 통과하지 못하여 **Dead Code** 상태이다.

**수정 (`Move.cs`):**
```csharp
public static bool IsCapture(ushort flag)
{
    return (flag == Capture) || (flag == EP) || (flag > 11);
}
```

---

### ✅ BUG-02 · `Coord.operator!=` — `&&` vs `||` 논리 반전 — **수정됨**

**파일:** `src/Core/Base/Coord.cs:27`

```csharp
// 현재 코드 (버그)
public static bool operator !=(Coord a, Coord b) => a.X != b.X && a.Y != b.Y;

// 올바른 코드
public static bool operator !=(Coord a, Coord b) => a.X != b.X || a.Y != b.Y;
```

드모르간 법칙: `!(A == B)` = `!(a.X==b.X && a.Y==b.Y)` = `a.X!=b.X || a.Y!=b.Y`.  
현재 코드에서는 `(0,0) != (1,0)`이 **false**를 반환한다 (`0!=1 && 0!=0` = `true && false` = `false`).

즉, "한 좌표만 다른 경우" `!=`이 `false`를 반환하여 두 Coord가 동일하다고 잘못 판단한다.

**현재 영향:** `Coord`의 `!=`는 소스 내에서 직접 호출되는 곳이 발견되지 않아 즉각적인 런타임 오류는 없다. 그러나 `Equals()` 오버라이드에서도 `this == other`를 사용하고 있어 `!=`가 `==`의 부정과 불일치한다는 점에서 불변식(invariant) 위반이다. 향후 `!=`를 직접 사용하는 코드 추가 시 버그 발생이 확실하다.

---

### ✅ BUG-03 · `SquareHelper.ToString(int, int)` — char 산술 오류 — **수정됨**

**파일:** `src/Core/Base/Square.cs:41`

```csharp
// 현재 코드 (버그)
return $"{'a' + file}{'1' + rank}";

// 올바른 코드
return $"{(char)('a' + file)}{(char)('1' + rank)}";
```

C#에서 `char + int`는 `int`를 반환한다. 따라서 file=0, rank=0일 때 `"a1"`이 아닌 **`"9749"`** (ASCII 코드)가 반환된다. 같은 파일의 `ToString(Square)` 메서드는 `(char)`로 캐스팅하여 올바르게 구현되어 있다.

---

## 3. 🟠 중간 수준 버그

### ✅ BUG-04 · TT 노드 타입 주석 혼동 (Alpha ↔ Beta 설명 반전) — **수정됨**

**파일:** `src/Engine/TT.cs:13-15`

```csharp
public const byte Exact = 0;
// Lower bound  ← 주석 틀림! Upper bound가 맞다
public const byte Alpha = 1;
// Upper bound  ← 주석 틀림! Lower bound가 맞다
public const byte Beta = 2;
```

표준 Alpha-Beta 용어:
- **Alpha(실패-저) 노드**: 점수 ≤ alpha → **상한(Upper bound)** 저장
- **Beta(실패-고) 노드**: 점수 ≥ beta → **하한(Lower bound)** 저장

실제 `LookupEval()` 로직은 올바르다. 하지만 주석이 반대로 적혀 있어 유지보수 시 혼란을 야기한다.

```csharp
// 사용 로직은 맞지만 주석이 틀림
if (entry.nodeType == Alpha && correctEval <= alpha)  // Alpha = 상한이므로 correctEval <= alpha면 컷오프 가능 ✓
    return correctEval;
if (entry.nodeType == Beta && correctEval >= beta)    // Beta = 하한이므로 correctEval >= beta면 컷오프 가능 ✓
    return correctEval;
```

**수정안:** 주석 수정
```csharp
// Upper bound (fail-low node: true score ≤ stored value)
public const byte Alpha = 1;
// Lower bound (fail-high node: true score ≥ stored value)
public const byte Beta = 2;
```

---

### ✅ BUG-05 · `TaperedScore` 생성자 — 음수 EG 값 시 MG 오차 — **수정됨**

**파일:** `src/Engine/Evaluation.cs:474-479`

```csharp
public TaperedScore(int m, int e)
{
    value = e;        // 음수 e → 부호 확장된 int (상위 16비트 = 0xFFFF)
    value += m << 16; // m에 상위 비트 오염이 전달됨
}
```

`e`가 음수일 때, `int` 표현의 상위 비트가 `m << 16`을 오염시킨다.

**예시:** `new S(1, -1)`:
```
value = -1 (0xFFFFFFFF)
value += 1 << 16 = 0x10000
result = 0x0000FFFF

Mid = 0x0000FFFF >> 16 = 0   ← 1이어야 함 (오차 -1)
End = (short)(0xFFFF) = -1   ✓
```

**수정 (`Evaluation.cs`):**
```csharp
public TaperedScore(int m, int e)
{
    value = (e & 0xFFFF) | (m << 16);
}
```

---

### ✅ BUG-06 · `TaperedScore operator *` — packed 값 직접 곱셈의 carry 오염 — **수정됨**

**파일:** `src/Engine/Evaluation.cs:513-520`

```csharp
public static TaperedScore operator *(TaperedScore a, int m)
{
    return new TaperedScore(a.value * m);  // packed raw int 곱셈
}
```

`a.value * m` 연산에서 하위 16비트(EG) 부분의 carry가 상위 16비트(MG)를 오염시킬 수 있다.

**예시:** MG=100, EG=-50, m=3일 경우:
```
value = 6553550 (0x0063FFCE)
value * 3 = 19660650 (0x012BFF6A)
Mid = 0x012B = 299  ← 300이어야 함
End = (short)(0xFF6A) = -150 ✓
```

**수정 (`Evaluation.cs`):**
```csharp
public static TaperedScore operator *(TaperedScore a, int m)
{
    return new TaperedScore(a.Mid * m, a.End * m);
}
```

현재 코드에서 `m = -1` (PSQTValue에서 흑 관점 반전)은 올바르게 동작한다 (2의 보수 부정은 패킹 형식에서도 분배 법칙 성립). 다른 배수(`MaterialValues * pieceCountDiff` 등)에서 EG가 크게 음수일 때 오차가 발생할 수 있다.

---

### ✅ BUG-07 · `ucinewgame` 명령 미구현 — **수정됨**

**파일:** `src/Program/UCI.cs`

UCI 프로토콜 규격상 `ucinewgame`은 엔진에 새 게임 시작을 알리며, 이때 TT(Transposition Table)를 비우고 탐색 히스토리를 초기화해야 한다. 현재 `switch` 처리에 해당 명령어가 없어 **이전 게임의 TT 항목이 다음 게임에 오염**될 수 있다.

**수정:** `Engine.NewGame()` → `EnginePlayer.NewGame()` → `UCI.cs` `case "ucinewgame"` 추가.

```csharp
// Engine.cs
public void NewGame()
{
    tt.Clear();
    moveOrdering.ClearHistory();
    moveOrdering.ClearKillerMoves();
    pv.ClearAll();
}

// EnginePlayer.cs
public void NewGame()
{
    CancelAndWait();
    engine.NewGame();
}

// UCI.cs switch
case "ucinewgame":
    engine.NewGame();
    break;
```

---

### BUG-08 · `IsRepetition()` — 2회 반복 시 무승부 처리

**파일:** `src/Core/Board/Board.cs:369-402`

현재 구현은 같은 Zobrist 키가 과거에 **단 한 번이라도** 등장하면 `true`를 반환한다 (2-fold repetition = draw). 체스 규칙은 **3회** 반복을 요구한다.

**실용적 관점:** 탐색 중 2-fold를 무승부로 처리하는 것은 반복 회피를 위한 흔한 엔진 기법이나, 실제 규칙(3-fold)과 다르므로 유리한 포지션을 잘못 포기할 수 있다. 의도적 선택이라면 주석으로 명시 필요.

---

## 4. 🟡 마이너 이슈 및 코드 품질

### ✅ CLEAN-01 · `Coord.GetHashCode()` 미구현 — **수정됨**

**파일:** `src/Core/Base/Coord.cs`

모든 `Coord`가 동일한 해시값 0을 반환하여 `Dictionary<Coord,…>`, `HashSet<Coord>` 등에서 O(1) → O(n) 성능 저하 발생.

**수정:** `HashCode.Combine(X, Y)` 사용, 불필요한 `// NOT IMPLEMENTED` 주석 제거.
```csharp
public override int GetHashCode() => HashCode.Combine(X, Y);
```

---

### ✅ CLEAN-02 · `MoveGen.CalculateAttackData()` — 대량의 주석 처리 코드 — **수정됨**

**파일:** `src/Core/MoveGen/MoveGen.cs`

핀/체크 계산의 구버전 구현 140여 줄이 주석으로 남아 있었다. 이 로직은 이미 `Board.CalculatePinCheckData()`로 이전되어 있어 완전히 불필요한 코드였다.

**수정:** 주석 처리된 구버전 코드 전체 삭제. `CalculateAttackData()`가 슬라이딩 공격맵·나이트·폰·킹 공격맵 계산만 담당하도록 정리. 이전 목적을 설명하는 단일 주석 추가.

---

### ✅ CLEAN-03 · `EvaluationConstants.cs` — `EvaluationHelper.cs`와 중복 필드 — **수정됨**

**파일:** `src/Engine/EvaluationConstants.cs`

`DistanceFromSquare`, `DistanceFromCenter`, `PawnForwardMask`, `PassedPawnMask`, `ForwardPawnAttackers`, `KingArea` 6개 필드와 그 정적 생성자 초기화 코드(약 90줄)가 `EvaluationHelper`와 완전히 중복되어 있었다. 실제 사용처는 모두 `using static EvaluationHelper`를 통해 `EvaluationHelper` 버전을 사용하고 있었다.

**수정:** 6개 중복 필드 및 정적 생성자 전체 제거. 더불어 참조처가 없던 `MaterialValues`(TaperedScore[]) 배열도 제거, 불필요해진 `using H.Core` 제거. `AbsoluteMaterial`(SEE·MoveOrdering에서 사용)과 평가 상수들만 유지.

---

### ✅ CLEAN-04 · `SEE.IsGoodCapture()` vs `SEE.HasPositiveScore()` — 코드 중복 — **수정됨**

**파일:** `src/Engine/SEE.cs`

두 메서드의 SEE 교환 루프(~80줄)가 완전히 동일한 코드였다. 유일한 차이는 초기 gain 계산 방식과 프로모션 처리 여부뿐이었다.

**수정:** 공통 루프를 `private bool RunSEELoop(Move move, int score)` 메서드로 추출. 두 public 메서드는 각각의 초기 score를 계산한 뒤 이른 반환 최적화(`if score < 0`, `if score >= 0`) 후 `RunSEELoop`을 호출하는 형태로 간소화.

---

### ✅ CLEAN-05 · `Evaluation.board` — 전역 싱글톤 참조 — **주석으로 명시됨**

**파일:** `src/Engine/Evaluation.cs`

`static Board board = Main.MainBoard;` 선언은 SMP(멀티스레드 탐색) 추가 시 구조적 제약이 된다.

**수정:** 코드 구조를 변경하지 않고, 싱글톤 참조의 한계와 SMP 도입 시 필요한 변경 방향을 `//` 주석으로 명시.

---

### ✅ CLEAN-06 · `pieceCount` 인덱스와 `PieceType` 상수 불일치 — **주석으로 명시됨**

**파일:** `src/Engine/Evaluation.cs`

`pieceCount[color][i]`는 `BitboardSet.Indexed(i)` 기반의 0-based 인덱스(`0=Pawn…5=King`)를 사용하지만, `PieceHelper` 상수는 1-based(`PAWN=1…KING=6`)이다. 현재 코드는 0-based 루프로 일관되게 작성되어 동작상 오류는 없으나 혼동 여지가 있었다.

**수정:** `pieceCount` 선언부에 인덱스 매핑(`0=Pawn, 1=Knight, …`)과 `PieceHelper` 상수로 직접 인덱싱하면 안 된다는 경고를 `//` 주석으로 명시.

---

### ✅ CLEAN-07 · `MoveOrdering.FindFirstLE` / `FindLastGE` — 로직 검증 및 주석 추가 — **수정됨**

**파일:** `src/Engine/MoveOrdering.cs`

초기 리뷰에서 "이진 탐색 방향이 잘못되었을 수 있다"고 지적했으나, 직접 트레이스로 **로직이 정확함을 확인**했다. `moveScores`는 내림차순으로 정렬되며, 두 함수는 이를 올바르게 처리한다:
- `FindFirstLE`: 매칭 시 `hi = mid-1` (왼쪽 탐색) → 내림차순에서 첫 번째 `≤ b` 위치를 정확히 찾음
- `FindLastGE`: 매칭 시 `lo = mid+1` (오른쪽 탐색) → 내림차순에서 마지막 `≥ a` 위치를 정확히 찾음

**수정:** 코드 로직은 변경 없음. 내림차순 배열을 전제로 작성된 함수임을 명확히 하는 `//` 주석 추가. 인라인 조건 구조로 가독성 개선.

---

### ✅ CLEAN-08 · `GenerateMoves(bool)` 오버로드 — 힙 할당 — **주석으로 명시됨**

**파일:** `src/Core/MoveGen/MoveGen.cs`

탐색 내부에서는 `stackalloc ref MoveList` 오버로드를 사용하지만, 이 오버로드는 매 호출마다 힙 할당을 한다. 성능 경로 외부(UCI 명령, perft 등)에서만 사용해야 한다.

**수정:** 힙 할당 오버로드임을 명시하고 내부 탐색에서는 사용하지 말아야 한다는 `//` 주석 추가.

---

### ✅ CLEAN-09 · Aspiration Window — 재탐색 횟수 초과 시 주석 불명확 — **수정됨**

**파일:** `src/Engine/Engine.cs`

기존 주석 `"Search on this depth will be complete after this iteration"` 은 "이번 반복에서 완료된다"는 뜻으로 읽히지만, 실제로는 full-window로 리셋된 **다음** Negamax 호출이 성공한 뒤 `else { break; }` 분기에서 종료된다.

**수정:** `"Window reset to full-width. The next Negamax call will be a full-window search, after which the while(true) loop will break via the else branch above."` 로 교체.

---

### ✅ CLEAN-10 · `Engine.cs` — Search 중 상태 변수 thread-safety — **수정됨**

**파일:** `src/Engine/Engine.cs`

`isSearching`과 `cancellationRequested`가 UCI 스레드와 탐색 스레드 양쪽에서 동기화 없이 읽고 쓰여졌다. `_searchRequestedFlag`만 `Interlocked`로 보호되어 있었다.

**수정:** `isSearching`과 `cancellationRequested`에 `volatile` 키워드 추가. `volatile`은 JIT가 값을 레지스터에 캐싱하는 것을 막아 크로스-스레드 가시성을 보장한다. (`bestMove`, `bestEval` 등 복합 타입은 `volatile` 적용 불가 — 이들은 탐색 스레드에서만 쓰이므로 현재는 문제 없음.)

---

### ✅ CLEAN-11 · `Board.GetAllAttackersTo` — 연산자 우선순위 가독성 — **수정됨**

**파일:** `src/Core/Board/Board.cs`

킹 공격자 계산 부분이 줄 바꿈으로 인해 오독될 위험이 있었다. `&` 우선순위가 `|`보다 높아 실제 평가 순서는 `((Kings & KingMovement) & occupancy)`로 맞지만, 겉으로는 `(Kings | Knights) & KingMovement & occupancy`처럼 보였다.

**수정:** 의도한 연산 순서를 반영하는 명시적 괄호 추가:
```csharp
(((BitboardSets[0][PieceHelper.KING] | BitboardSets[1][PieceHelper.KING])
  & Bits.KingMovement[square]) & occupancy);
```

---

## 5. 🟢 잘 된 부분

- **Magic Bitboards 구현**: Fancy Magic을 사용한 슬라이더 공격 생성은 정확하고 효율적이다.
- **CalculatePinCheckData**: Board 레벨에서 핀/체크 데이터를 미리 계산하여 MoveGen에서 재사용하는 구조는 올바르다. `BetweenMasks[ksq + offset][firstBlocker]`를 사용하여 킹 스퀘어를 체크 레이에서 제외하는 것도 정확하다.
- **TT 구현**: `CorrectMateScoreForStorage` / `CorrectRetrievedMateScore`로 메이트 점수를 루트 거리에 따라 보정하는 로직은 표준적이며 올바르다.
- **Aspiration Windows**: `MaxAspirations` 초과 시 full-window fallback 처리 흐름이 올바르다.
- **PV 테이블**: 삼각형 배열(Triangular PV Table) 구조로 구현되어 있으며 PV 복사 로직이 NullMove terminator를 통해 정상 동작한다.
- **SEE**: `PopLeastValuableAttacker` 패턴 및 X-ray 공격 갱신 로직이 올바르게 구현되어 있다.
- **Tapered Eval**: 미드게임/엔드게임 보간 방식이 Texel Tuning과 연동되어 있다.
- **이중 체크 시 킹 이동만 생성**: `inDoubleCheck` 시 `GenerateSlidingMoves` 등을 스킵하는 구조가 정확하다.
- **앙파상 후 X-ray 체크 검증**: `InCheckAfterEP()`에서 Magic Rook 공격으로 EP 후 수평 체크를 직접 계산하는 로직이 올바르다.

---

## 요약 표

| ID | 심각도 | 파일 | 설명 |
|----|--------|------|------|
| BUG-01 | 🔴 치명 | `Move.cs` | `IsCapture(EP) = false` — EP가 조용한 수로 처리됨 |
| BUG-02 | 🔴 치명 | `Coord.cs` | `!=` 연산자 `&&` → `\|\|` 논리 오류 |
| BUG-03 | 🔴 치명 | `Square.cs` | `ToString(int,int)` char 산술 오류 (숫자 출력) |
| BUG-04 | 🟠 중간 | `TT.cs` | Alpha/Beta 주석 반전 (로직은 정상, 주석 버그) |
| BUG-05 | 🟠 중간 | `Evaluation.cs` | `TaperedScore(m,e)` 음수 EG 시 MG 오차 ±1 |
| BUG-06 | 🟠 중간 | `Evaluation.cs` | `TaperedScore * int` carry 오염 가능성 |
| BUG-07 | 🟠 중간 | `UCI.cs` | `ucinewgame` 미처리 — TT 오염 |
| BUG-08 | 🟠 중간 | `Board.cs` | `IsRepetition()` 2-fold(규칙은 3-fold) |
| CLEAN-01 | ✅ | `Coord.cs` | `GetHashCode()` → `HashCode.Combine(X, Y)` |
| CLEAN-02 | ✅ | `MoveGen.cs` | 구버전 핀/체크 코드 140줄 삭제 |
| CLEAN-03 | ✅ | `EvaluationConstants.cs` | 6개 중복 필드·정적 생성자·미사용 MaterialValues 제거 |
| CLEAN-04 | ✅ | `SEE.cs` | 공통 SEE 루프를 `RunSEELoop()`으로 추출 |
| CLEAN-05 | ✅ | `Evaluation.cs` | 싱글톤 참조 한계·SMP 제약을 `//` 주석으로 명시 |
| CLEAN-06 | ✅ | `Evaluation.cs` | 0-based 인덱스 vs 1-based PieceType 불일치를 `//` 주석으로 명시 |
| CLEAN-07 | ✅ | `MoveOrdering.cs` | 로직 정확함 확인, 내림차순 전제 설명 주석 추가 |
| CLEAN-08 | ✅ | `MoveGen.cs` | 힙 할당 오버로드임을 `//` 주석으로 명시 |
| CLEAN-09 | ✅ | `Engine.cs` | Aspiration Window 주석 정확한 내용으로 교체 |
| CLEAN-10 | ✅ | `Engine.cs` | `isSearching`, `cancellationRequested`에 `volatile` 추가 |
| CLEAN-11 | ✅ | `Board.cs` | 킹 공격자 계산에 명시적 괄호 추가 |

---

*Helena Engine Code Review — Generated by analysis of full source tree*
