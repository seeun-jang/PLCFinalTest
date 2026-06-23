using ACTMULTILIB_K;
using System;
using System.Windows.Forms;

namespace PLCFinalTest
{
    public partial class Form1 : Form
    {
        ActEasyIF control = new ActEasyIF();

        Timer autoTimer = new Timer();

        bool isConnected = false;
        bool isAutoRunning = false;

        string currentMode = "NONE";

        int step = 0;

        short lastSensorValue = 0;
        short lastYValue = 0;

        string statusMessage = "초기상태";

        // 센서 읽기 실패 횟수
        int sensorFailCount = 0;

        // X 입력 센서
        int SENSOR_B_FWD = 1 << 2;       // X02 : B실린더 전진 완료
        int SENSOR_B_BWD = 1 << 3;       // X03 : B실린더 후진 완료

        int SENSOR_C_BWD = 1 << 4;       // X04 : C실린더 후진 완료
        int SENSOR_C_FWD = 1 << 5;       // X05 : C실린더 전진 완료

        int SENSOR_LIFTA_UP = 1 << 6;    // X06 : LiftA 상승 완료
        int SENSOR_LIFTA_DOWN = 1 << 7;  // X07 : LiftA 하강 완료

        int SENSOR_LIFTB_UP = 1 << 8;    // X08 : LiftB 상승 완료
        int SENSOR_LIFTB_DOWN = 1 << 9;  // X09 : LiftB 하강 완료

        int STAGE_A = 1 << 10;           // XA : 리프트센서 A
        int STAGE_B = 1 << 11;           // XB : 리프트센서 B

        // Y 출력
        int OUT_B_FWD = 1 << 1;          // Y01 : B실린더 전진
        int OUT_B_BWD = 1 << 2;          // Y02 : B실린더 후진

        int OUT_C_FWD = 1 << 3;          // Y03 : C실린더 전진
        int OUT_C_BWD = 1 << 4;          // Y04 : C실린더 후진

        int OUT_LIFTA_UP = 1 << 5;       // Y05 : LiftA Up
        int OUT_LIFTA_DOWN = 1 << 6;     // Y06 : LiftA Down

        int OUT_LIFTB_DOWN = 1 << 7;     // Y07 : LiftB Down
        int OUT_LIFTB_UP = 1 << 8;       // Y08 : LiftB Up

        public Form1()
        {
            InitializeComponent();

            // Timer는 디자인에 넣지 않고 코드에서 직접 생성
            // 100ms보다 200ms가 시뮬레이터에서 더 안정적임
            autoTimer.Interval = 200;
            autoTimer.Tick += autoTimer_Tick;

            UpdateAllLabels();
        }

        // 연결 버튼
        private void button1_Click(object sender, EventArgs e)
        {
            int result = control.Open();

            if (result == 0)
            {
                isConnected = true;
                statusMessage = "연결 성공";

                MessageBox.Show("연결 성공!");
            }
            else
            {
                isConnected = false;
                statusMessage = "연결 실패";

                MessageBox.Show("연결 실패!");
            }

            UpdateAllLabels();
        }

        // 자동 모드 버튼
        private void button2_Click(object sender, EventArgs e)
        {
            if (!CheckConnected())
                return;

            currentMode = "AUTO";
            isAutoRunning = false;
            step = 0;
            sensorFailCount = 0;

            autoTimer.Stop();
            StopAll();

            statusMessage = "자동 모드";
            UpdateAllLabels();
        }

        // 수동 모드 버튼
        private void button3_Click(object sender, EventArgs e)
        {
            if (!CheckConnected())
                return;

            currentMode = "MANUAL";
            isAutoRunning = false;
            step = 0;
            sensorFailCount = 0;

            // 수동 모드에서는 센서를 계속 읽지 않음
            autoTimer.Stop();

            StopAll();

            statusMessage = "수동 모드";
            UpdateAllLabels();
        }

        // 정지 버튼
        private void button4_Click(object sender, EventArgs e)
        {
            isAutoRunning = false;
            step = 0;
            sensorFailCount = 0;

            autoTimer.Stop();

            if (isConnected)
                StopAll();

            statusMessage = "정지";
            UpdateAllLabels();
        }

        // 자동 시작 버튼
        private void button5_Click(object sender, EventArgs e)
        {
            if (!CheckConnected())
                return;

            if (currentMode != "AUTO")
            {
                MessageBox.Show("자동 모드를 먼저 선택하세요.");
                return;
            }

            isAutoRunning = true;
            step = 0;
            sensorFailCount = 0;

            // 자동 시작 시 B, C 실린더는 후진 상태
            // 리프트는 물건이 감지되기 전까지 정지
            SetOutput(
                false, true,      // B 후진
                false, true,      // C 후진
                false, false,     // LiftA 정지
                false, false      // LiftB 정지
            );

            // 자동 시작할 때만 타이머 작동
            autoTimer.Start();

            statusMessage = "자동 시작";
            UpdateAllLabels();
        }

        // B 전진
        private void button6_Click(object sender, EventArgs e)
        {
            if (!CheckManualMode())
                return;

            SetBOutput(true);
            statusMessage = "B 전진";
            UpdateAllLabels();
        }

        // B 후진
        private void button7_Click(object sender, EventArgs e)
        {
            if (!CheckManualMode())
                return;

            SetBOutput(false);
            statusMessage = "B 후진";
            UpdateAllLabels();
        }

        // C 전진
        private void button8_Click(object sender, EventArgs e)
        {
            if (!CheckManualMode())
                return;

            SetCOutput(true);
            statusMessage = "C 전진";
            UpdateAllLabels();
        }

        // C 후진
        private void button9_Click(object sender, EventArgs e)
        {
            if (!CheckManualMode())
                return;

            SetCOutput(false);
            statusMessage = "C 후진";
            UpdateAllLabels();
        }

        // LiftA Up
        private void button10_Click(object sender, EventArgs e)
        {
            if (!CheckManualMode())
                return;

            SetLiftAOutput(true);
            statusMessage = "LiftA Up";
            UpdateAllLabels();
        }

        // LiftA Down
        private void button11_Click(object sender, EventArgs e)
        {
            if (!CheckManualMode())
                return;

            SetLiftAOutput(false);
            statusMessage = "LiftA Down";
            UpdateAllLabels();
        }

        // LiftB Up
        private void button12_Click(object sender, EventArgs e)
        {
            if (!CheckManualMode())
                return;

            SetLiftBOutput(true);
            statusMessage = "LiftB Up";
            UpdateAllLabels();
        }

        // LiftB Down
        private void button13_Click(object sender, EventArgs e)
        {
            if (!CheckManualMode())
                return;

            SetLiftBOutput(false);
            statusMessage = "LiftB Down";
            UpdateAllLabels();
        }

        // 타이머: 자동운전일 때만 센서 읽기
        private void autoTimer_Tick(object sender, EventArgs e)
        {
            if (!isConnected)
                return;

            if (currentMode != "AUTO")
                return;

            if (!isAutoRunning)
                return;

            short sensor = 0;
            int readResult = -1;

            try
            {
                readResult = control.ReadDeviceBlock2("X0", 1, out sensor);
            }
            catch
            {
                sensorFailCount++;

                // 한두 번 센서 읽기가 튀는 것은 무시
                if (sensorFailCount < 5)
                    return;

                isAutoRunning = false;
                autoTimer.Stop();

                statusMessage = "센서 읽기 오류";
                UpdateAllLabels();

                MessageBox.Show("센서 값을 여러 번 연속으로 읽지 못했습니다.\n시뮬레이터 연결 상태를 확인하세요.");
                return;
            }

            if (readResult != 0)
            {
                sensorFailCount++;

                if (sensorFailCount < 5)
                    return;

                isAutoRunning = false;
                autoTimer.Stop();

                statusMessage = "센서 읽기 실패";
                UpdateAllLabels();

                MessageBox.Show("센서 읽기에 실패했습니다.\n시뮬레이터 연결 상태를 확인하세요.");
                return;
            }

            // 센서 읽기 성공하면 실패 횟수 초기화
            sensorFailCount = 0;

            lastSensorValue = sensor;

            RunAutoSequence(sensor);

            UpdateAllLabels();
        }

        // 자동운전 시퀀스
        private void RunAutoSequence(short sensor)
        {
            bool bForwardSensor = IsOn(sensor, SENSOR_B_FWD);
            bool bBackwardSensor = IsOn(sensor, SENSOR_B_BWD);

            bool cForwardSensor = IsOn(sensor, SENSOR_C_FWD);
            bool cBackwardSensor = IsOn(sensor, SENSOR_C_BWD);

            bool liftAUpSensor = IsOn(sensor, SENSOR_LIFTA_UP);
            bool liftADownSensor = IsOn(sensor, SENSOR_LIFTA_DOWN);

            bool liftBUpSensor = IsOn(sensor, SENSOR_LIFTB_UP);
            bool liftBDownSensor = IsOn(sensor, SENSOR_LIFTB_DOWN);

            bool stageA = IsOn(sensor, STAGE_A);
            bool stageB = IsOn(sensor, STAGE_B);

            switch (step)
            {
                case 0:
                    // LiftA에 물건 감지 -> B 전진 + LiftB 상승
                    if (stageA)
                    {
                        SetOutput(
                            true, false,     // B 전진
                            false, true,     // C 후진 유지
                            false, false,    // LiftA 정지
                            true, false      // LiftB 상승
                        );

                        step = 1;
                        statusMessage = "1단계";
                    }
                    break;

                case 1:
                    // B 전진 완료 + LiftB 상승 완료 + StageB 감지
                    if (bForwardSensor && liftBUpSensor && stageB)
                    {
                        SetOutput(
                            false, true,     // B 후진
                            false, true,     // C 후진 유지
                            false, false,    // LiftA 정지
                            false, true      // LiftB 하강
                        );

                        step = 2;
                        statusMessage = "2단계";
                    }
                    break;

                case 2:
                    // B 후진 완료 + LiftB 하강 완료 -> C 전진 + LiftA 하강
                    if (bBackwardSensor && liftBDownSensor)
                    {
                        SetOutput(
                            false, true,     // B 후진 유지
                            true, false,     // C 전진
                            false, true,     // LiftA 하강
                            false, false     // LiftB 정지
                        );

                        step = 3;
                        statusMessage = "3단계";
                    }
                    break;

                case 3:
                    // C 전진 완료 + LiftA 하강 완료 + StageA 감지
                    if (cForwardSensor && liftADownSensor && stageA)
                    {
                        SetOutput(
                            false, true,     // B 후진 유지
                            false, true,     // C 후진
                            true, false,     // LiftA 상승
                            false, false     // LiftB 정지
                        );

                        step = 4;
                        statusMessage = "4단계";
                    }
                    break;

                case 4:
                    // C 후진 완료 + LiftA 상승 완료 -> 반복
                    if (cBackwardSensor && liftAUpSensor)
                    {
                        SetOutput(
                            false, true,     // B 후진 유지
                            false, true,     // C 후진 유지
                            false, false,    // LiftA 정지
                            false, false     // LiftB 정지
                        );

                        step = 0;
                        statusMessage = "반복 대기";
                    }
                    break;
            }
        }

        // 전체 출력 제어
        private void SetOutput(
            bool bFwd, bool bBwd,
            bool cFwd, bool cBwd,
            bool liftAUp, bool liftADown,
            bool liftBUp, bool liftBDown)
        {
            short yValue = 0;

            if (bFwd)
                yValue |= (short)OUT_B_FWD;

            if (bBwd)
                yValue |= (short)OUT_B_BWD;

            if (cFwd)
                yValue |= (short)OUT_C_FWD;

            if (cBwd)
                yValue |= (short)OUT_C_BWD;

            if (liftAUp)
                yValue |= (short)OUT_LIFTA_UP;

            if (liftADown)
                yValue |= (short)OUT_LIFTA_DOWN;

            if (liftBUp)
                yValue |= (short)OUT_LIFTB_UP;

            if (liftBDown)
                yValue |= (short)OUT_LIFTB_DOWN;

            WriteY(yValue);
        }

        // 수동 B 제어
        private void SetBOutput(bool forward)
        {
            int yValue = lastYValue;

            yValue &= ~(OUT_B_FWD | OUT_B_BWD);

            if (forward)
                yValue |= OUT_B_FWD;
            else
                yValue |= OUT_B_BWD;

            WriteY((short)yValue);
        }

        // 수동 C 제어
        private void SetCOutput(bool forward)
        {
            int yValue = lastYValue;

            yValue &= ~(OUT_C_FWD | OUT_C_BWD);

            if (forward)
                yValue |= OUT_C_FWD;
            else
                yValue |= OUT_C_BWD;

            WriteY((short)yValue);
        }

        // 수동 LiftA 제어
        private void SetLiftAOutput(bool up)
        {
            int yValue = lastYValue;

            yValue &= ~(OUT_LIFTA_UP | OUT_LIFTA_DOWN);

            if (up)
                yValue |= OUT_LIFTA_UP;
            else
                yValue |= OUT_LIFTA_DOWN;

            WriteY((short)yValue);
        }

        // 수동 LiftB 제어
        private void SetLiftBOutput(bool up)
        {
            int yValue = lastYValue;

            yValue &= ~(OUT_LIFTB_UP | OUT_LIFTB_DOWN);

            if (up)
                yValue |= OUT_LIFTB_UP;
            else
                yValue |= OUT_LIFTB_DOWN;

            WriteY((short)yValue);
        }

        // Y 출력 쓰기
        private void WriteY(short yValue)
        {
            lastYValue = yValue;

            if (!isConnected)
                return;

            try
            {
                int writeResult = control.WriteDeviceBlock2("Y0", 1, ref yValue);

                if (writeResult != 0)
                {
                    statusMessage = "출력 실패";
                }
            }
            catch
            {
                isAutoRunning = false;
                autoTimer.Stop();

                statusMessage = "출력 오류";
                MessageBox.Show("출력 중 오류가 발생했습니다.\n시뮬레이터 연결 상태를 확인하세요.");
            }
        }

        // 모든 출력 OFF
        private void StopAll()
        {
            short yValue = 0;
            WriteY(yValue);
        }

        // 연결 확인
        private bool CheckConnected()
        {
            if (!isConnected)
            {
                MessageBox.Show("먼저 연결 버튼을 누르세요.");
                return false;
            }

            return true;
        }

        // 수동 모드 확인
        private bool CheckManualMode()
        {
            if (!CheckConnected())
                return false;

            if (currentMode != "MANUAL")
            {
                MessageBox.Show("수동 모드를 먼저 선택하세요.");
                return false;
            }

            if (isAutoRunning)
            {
                MessageBox.Show("자동운전 중에는 수동 조작을 할 수 없습니다.");
                return false;
            }

            return true;
        }

        // 비트가 켜졌는지 확인
        private bool IsOn(short value, int mask)
        {
            return (((int)value & mask) != 0);
        }

        // 현재 모드 글자
        private string GetModeText()
        {
            if (currentMode == "AUTO")
                return "자동";

            if (currentMode == "MANUAL")
                return "수동";

            return "없음";
        }

        // 전체 라벨 갱신
        private void UpdateAllLabels()
        {
            UpdateStatusLabel();
            UpdateSensorLabel();
            UpdateOutputLabel();
        }

        // 현재 모드 / 전체 상태
        private void UpdateStatusLabel()
        {
            string runText = isAutoRunning ? "실행" : "정지";

            label1.Text =
                "모드: " + GetModeText() +
                "   운전: " + runText +
                "   Step: " + step +
                "   상태: " + statusMessage;
        }

        // 센서 상태
        private void UpdateSensorLabel()
        {
            string sensorText = "";

            sensorText = AddOnText(sensorText, IsOn(lastSensorValue, STAGE_A), "StageA");
            sensorText = AddOnText(sensorText, IsOn(lastSensorValue, STAGE_B), "StageB");

            sensorText = AddOnText(sensorText, IsOn(lastSensorValue, SENSOR_B_FWD), "B전진완료");
            sensorText = AddOnText(sensorText, IsOn(lastSensorValue, SENSOR_B_BWD), "B후진완료");

            sensorText = AddOnText(sensorText, IsOn(lastSensorValue, SENSOR_C_FWD), "C전진완료");
            sensorText = AddOnText(sensorText, IsOn(lastSensorValue, SENSOR_C_BWD), "C후진완료");

            sensorText = AddOnText(sensorText, IsOn(lastSensorValue, SENSOR_LIFTA_UP), "LiftA Up");
            sensorText = AddOnText(sensorText, IsOn(lastSensorValue, SENSOR_LIFTA_DOWN), "LiftA Down");

            sensorText = AddOnText(sensorText, IsOn(lastSensorValue, SENSOR_LIFTB_UP), "LiftB Up");
            sensorText = AddOnText(sensorText, IsOn(lastSensorValue, SENSOR_LIFTB_DOWN), "LiftB Down");

            if (sensorText == "")
                sensorText = "감지 없음";

            label2.Text = "센서: " + sensorText;
        }

        // 출력 상태
        private void UpdateOutputLabel()
        {
            string outputText = "";

            outputText = AddOnText(outputText, IsOn(lastYValue, OUT_B_FWD), "B전진");
            outputText = AddOnText(outputText, IsOn(lastYValue, OUT_B_BWD), "B후진");

            outputText = AddOnText(outputText, IsOn(lastYValue, OUT_C_FWD), "C전진");
            outputText = AddOnText(outputText, IsOn(lastYValue, OUT_C_BWD), "C후진");

            outputText = AddOnText(outputText, IsOn(lastYValue, OUT_LIFTA_UP), "LiftA Up");
            outputText = AddOnText(outputText, IsOn(lastYValue, OUT_LIFTA_DOWN), "LiftA Down");

            outputText = AddOnText(outputText, IsOn(lastYValue, OUT_LIFTB_UP), "LiftB Up");
            outputText = AddOnText(outputText, IsOn(lastYValue, OUT_LIFTB_DOWN), "LiftB Down");

            if (outputText == "")
                outputText = "없음";

            label3.Text = "출력: " + outputText;
        }

        // ON인 항목만 문자열에 추가
        private string AddOnText(string text, bool isOn, string name)
        {
            if (!isOn)
                return text;

            if (text != "")
                text += ", ";

            text += name;
            return text;
        }

        // 디자이너 오류 방지용
        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void label6_Click(object sender, EventArgs e)
        {
        }

        // 폼 종료 시 출력 OFF + 연결 종료
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (isConnected)
                {
                    StopAll();
                    control.Close();
                }
            }
            catch
            {
            }

            base.OnFormClosing(e);
        }
    }
}
