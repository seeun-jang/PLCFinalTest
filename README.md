# [실습6] PLC 리프트 제어 자동/수동 운전 프로그램  

### 2601110299 장세은

## 실행 영상
[자동 운전](https://github.com/user-attachments/assets/e404dcf5-bdc6-4d40-a26f-141c8cba72bb)  
[수동 조작](https://github.com/user-attachments/assets/49030a3d-cb30-4f36-8844-edeaa8003482)  

---

## 프로젝트 개요

본 프로젝트는 **리프트 제어를 기반으로 한 PLC 기말 과제 프로그램**입니다.
C# Windows Forms와 `ACTMULTILIB_K` 라이브러리를 사용하여 자동 운전과 수동 조작 기능을 구현하였습니다.

자동 운전에서는 센서 조건에 따라 전체 시퀀스가 순차적으로 진행되며,
수동 조작에서는 각 실린더와 리프트를 개별 버튼으로 제어할 수 있도록 구성하였습니다.

---

## 주요 기능

* PLC 연결 기능
* 자동 모드 / 수동 모드 선택
* 자동 운전 시 전체 시퀀스 진행
* B실린더 전진 / 후진 수동 제어
* C실린더 전진 / 후진 수동 제어
* LiftA Up / Down 수동 제어
* LiftB Up / Down 수동 제어
* GUI를 통한 현재 모드, 센서 상태, 출력 상태 표시

---

## 자동 운전 시퀀스

1. StageA에 물건 감지
2. B실린더 전진 + LiftB 상승
3. StageB 감지 후 B실린더 후진 + LiftB 하강
4. C실린더 전진 + LiftA 하강
5. StageA 감지 후 C실린더 후진 + LiftA 상승
6. 한 사이클 완료 후 반복 동작

---

## 수동 조작 구성

수동 모드에서는 다음 컴포넌트를 각각 개별 제어할 수 있습니다.

| 컴포넌트  | 수동 조작     |
| ----- | --------- |
| B실린더  | 전진 / 후진   |
| C실린더  | 전진 / 후진   |
| LiftA | Up / Down |
| LiftB | Up / Down |

---

## 개발 환경

* C#
* Windows Forms
* Visual Studio
* ACTMULTILIB_K
* PLC Simulator

---

## 제출 관련

본 저장소는 PLC 기말 과제의 코드 및 실행 자료를 정리하기 위한 저장소입니다.
세부 동작 시나리오 검증과 핵심 코드 설명은 별도 PDF 보고서에 작성하였습니다.
