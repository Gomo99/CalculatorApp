using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Calculator
{
    public class ModernCalculator : Form
    {
        private Panel displayPanel;
        private Label displayLabel;
        private Label expressionLabel;
        private Panel historyPanel;
        private ListBox historyList;
        private Button historyToggle;

        // Scientific mode additions
        private Panel scientificPanel;
        private Button scientificToggle;
        private List<Control> shiftableControls = new List<Control>();

        private string currentInput = "";
        private double previousValue = 0;
        private string pendingOperator = "";
        private bool newNumber = true;
        private double memory = 0;
        private List<string> calculationHistory = new List<string>();

        // Modern color scheme
        private Color darkBg = Color.FromArgb(25, 25, 35);
        private Color displayBg = Color.FromArgb(35, 35, 50);
        private Color buttonBg = Color.FromArgb(50, 50, 70);
        private Color buttonHover = Color.FromArgb(70, 70, 90);
        private Color accentBlue = Color.FromArgb(100, 150, 255);
        private Color accentOrange = Color.FromArgb(255, 150, 80);
        private Color textColor = Color.FromArgb(230, 230, 240);

        // Panel widths
        private const int PANEL_WIDTH = 250;
        private const int BASE_FORM_WIDTH = 420;

        public ModernCalculator()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Keyboard support
            this.KeyPreview = true;
            this.KeyDown += Form_KeyDown;

            // Form settings
            this.Text = "Modern Calculator";
            this.Size = new Size(BASE_FORM_WIDTH, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = darkBg;
            this.DoubleBuffered = true;
            this.MinimumSize = new Size(BASE_FORM_WIDTH, 400);

            // Set custom programmatically-generated icon
            this.Icon = CreateCalculatorIcon();

            CreateTitleBar();

            // Display panel
            displayPanel = new Panel
            {
                Location = new Point(15, 45),
                Size = new Size(380, 120),
                BackColor = displayBg
            };
            ApplyRoundedCorners(displayPanel, 15);
            this.Controls.Add(displayPanel);
            shiftableControls.Add(displayPanel);

            // Expression label
            expressionLabel = new Label
            {
                Location = new Point(15, 10),
                Size = new Size(350, 30),
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(150, 150, 170),
                TextAlign = ContentAlignment.MiddleRight,
                Text = ""
            };
            displayPanel.Controls.Add(expressionLabel);

            // Display label
            displayLabel = new Label
            {
                Location = new Point(15, 45),
                Size = new Size(350, 60),
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleRight,
                Text = "0"
            };
            displayPanel.Controls.Add(displayLabel);

            // Toggle buttons
            historyToggle = CreateModernButton("📋", 15, 175, 70, 50, accentBlue);
            historyToggle.Click += (s, e) => ToggleHistory();
            this.Controls.Add(historyToggle);
            shiftableControls.Add(historyToggle);

            scientificToggle = CreateModernButton("🧮", 95, 175, 70, 50, accentBlue);
            scientificToggle.Click += (s, e) => ToggleScientific();
            this.Controls.Add(scientificToggle);
            shiftableControls.Add(scientificToggle);

            // History panel (initially closed)
            historyPanel = new Panel
            {
                Location = new Point(BASE_FORM_WIDTH, 45),
                Size = new Size(0, 560),
                BackColor = displayBg,
                Visible = false
            };
            historyPanel.Resize += (s, e) => ApplyRoundedCorners(historyPanel, 15);
            ApplyRoundedCorners(historyPanel, 15);

            historyList = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(230, 505),
                BackColor = displayBg,
                ForeColor = textColor,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 10F),
                ItemHeight = 25
            };
            historyPanel.Controls.Add(historyList);

            Label historyTitle = new Label
            {
                Text = "History",
                Location = new Point(10, 10),
                Size = new Size(230, 25),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            historyPanel.Controls.Add(historyTitle);
            this.Controls.Add(historyPanel);

            // Scientific panel (initially closed)
            scientificPanel = new Panel
            {
                Location = new Point(0, 45),
                Size = new Size(0, 560),
                BackColor = displayBg,
                Visible = false
            };
            scientificPanel.Resize += (s, e) => ApplyRoundedCorners(scientificPanel, 15);
            ApplyRoundedCorners(scientificPanel, 15);
            CreateScientificButtons();
            this.Controls.Add(scientificPanel);

            // Main button layout
            CreateButtonLayout();

            // Enable dragging
            EnableFormDrag(this);
            EnableFormDrag(displayPanel);
        }

        // ========== ICON CREATION ==========
        private Icon CreateCalculatorIcon()
        {
            int size = 64;
            Bitmap bmp = new Bitmap(size, size);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw rounded rectangle background with gradient
                Rectangle rect = new Rectangle(4, 4, size - 8, size - 8);
                GraphicsPath path = GetRoundedRect(rect, 12);

                using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect,
                    Color.FromArgb(100, 150, 255),  // Blue
                    Color.FromArgb(255, 150, 80),   // Orange
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillPath(brush, path);
                }

                // Draw display area
                Rectangle displayRect = new Rectangle(10, 10, size - 20, 16);
                using (SolidBrush displayBrush = new SolidBrush(Color.FromArgb(35, 35, 50)))
                {
                    GraphicsPath displayPath = GetRoundedRect(displayRect, 4);
                    g.FillPath(displayBrush, displayPath);
                }

                // Draw "123" text on display
                using (Font font = new Font("Segoe UI", 9, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Far;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString("123", font, textBrush, displayRect, sf);
                }

                // Draw button grid (3x3)
                int btnSize = 12;
                int btnSpacing = 4;
                int startX = 10;
                int startY = 32;

                using (SolidBrush btnBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                {
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            Rectangle btnRect = new Rectangle(
                                startX + col * (btnSize + btnSpacing),
                                startY + row * (btnSize + btnSpacing),
                                btnSize, btnSize);
                            GraphicsPath btnPath = GetRoundedRect(btnRect, 3);
                            g.FillPath(btnBrush, btnPath);
                        }
                    }
                }

                // Draw equals button (special color)
                using (SolidBrush equalsBrush = new SolidBrush(Color.FromArgb(200, 255, 200, 100)))
                {
                    Rectangle equalsRect = new Rectangle(startX + 3 * (btnSize + btnSpacing), startY + 2 * (btnSize + btnSpacing), btnSize, btnSize);
                    GraphicsPath equalsPath = GetRoundedRect(equalsRect, 3);
                    g.FillPath(equalsBrush, equalsPath);
                }
            }

            // Convert bitmap to icon
            IntPtr hIcon = bmp.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);

            return icon;
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
            path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90);
            path.AddArc(bounds.X + bounds.Width - radius, bounds.Y + bounds.Height - radius, radius, radius, 0, 90);
            path.AddArc(bounds.X, bounds.Y + bounds.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ---------- KEYBOARD HANDLING ----------
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
                HandleKeyInput(((char)('0' + (e.KeyCode - Keys.D0))).ToString());
            else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
                HandleKeyInput(((char)('0' + (e.KeyCode - Keys.NumPad0))).ToString());
            else if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
                HandleKeyInput("+");
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
                HandleKeyInput("-");
            else if (e.KeyCode == Keys.Multiply || (e.KeyCode == Keys.D8 && e.Shift))
                HandleKeyInput("×");
            else if (e.KeyCode == Keys.Divide || e.KeyCode == Keys.OemQuestion)
                HandleKeyInput("/");
            else if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod)
                HandleKeyInput(".");
            else if (e.KeyCode == Keys.Enter)
                HandleKeyInput("=");
            else if (e.KeyCode == Keys.Escape)
                HandleKeyInput("C");
            else if (e.KeyCode == Keys.Back)
                HandleKeyInput("⌫");
            else if (e.KeyCode == Keys.Delete)
                HandleKeyInput("⌫");
        }

        private void HandleKeyInput(string keyText)
        {
            ModernButton dummyBtn = new ModernButton { Text = keyText };

            if (new[] { "±", "%", "⌫", "C" }.Contains(keyText))
                Special_Click(dummyBtn, EventArgs.Empty);
            else if (new[] { "MC", "MR", "M+", "M-" }.Contains(keyText))
                Memory_Click(dummyBtn, EventArgs.Empty);
            else
                Button_Click(dummyBtn, EventArgs.Empty);
        }

        // ---------- SCIENTIFIC BUTTONS ----------
        private void CreateScientificButtons()
        {
            string[] functions = { "sin", "cos", "tan", "log", "sqrt", "x²", "n!", "π", "e" };
            int cols = 3;
            int btnW = 70, btnH = 50, spacing = 10;
            int startX = 10, startY = 50;

            for (int i = 0; i < functions.Length; i++)
            {
                int row = i / cols;
                int col = i % cols;
                var btn = new ModernButton
                {
                    Text = functions[i],
                    Size = new Size(btnW, btnH),
                    Location = new Point(startX + col * (btnW + spacing), startY + row * (btnH + spacing)),
                    BaseColor = buttonBg,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = textColor,
                    Cursor = Cursors.Hand
                };
                btn.Click += ScientificFunction_Click;
                scientificPanel.Controls.Add(btn);
            }

            Label sciTitle = new Label
            {
                Text = "Scientific",
                Location = new Point(10, 10),
                Size = new Size(230, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            scientificPanel.Controls.Add(sciTitle);
        }

        private void ScientificFunction_Click(object sender, EventArgs e)
        {
            ModernButton btn = sender as ModernButton;
            string func = btn.Text;

            if (!double.TryParse(currentInput, out double val))
                return;

            double result = 0;
            bool success = true;

            switch (func)
            {
                case "sin":
                    result = Math.Sin(val * Math.PI / 180.0);
                    break;
                case "cos":
                    result = Math.Cos(val * Math.PI / 180.0);
                    break;
                case "tan":
                    result = Math.Tan(val * Math.PI / 180.0);
                    break;
                case "log":
                    if (val > 0) result = Math.Log10(val);
                    else success = false;
                    break;
                case "sqrt":
                    if (val >= 0) result = Math.Sqrt(val);
                    else success = false;
                    break;
                case "x²":
                    result = val * val;
                    break;
                case "n!":
                    if (val >= 0 && val == Math.Floor(val) && val <= 170)
                        result = Factorial((int)val);
                    else success = false;
                    break;
                case "π":
                    result = Math.PI;
                    break;
                case "e":
                    result = Math.E;
                    break;
                default:
                    success = false;
                    break;
            }

            if (success)
            {
                currentInput = result.ToString();
                displayLabel.Text = FormatResult(result);
                newNumber = true;
            }
        }

        private double Factorial(int n)
        {
            if (n == 0 || n == 1) return 1;
            double f = 1;
            for (int i = 2; i <= n; i++) f *= i;
            return f;
        }

        // ---------- TOGGLE LOGIC ----------
        private void ToggleScientific()
        {
            scientificPanel.Visible = !scientificPanel.Visible;

            if (scientificPanel.Visible)
            {
                Timer timer = new Timer { Interval = 10 };
                int targetWidth = PANEL_WIDTH;
                timer.Tick += (s, e) =>
                {
                    if (scientificPanel.Width < targetWidth)
                    {
                        int step = 25;
                        scientificPanel.Width += step;
                        this.Width += step;

                        foreach (var ctrl in shiftableControls)
                            ctrl.Left += step;

                        if (historyPanel.Visible && historyPanel.Width > 0)
                            historyPanel.Left = this.ClientSize.Width - historyPanel.Width;
                    }
                    else
                    {
                        scientificPanel.Width = targetWidth;
                        timer.Stop();
                    }
                };
                timer.Start();
            }
            else
            {
                Timer timer = new Timer { Interval = 10 };
                timer.Tick += (s, e) =>
                {
                    if (scientificPanel.Width > 0)
                    {
                        int step = 25;
                        if (scientificPanel.Width < step) step = scientificPanel.Width;
                        scientificPanel.Width -= step;
                        this.Width -= step;

                        foreach (var ctrl in shiftableControls)
                            ctrl.Left -= step;

                        if (historyPanel.Visible && historyPanel.Width > 0)
                            historyPanel.Left = this.ClientSize.Width - historyPanel.Width;
                    }
                    else
                    {
                        scientificPanel.Width = 0;
                        timer.Stop();
                    }
                };
                timer.Start();
            }
        }

        private void ToggleHistory()
        {
            historyPanel.Visible = !historyPanel.Visible;

            if (historyPanel.Visible)
            {
                Timer timer = new Timer { Interval = 10 };
                int targetWidth = PANEL_WIDTH;
                timer.Tick += (s, e) =>
                {
                    if (historyPanel.Width < targetWidth)
                    {
                        int step = 25;
                        historyPanel.Width += step;
                        this.Width += step;

                        historyPanel.Left = this.ClientSize.Width - historyPanel.Width;
                    }
                    else
                    {
                        historyPanel.Width = targetWidth;
                        timer.Stop();
                    }
                };
                timer.Start();
            }
            else
            {
                Timer timer = new Timer { Interval = 10 };
                timer.Tick += (s, e) =>
                {
                    if (historyPanel.Width > 0)
                    {
                        int step = 25;
                        if (historyPanel.Width < step) step = historyPanel.Width;
                        historyPanel.Width -= step;
                        this.Width -= step;

                        historyPanel.Left = this.ClientSize.Width - historyPanel.Width;
                    }
                    else
                    {
                        historyPanel.Width = 0;
                        timer.Stop();
                    }
                };
                timer.Start();
            }
        }

        // ---------- UI & CALCULATOR LOGIC ----------
        private void CreateTitleBar()
        {
            Panel titleBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(this.Width, 35),
                BackColor = Color.FromArgb(30, 30, 45)
            };

            Label titleLabel = new Label
            {
                Text = "● Modern Calculator",
                Location = new Point(10, 8),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 10F),
                ForeColor = textColor
            };
            titleBar.Controls.Add(titleLabel);

            Button closeBtn = new Button
            {
                Text = "✕",
                Location = new Point(this.Width - 40, 5),
                Size = new Size(35, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 12F),
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 50, 50);
            closeBtn.Click += (s, e) => Application.Exit();
            titleBar.Controls.Add(closeBtn);

            Button minBtn = new Button
            {
                Text = "−",
                Location = new Point(this.Width - 75, 5),
                Size = new Size(35, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            minBtn.FlatAppearance.BorderSize = 0;
            minBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 80);
            minBtn.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            titleBar.Controls.Add(minBtn);

            this.Controls.Add(titleBar);
            EnableFormDrag(titleBar);
            EnableFormDrag(titleLabel);
        }

        private void CreateButtonLayout()
        {
            int startX = 95;
            int startY = 175;
            int buttonWidth = 70;
            int buttonHeight = 50;
            int spacing = 10;

            var memoryButtons = new[] { "MC", "MR", "M+", "M-" };
            for (int i = 0; i < memoryButtons.Length; i++)
            {
                var btn = CreateModernButton(memoryButtons[i],
                    startX + i * (buttonWidth + spacing), startY,
                    buttonWidth, buttonHeight, accentBlue);
                btn.Click += Memory_Click;
                this.Controls.Add(btn);
                shiftableControls.Add(btn);
            }

            startY += buttonHeight + spacing;
            var specialButtons = new[] { "±", "%", "⌫", "C" };
            var specialColors = new[] { buttonBg, buttonBg, accentOrange, accentOrange };

            for (int i = 0; i < specialButtons.Length; i++)
            {
                var btn = CreateModernButton(specialButtons[i],
                    startX + i * (buttonWidth + spacing), startY,
                    buttonWidth, buttonHeight, specialColors[i]);
                btn.Click += Special_Click;
                this.Controls.Add(btn);
                shiftableControls.Add(btn);
            }

            var buttonLayout = new[]
            {
                new[] { "7", "8", "9", "/" },
                new[] { "4", "5", "6", "×" },
                new[] { "1", "2", "3", "-" },
                new[] { "0", ".", "=", "+" }
            };

            startY += buttonHeight + spacing;
            for (int row = 0; row < buttonLayout.Length; row++)
            {
                for (int col = 0; col < buttonLayout[row].Length; col++)
                {
                    string text = buttonLayout[row][col];
                    Color btnColor = buttonBg;

                    if (text == "/" || text == "×" || text == "-" || text == "+")
                        btnColor = accentBlue;
                    else if (text == "=")
                        btnColor = accentOrange;

                    int width = (text == "0") ? (buttonWidth * 2 + spacing) : buttonWidth;

                    var btn = CreateModernButton(text,
                        startX + col * (buttonWidth + spacing),
                        startY + row * (buttonHeight + spacing),
                        width, buttonHeight, btnColor);

                    btn.Click += Button_Click;
                    this.Controls.Add(btn);
                    shiftableControls.Add(btn);
                }
            }
        }

        private ModernButton CreateModernButton(string text, int x, int y, int width, int height, Color color)
        {
            return new ModernButton
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BaseColor = color,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = textColor,
                Cursor = Cursors.Hand
            };
        }

        private void Button_Click(object sender, EventArgs e)
        {
            ModernButton btn = sender as ModernButton;
            string text = btn.Text;

            if (char.IsDigit(text[0]))
            {
                if (newNumber || displayLabel.Text == "0")
                {
                    displayLabel.Text = text;
                    currentInput = text;
                    newNumber = false;
                }
                else
                {
                    displayLabel.Text += text;
                    currentInput += text;
                }
            }
            else if (text == ".")
            {
                if (newNumber)
                {
                    displayLabel.Text = "0.";
                    currentInput = "0.";
                    newNumber = false;
                }
                else if (!currentInput.Contains("."))
                {
                    displayLabel.Text += ".";
                    currentInput += ".";
                }
            }
            else if (text == "/" || text == "×" || text == "-" || text == "+")
            {
                HandleOperator(text);
            }
            else if (text == "=")
            {
                PerformEquals();
            }
        }

        private void Special_Click(object sender, EventArgs e)
        {
            ModernButton btn = sender as ModernButton;
            string text = btn.Text;

            if (text == "C") Clear();
            else if (text == "⌫")
            {
                if (!newNumber && currentInput.Length > 0)
                {
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                    displayLabel.Text = currentInput.Length > 0 ? currentInput : "0";
                    if (currentInput.Length == 0) newNumber = true;
                }
            }
            else if (text == "±")
            {
                if (!newNumber && !string.IsNullOrEmpty(currentInput))
                {
                    if (double.TryParse(currentInput, out double val))
                    {
                        val = -val;
                        currentInput = val.ToString();
                        displayLabel.Text = currentInput;
                    }
                }
            }
            else if (text == "%")
            {
                if (!string.IsNullOrEmpty(currentInput))
                {
                    if (double.TryParse(currentInput, out double val))
                    {
                        val = val / 100;
                        currentInput = val.ToString();
                        displayLabel.Text = FormatResult(val);
                    }
                }
            }
        }

        private void Memory_Click(object sender, EventArgs e)
        {
            ModernButton btn = sender as ModernButton;
            string text = btn.Text;

            if (text == "MC") memory = 0;
            else if (text == "MR")
            {
                displayLabel.Text = FormatResult(memory);
                currentInput = memory.ToString();
                newNumber = false;
            }
            else if (text == "M+" || text == "M-")
            {
                if (double.TryParse(currentInput, out double val))
                    memory += (text == "M+") ? val : -val;
            }
        }

        private void HandleOperator(string op)
        {
            if (!string.IsNullOrEmpty(currentInput))
            {
                if (!double.TryParse(currentInput, out double currentNumber))
                    return;

                if (!string.IsNullOrEmpty(pendingOperator))
                {
                    double result = PerformOperation(previousValue, currentNumber, pendingOperator);
                    previousValue = result;
                    displayLabel.Text = FormatResult(result);
                    expressionLabel.Text = $"{FormatResult(result)} {op}";
                }
                else
                {
                    previousValue = currentNumber;
                    expressionLabel.Text = $"{FormatResult(currentNumber)} {op}";
                }
            }
            pendingOperator = op;
            newNumber = true;
        }

        private void PerformEquals()
        {
            if (string.IsNullOrEmpty(pendingOperator) || string.IsNullOrEmpty(currentInput))
                return;

            if (!double.TryParse(currentInput, out double secondOperand))
                return;

            string fullExpression = $"{FormatResult(previousValue)} {pendingOperator} {FormatResult(secondOperand)}";
            double result = PerformOperation(previousValue, secondOperand, pendingOperator);

            displayLabel.Text = FormatResult(result);
            expressionLabel.Text = $"{fullExpression} =";

            calculationHistory.Insert(0, $"{fullExpression} = {FormatResult(result)}");
            if (calculationHistory.Count > 50) calculationHistory.RemoveAt(50);
            UpdateHistoryList();

            currentInput = result.ToString();
            previousValue = result;
            pendingOperator = "";
            newNumber = true;
        }

        private double PerformOperation(double a, double b, string op)
        {
            switch (op)
            {
                case "+": return a + b;
                case "-": return a - b;
                case "×": return a * b;
                case "/": return b != 0 ? a / b : 0;
                default: return b;
            }
        }

        private void Clear()
        {
            currentInput = "";
            previousValue = 0;
            pendingOperator = "";
            newNumber = true;
            displayLabel.Text = "0";
            expressionLabel.Text = "";
        }

        private string FormatResult(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return "Error";
            if (value == Math.Floor(value) && Math.Abs(value) < 1e10)
                return value.ToString("0");
            return value.ToString("0.##########");
        }

        private void UpdateHistoryList()
        {
            historyList.Items.Clear();
            foreach (var item in calculationHistory)
                historyList.Items.Add(item);
        }

        private void ApplyRoundedCorners(Control control, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, control.Width, control.Height);
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            control.Region = new Region(path);
        }

        private void EnableFormDrag(Control control)
        {
            Point lastPoint = Point.Empty;
            control.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    lastPoint = e.Location;
            };
            control.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && lastPoint != Point.Empty)
                {
                    this.Left += e.X - lastPoint.X;
                    this.Top += e.Y - lastPoint.Y;
                }
            };
        }

      
    }

    // Custom button class
    public class ModernButton : Button
    {
        public Color BaseColor { get; set; }
        private bool isHovered = false;

        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = BaseColor;

            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            int radius = 10;
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            this.Region = new Region(path);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            this.BackColor = LightenColor(BaseColor, 20);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            this.BackColor = BaseColor;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.BackColor = LightenColor(BaseColor, 35);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this.BackColor = isHovered ? LightenColor(BaseColor, 20) : BaseColor;
        }

        private Color LightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount)
            );
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            int radius = 10;
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            this.Region = new Region(path);
        }
    }
}