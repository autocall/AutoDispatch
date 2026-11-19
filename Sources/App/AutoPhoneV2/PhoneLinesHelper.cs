using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AutoPhoneV2 {

    public class PhoneLine {
        private Timer CallDurationTimer;
        private TimeSpan CallTime;
        private bool callEstablished;
        private bool callHeld;
        private bool calling;

        public int Number { get; set; }
        public int? LineId { get; set; }
        public object Model { get; set; }
        public string Phone { get; set; }
        public string TransferPhone { get; set; }
        public List<int> ConnectionIds { get; set; } = new List<int>();
        public Dictionary<int, string> Connections { get; set; } = new Dictionary<int, string>();

        public bool CallEstablished {
            get => callEstablished;
            set {
                callEstablished = value;
                UpdatePanel(Number, Calling, CallEstablished, CallHeld);
            }
        }
        public bool CallHeld {
            get => callHeld;
            set {
                callHeld = value;
                UpdatePanel(Number, Calling, CallEstablished, CallHeld);
            }
        }
        public bool Calling {
            get => calling;
            set {
                calling = value;
                UpdatePanel(Number, Calling, CallEstablished, CallHeld);
            }
        }

        public void StartCallDurationTimer(Action<string> handler) {
            if (this.CallDurationTimer == null) {
                this.CallDurationTimer = new Timer();
                this.CallDurationTimer.Tick += (sender, e) => {
                    this.CallTime = this.CallTime.Add(new TimeSpan(0, 0, 1));
                    handler(this.CallTime.ToString(@"mm\:ss"));
                };
                this.CallDurationTimer.Tag = this.LineId;
                this.CallDurationTimer.Interval = 1000;
            }

            this.CallTime = new TimeSpan(0, 0, 0);
            handler(this.CallTime.ToString(@"mm\:ss"));

            this.CallDurationTimer.Start();
        }

        public void DisposeTimer() {
            if (this.CallDurationTimer != null) {
                this.CallDurationTimer.Dispose();
                this.CallDurationTimer = null;
            }
        }

        public Action<int /*number*/, bool /*calling*/, bool /*established*/, bool /*held*/> UpdatePanel;
        public PhoneLine(Action<int /*number*/, bool /*calling*/, bool /*established*/, bool /*held*/> updatePanel) {
            UpdatePanel = updatePanel;
        }
    }

    public static class PhoneLinesHelper {

        public static int MaxLines = 0;
        public static bool MultilineMode = true;

        private static List<PhoneLine> Lines = new List<PhoneLine>();

        public static Action<bool, bool> UpdateShortcuts;
        public static Action<int /*number*/, bool /*calling*/, bool /*established*/, bool /*held*/> UpdatePanel;

        public static void SetMaxLines(int max) {
            MaxLines = max;
        }

        public static void Reset() {
            foreach (var line in Lines) {
                line?.DisposeTimer();
            }
        }

        public static void Reset(int lineNumber) {
            if (Lines[lineNumber] != null) {
                Lines[lineNumber].DisposeTimer();
            }
            Lines[lineNumber] = null;
            UpdatePanel(lineNumber, false, false, false);
        }
        public static PhoneLine ReserveLine(int lineId) {
            if (Lines.Any(e => e != null && e.LineId == lineId)) {
                return null;
            }
            var line = new PhoneLine(UpdatePanel) { LineId = lineId };
            for (int i = 0; i < MaxLines; i++) {
                if (i < Lines.Count) {
                    if (Lines[i] == null) {
                        Lines[i] = line;
                        line.Number = i;
                        return line;
                    }
                } else {
                    Lines.Add(line);
                    line.Number = i;
                    return line;
                }
            }
            return line;
        }


        public static PhoneLine ReserveLine(bool multilineMode = false) {
            if (MultilineMode == false && multilineMode == false) {
                // single call
                var line = new PhoneLine(UpdatePanel) { Number = 0 };
                if (Lines.Any() == false) {
                    Lines.Add(line);
                } else {
                    Lines[0] = line;
                }
                return line;
            } else {
                // multi call
                var line = new PhoneLine(UpdatePanel);
                for (int i = 0; i < MaxLines; i++) {
                    if (i < Lines.Count) {
                        if (Lines[i] == null) {
                            Lines[i] = line;
                            line.Number = i;
                            return line;
                        }
                    } else {
                        Lines.Add(line);
                        line.Number = i;
                        return line;
                    }
                }
                return null;
            }
        }

        public static PhoneLine GetLine(int? lineId) {
            return Lines.FirstOrDefault(e => e != null && e.LineId == lineId);
        }

        public static PhoneLine GetActiveLine() {
            return Lines.FirstOrDefault(e => e != null && e.LineId != null && e.CallHeld == false && e.CallEstablished);
        }

        public static List<PhoneLine> GetActiveLines() {
            return Lines.Where(e => e != null && e.LineId != null && e.CallHeld == false && e.CallEstablished).ToList();
        }

        public static PhoneLine GetAvailableTransferLine() {
            return Lines.FirstOrDefault(e => e != null && e.LineId != null && (e.CallEstablished || e.Calling) && e.CallHeld == false);
        }

        public static List<PhoneLine> GetAvailableTransferLines() {
            return Lines.Where(e => e != null && e.LineId != null && (e.CallEstablished || e.Calling) && e.CallHeld == false).ToList();
        }

        public static bool GetAvailableAny() {
            return Lines.Any(e => e != null);
        }

        /// <summary>
        ///     Gets a Line by index in List </summary>
        public static PhoneLine GetLineByNumber(int lineNumber) {
            return lineNumber < Lines.Count ? Lines[lineNumber] : null;
        }

        public static PhoneLine GetLineByPhone(string phone) {
            return Lines.FirstOrDefault(e => e != null && e.Phone == phone);
        }

        public static PhoneLine GetLineByConnectionId(int connectionId) {
            return Lines.FirstOrDefault(e => e != null && e.ConnectionIds.Contains(connectionId));
        }

        public static List<int> GetAllConnectionIds() {
            return Lines.Where(e => e != null).SelectMany(e => e.ConnectionIds).ToList();
        }

        public static bool GetShortcutAutoTransferState() {
            if (MultilineMode) {
                return Lines.Count(e => e != null && (e.CallEstablished || e.Calling) && e.CallHeld == false) == 1;
            } else {
                return Lines.Any(e => e != null && (e.CallEstablished || e.Calling));
            }
        }

        public static bool GetShortcutCallState() {
            return Lines.Any(e => e != null && (e.CallEstablished || e.Calling)) == false;
        }
    }
}
