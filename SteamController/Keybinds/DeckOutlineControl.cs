using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace SteamController.Keybinds
{
    /// Draws an outline of the Steam Deck with clickable buttons and
    /// a callout for every button describing its current mapping
    internal class DeckOutlineControl : Control
    {
        private const float DesignWidth = 1400;
        private const float DesignHeight = 540;
        private const float CalloutWidth = 188;
        private const float CalloutHeight = 40;
        private const float CalloutStep = 44;

        private enum ShapeKind { Circle, Rect, Round }

        private class ButtonShape
        {
            public DeckButton Button;
            public RectangleF Rect;
            public ShapeKind Kind;
            public String Label = "";
            public RectangleF Callout;
            public bool Back;
        }

        private static readonly Color BodyColor = Color.FromArgb(58, 61, 66);
        private static readonly Color OutlineColor = Color.FromArgb(28, 30, 33);
        private static readonly Color ScreenColor = Color.FromArgb(24, 26, 29);
        private static readonly Color ButtonColor = Color.FromArgb(96, 101, 110);
        private static readonly Color AccentColor = Color.FromArgb(26, 159, 255);
        private static readonly Color PressedColor = Color.FromArgb(255, 160, 0);
        private static readonly Color DisabledColor = Color.FromArgb(150, 150, 150);

        private readonly List<ButtonShape> shapes = new List<ButtonShape>();
        private DeckButton? hoverButton;
        private DeckButton selectedButton = DeckButton.A;
        private HashSet<DeckButton> pressedButtons = new HashSet<DeckButton>();

        /// Returns the text of a callout and if it is user defined
        public Func<DeckButton, (String text, bool custom)>? Describe { get; set; }

        /// Returns if the button can be bound in the current layer
        public Func<DeckButton, bool>? CanBind { get; set; }

        public String Caption { get; set; } = "";

        public event Action<DeckButton>? ButtonSelected;

        public DeckButton SelectedButton
        {
            get { return selectedButton; }
            set
            {
                if (selectedButton == value)
                    return;
                selectedButton = value;
                Invalidate();
                ButtonSelected?.Invoke(value);
            }
        }

        /// Buttons currently held on the device, highlighted in the outline
        public IEnumerable<DeckButton> PressedButtons
        {
            set
            {
                var newPressed = new HashSet<DeckButton>(value);
                if (newPressed.SetEquals(pressedButtons))
                    return;
                pressedButtons = newPressed;
                Invalidate();
            }
        }

        public DeckOutlineControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.Selectable, true);
            BackColor = SystemColors.Window;
            Cursor = Cursors.Hand;

            // Left side, ordered top to bottom as the callouts are
            AddLeft(DeckButton.L2, ShapeKind.Round, new RectangleF(310, 12, 130, 28), "L2");
            AddLeft(DeckButton.L1, ShapeKind.Round, new RectangleF(300, 40, 150, 24), "L1");
            AddLeft(DeckButton.Menu, ShapeKind.Round, new RectangleF(315, 76, 40, 20), "View");
            AddLeft(DeckButton.DPadUp, ShapeKind.Rect, new RectangleF(336, 105, 28, 30), "");
            AddLeft(DeckButton.LeftStick, ShapeKind.Circle, new RectangleF(427, 97, 76, 76), "L3");
            AddLeft(DeckButton.DPadLeft, ShapeKind.Rect, new RectangleF(306, 131, 30, 28), "");
            AddLeft(DeckButton.DPadRight, ShapeKind.Rect, new RectangleF(364, 131, 30, 28), "");
            AddLeft(DeckButton.DPadDown, ShapeKind.Rect, new RectangleF(336, 155, 28, 30), "");
            AddLeft(DeckButton.L4, ShapeKind.Round, new RectangleF(205, 230, 57, 32), "L4", true);
            AddLeft(DeckButton.LeftPad, ShapeKind.Round, new RectangleF(360, 220, 110, 110), "Pad");
            AddLeft(DeckButton.Steam, ShapeKind.Round, new RectangleF(490, 345, 52, 24), "STEAM");
            AddLeft(DeckButton.L5, ShapeKind.Round, new RectangleF(205, 380, 57, 32), "L5", true);

            // Right side, mirrored
            AddRight(DeckButton.R2, ShapeKind.Round, new RectangleF(310, 12, 130, 28), "R2");
            AddRight(DeckButton.R1, ShapeKind.Round, new RectangleF(300, 40, 150, 24), "R1");
            AddRight(DeckButton.Options, ShapeKind.Round, new RectangleF(315, 76, 40, 20), "Opt");
            AddRight(DeckButton.Y, ShapeKind.Circle, new RectangleF(333, 91, 34, 34), "Y");
            AddRight(DeckButton.RightStick, ShapeKind.Circle, new RectangleF(427, 97, 76, 76), "R3");
            AddRight(DeckButton.B, ShapeKind.Circle, new RectangleF(296, 128, 34, 34), "B");
            AddRight(DeckButton.X, ShapeKind.Circle, new RectangleF(370, 128, 34, 34), "X");
            AddRight(DeckButton.A, ShapeKind.Circle, new RectangleF(333, 165, 34, 34), "A");
            AddRight(DeckButton.R4, ShapeKind.Round, new RectangleF(205, 230, 57, 32), "R4", true);
            AddRight(DeckButton.RightPad, ShapeKind.Round, new RectangleF(360, 220, 110, 110), "Pad");
            AddRight(DeckButton.QuickAccess, ShapeKind.Round, new RectangleF(490, 345, 52, 24), "···");
            AddRight(DeckButton.R5, ShapeKind.Round, new RectangleF(205, 380, 57, 32), "R5", true);
        }

        private void AddLeft(DeckButton button, ShapeKind kind, RectangleF rect, String label, bool back = false)
        {
            int index = shapes.Count((s) => s.Callout.X < DesignWidth / 2);
            shapes.Add(new ButtonShape()
            {
                Button = button,
                Kind = kind,
                Rect = rect,
                Label = label,
                Back = back,
                Callout = new RectangleF(4, 8 + index * CalloutStep, CalloutWidth, CalloutHeight)
            });
        }

        private void AddRight(DeckButton button, ShapeKind kind, RectangleF rect, String label, bool back = false)
        {
            int index = shapes.Count((s) => s.Callout.X > DesignWidth / 2);
            shapes.Add(new ButtonShape()
            {
                Button = button,
                Kind = kind,
                Rect = new RectangleF(DesignWidth - rect.Right, rect.Y, rect.Width, rect.Height),
                Label = label,
                Back = back,
                Callout = new RectangleF(DesignWidth - 4 - CalloutWidth, 8 + index * CalloutStep, CalloutWidth, CalloutHeight)
            });
        }

        private float DrawScale
        {
            get { return Math.Max(0.01f, Math.Min(ClientSize.Width / DesignWidth, ClientSize.Height / DesignHeight)); }
        }

        private PointF Offset
        {
            get
            {
                return new PointF(
                    (ClientSize.Width - DesignWidth * DrawScale) / 2,
                    (ClientSize.Height - DesignHeight * DrawScale) / 2);
            }
        }

        private ButtonShape? HitTest(Point point)
        {
            var offset = Offset;
            var design = new PointF((point.X - offset.X) / DrawScale, (point.Y - offset.Y) / DrawScale);

            // Smaller shapes first, so that the D-pad wins over the body
            foreach (var shape in shapes.OrderBy((s) => s.Rect.Width * s.Rect.Height))
            {
                if (shape.Rect.Contains(design) || shape.Callout.Contains(design))
                    return shape;
            }
            return null;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var hover = HitTest(e.Location)?.Button;
            if (hover != hoverButton)
            {
                hoverButton = hover;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoverButton = null;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            var shape = HitTest(e.Location);
            if (shape is not null)
                SelectedButton = shape.Button;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            var offset = Offset;
            g.TranslateTransform(offset.X, offset.Y);
            g.ScaleTransform(DrawScale, DrawScale);

            DrawBody(g);

            using (var labelFont = new Font("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var smallFont = new Font("Segoe UI", 11, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var calloutFont = new Font("Segoe UI", 14, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                foreach (var shape in shapes)
                    DrawButton(g, shape, labelFont);

                foreach (var shape in shapes)
                    DrawCallout(g, shape, smallFont, calloutFont);

                if (Caption != "")
                {
                    var screen = new RectangleF(540, 95, 320, 205);
                    using (var format = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (var brush = new SolidBrush(Color.FromArgb(200, 205, 210)))
                    {
                        g.DrawString(Caption, calloutFont, brush, screen, format);
                    }
                }
            }
        }

        private static GraphicsPath RoundedRect(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void DrawBody(Graphics g)
        {
            var body = new RectangleF(270, 60, 860, 270);
            var leftGrip = new RectangleF(270, 180, 290, 320);
            var rightGrip = new RectangleF(840, 180, 290, 320);

            // Draw outlines of all parts first, then fill them over,
            // so that only the outer edge of the union stays visible
            using (var bodyPath = RoundedRect(body, 70))
            using (var outline = new Pen(OutlineColor, 8))
            using (var fill = new SolidBrush(BodyColor))
            {
                g.DrawPath(outline, bodyPath);
                g.DrawEllipse(outline, leftGrip);
                g.DrawEllipse(outline, rightGrip);
                g.FillPath(fill, bodyPath);
                g.FillEllipse(fill, leftGrip);
                g.FillEllipse(fill, rightGrip);
            }

            using (var screenPath = RoundedRect(new RectangleF(540, 95, 320, 205), 8))
            using (var screen = new SolidBrush(ScreenColor))
            {
                g.FillPath(screen, screenPath);
            }

            // Back buttons are on the other side of the device
            using (var font = new Font("Segoe UI", 11, FontStyle.Italic, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(DisabledColor))
            using (var format = new StringFormat() { Alignment = StringAlignment.Center })
            {
                g.DrawString("back", font, brush, new RectangleF(205, 214, 57, 16), format);
                g.DrawString("back", font, brush, new RectangleF(DesignWidth - 262, 214, 57, 16), format);
            }
        }

        private Color ShapeColor(ButtonShape shape)
        {
            if (pressedButtons.Contains(shape.Button))
                return PressedColor;
            if (shape.Button == selectedButton)
                return AccentColor;
            if (shape.Button == hoverButton)
                return ControlPaint.Light(ButtonColor);
            return ButtonColor;
        }

        private void DrawButton(Graphics g, ButtonShape shape, Font font)
        {
            using (var brush = new SolidBrush(ShapeColor(shape)))
            using (var outline = new Pen(OutlineColor, 2))
            {
                if (shape.Back)
                    outline.DashStyle = DashStyle.Dash;

                switch (shape.Kind)
                {
                    case ShapeKind.Circle:
                        g.FillEllipse(brush, shape.Rect);
                        g.DrawEllipse(outline, shape.Rect);
                        break;

                    case ShapeKind.Rect:
                        g.FillRectangle(brush, shape.Rect);
                        g.DrawRectangle(outline, shape.Rect.X, shape.Rect.Y, shape.Rect.Width, shape.Rect.Height);
                        break;

                    default:
                        using (var path = RoundedRect(shape.Rect, Math.Min(14, shape.Rect.Height / 2)))
                        {
                            g.FillPath(brush, path);
                            g.DrawPath(outline, path);
                        }
                        break;
                }
            }

            if (shape.Label != "")
            {
                using (var format = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(shape.Label, font, Brushes.White, shape.Rect, format);
                }
            }
        }

        private void DrawCallout(Graphics g, ButtonShape shape, Font labelFont, Font textFont)
        {
            var (text, custom) = Describe?.Invoke(shape.Button) ?? ("", false);
            bool canBind = CanBind?.Invoke(shape.Button) ?? true;
            bool selected = shape.Button == selectedButton;
            bool left = shape.Callout.X < DesignWidth / 2;

            // Leader line from the callout to the button
            var from = new PointF(left ? shape.Callout.Right : shape.Callout.Left, shape.Callout.Y + shape.Callout.Height / 2);
            var to = new PointF(left ? shape.Rect.Left : shape.Rect.Right, shape.Rect.Y + shape.Rect.Height / 2);
            using (var line = new Pen(selected ? AccentColor : Color.FromArgb(90, 120, 120, 120), selected ? 2 : 1))
            {
                g.DrawLine(line, from, to);
            }

            using (var path = RoundedRect(shape.Callout, 6))
            using (var fill = new SolidBrush(selected ? Color.FromArgb(225, 241, 255) : SystemColors.Window))
            using (var border = new Pen(selected ? AccentColor : custom ? AccentColor : Color.FromArgb(190, 190, 190), selected || custom ? 2 : 1))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            var inner = RectangleF.Inflate(shape.Callout, -6, -2);
            using (var format = new StringFormat() { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            using (var labelBrush = new SolidBrush(Color.FromArgb(110, 110, 110)))
            using (var textBrush = new SolidBrush(!canBind ? DisabledColor : custom ? AccentColor : Color.FromArgb(40, 40, 40)))
            {
                g.DrawString(KeybindNames.Button(shape.Button), labelFont, labelBrush,
                    new RectangleF(inner.X, inner.Y, inner.Width, 15), format);
                g.DrawString(text, textFont, textBrush,
                    new RectangleF(inner.X, inner.Y + 15, inner.Width, inner.Height - 15), format);
            }
        }
    }
}
