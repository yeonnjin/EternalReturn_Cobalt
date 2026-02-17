# Unity MOBA 팀 프로젝트

본 레포지토리는 팀 프로젝트 원본 레포지토리를 Fork한 저장소입니다.  
> 저는 서버 권한 구조에서 클라이언트 입력, FSM 상태 처리, 스킬 로직, 이동 동기화가 어떻게 연결되는지를 중심으로 구현했습니다.

---

## 📌 프로젝트 개요

Unity 기반 멀티플레이 MOBA 프로젝트입니다.  
서버 권한 구조를 기반으로 클라이언트–서버 간 상태 불일치를 최소화하는 것을 목표로 개발했습니다.

- 프로젝트 형태: 팀 프로젝트
- 엔진: Unity
- 언어: C#
- 구조: FSM 기반 플레이어 아키텍처
- 네트워크: Server Authoritative Model

---

## 🙋 담당 역할

### 1️⃣ 플레이어 FSM 구조 설계 및 구현

- 플레이어 상태(Idle / Move / Attack / Skill 등)를 FSM으로 관리
- 네트워크 지연 상황에서도 상태 전이가 꼬이지 않도록 구조 설계
- 서버에서 최종 상태를 결정하고 클라이언트는 이를 표현하는 방식 적용

---

### 2️⃣ SkillHandler 기반 스킬 시스템 구현

- 스킬 로직을 PlayerController에서 분리하여 SkillHandler로 관리
- 다양한 형태의 스킬을 공통 구조로 처리 가능하도록 설계

구현 스킬 타입:

- 즉발 스킬
- 시전형 스킬
- 투사체 기반 스킬
- 이동 포함 스킬

---

### 3️⃣ 클라이언트–서버 이동 동기화

- 입력 시 클라이언트에서 즉시 이동하여 조작감 확보
- 서버에서 이동 가능 여부 및 최종 위치 검증
- 이동 중 스킬 입력이 발생해도 상태가 꼬이지 않도록 FSM과 연동

---

### 4️⃣ 네트워크 패킷 흐름 통합

- Command 기반 입력 구조 설계
- Client Input → Server 검증 → 결과 브로드캐스트 → Client 반영 흐름 구성
- 애니메이션 상태와 서버 상태가 항상 일치하도록 처리

---

## 🧠 구조 요약

본 프로젝트는 다음과 같은 책임 분리를 기반으로 설계되었습니다.

### Client

- 입력 처리
- 즉각적인 시각적 피드백
- 애니메이션 및 UI 표현

### Server

- 플레이어 상태 검증
- 스킬 판정
- 위치 권한 관리
- 최종 결과 전송

FSM을 중심으로 상태를 관리하여:

- 상태 전이 흐름을 명확하게 유지
- 이동 / 공격 / 스킬 중복 처리 방지
- 캐릭터 확장 시 기존 로직 수정 최소화

를 목표로 설계했습니다.

---

## ⭐ 주요 구현 코드

▶ Player FSM 구조

[ PlayerStateMachine ]
- 플레이어 상태(FSM) 진입/실행/종료 흐름을 관리하는 메인 클래스
- https://github.com/yeonnjin/EternalReturn_Cobalt/blob/main/Server/Server/Game/Object/Player/State/PlayerStateMachine.cs

[ Player_MovingState ]
- 이동 중 타겟 판별, 공격 전환, 서버 확정 위치 반영 등
- 이동 관련 상태 처리 전반을 담당
https://github.com/yeonnjin/EternalReturn_Cobalt/blob/main/Server/Server/Game/Object/Player/State/States/Player_MovingState.cs

▶ SkillHandler 기반 스킬 처리 구조

[ Player_SkillState ]
- 스킬 시전 중 이동 허용, 입력 큐 처리, 스킬 종료 조건 관리 등
- 스킬 상태 전반을 담당하는 FSM 상태 클래스
- https://github.com/yeonnjin/EternalReturn_Cobalt/blob/main/Server/Server/Game/Object/Player/State/States/Player_SkillState.cs

[ SkillHandlerBase ]
- 스킬 공통 처리 베이스 클래스
- 스킬별 로직 분리 및 확장 가능한 구조를 위한 핵심 설계 파일
- https://github.com/yeonnjin/EternalReturn_Cobalt/blob/main/Server/Server/Game/Object/Player/Skill/SkillHandler/SkillHandlerBase.cs

[ Rozzi_Q ]
- 실제 캐릭터 스킬 구현 사례 (타겟 판정, 후속 처리 등 포함)
- https://github.com/yeonnjin/EternalReturn_Cobalt/blob/main/Server/Server/Game/Object/Player/Skill/Skills/Rozzi/Rozzi_Q.cs

▶ Client Controller (클라이언트 입력 및 표현)

[ MyPlayerController ]
- 클라이언트 입력 처리 및 서버 상태 반영을 담당하는 컨트롤러
- FSM 및 서버 결과와 애니메이션/이동 표현을 연결
- https://github.com/yeonnjin/EternalReturn_Cobalt/blob/main/Client/Assets/Scripts/Controllers/MyPlayerController.cs

---

## 💡 작업하며 중점적으로 고려한 부분

단순 기능 구현보다 다음을 중점적으로 고민했습니다:

- 역할 분리 및 책임 명확화
- 캐릭터 확장 가능 구조
- 네트워크 지연 상황에서도 안정적인 상태 관리
- 유지보수를 고려한 구조 설계
