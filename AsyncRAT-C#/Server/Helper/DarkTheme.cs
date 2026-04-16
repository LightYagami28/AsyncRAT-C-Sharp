using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Server.Helper
{
    /// <summary>
    /// Provides a centralized dark "GitHub-style" theme for all WinForms controls.
    /// Call <see cref="Apply"/> once per Form to recursively paint every control,
    /// and <see cref="ApplyDwmDarkTitleBar"/> to darken the native Win10/11 title bar.
    /// </summary>
    public static class DarkTheme
    {
        // ── Palette ─────────────────────────────────────────────────────────────
        public static readonly Color Background      = Color.FromArgb(0x0D, 0x11, 0x17); // #0D1117
        public static readonly Color Surface         = Color.FromArgb(0x16, 0x1B, 0x22); // #161B22
        public static readonly Color SurfaceAlt      = Color.FromArgb(0x21, 0x26, 0x2D); // #21262D
        public static readonly Color BorderColor     = Color.FromArgb(0x30, 0x36, 0x3D); // #30363D
        public static readonly Color TextPrimary     = Color.FromArgb(0xC9, 0xD1, 0xD9); // #C9D1D9
        public static readonly Color TextMuted       = Color.FromArgb(0x8B, 0x94, 0x9E); // #8B949E
        public static readonly Color AccentBlue      = Color.FromArgb(0x1F, 0x6F, 0xEB); // #1F6FEB
        public static readonly Color AccentGreen     = Color.FromArgb(0x3F, 0xB9, 0x50); // #3FB950
        public static readonly Color AccentRed       = Color.FromArgb(0xF8, 0x51, 0x49); // #F85149
        public static readonly Color AccentYellow    = Color.FromArgb(0xD2, 0x9B, 0x22); // #D29B22

        // ── DWM dark title bar ───────────────────────────────────────────────────
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        /// <summary>
        /// Requests a dark title bar from the Desktop Window Manager (Windows 10 1903+ / Windows 11).
        /// Silently ignored on older systems.
        /// </summary>
        public static void ApplyDwmDarkTitleBar(IntPtr handle)
        {
            try
            {
                int value = 1;
                DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
            }
            catch
            {
                // DWM not available (older Windows / Wine) — safe to ignore
            }
        }

        // ── Recursive apply ─────────────────────────────────────────────────────
        /// <summary>
        /// Recursively applies the dark theme to <paramref name="form"/> and all its child controls.
        /// </summary>
        public static void Apply(Form form)
        {
            form.BackColor = Background;
            form.ForeColor = TextPrimary;
            ApplyToControls(form.Controls);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                ApplyToControl(ctrl);
                if (ctrl.HasChildren)
                    ApplyToControls(ctrl.Controls);
            }
        }

        private static void ApplyToControl(Control ctrl)
        {
            switch (ctrl)
            {
                case TabControl tc:
                    tc.BackColor = Background;
                    tc.ForeColor = TextPrimary;
                    break;

                case TabPage tp:
                    tp.BackColor = Surface;
                    tp.ForeColor = TextPrimary;
                    break;

                case ListView lv:
                    lv.BackColor  = Surface;
                    lv.ForeColor  = TextPrimary;
                    lv.GridLines  = true;
                    // Owner-draw header for dark column headers
                    lv.OwnerDraw  = true;
                    lv.DrawColumnHeader -= OnDrawColumnHeader;
                    lv.DrawColumnHeader += OnDrawColumnHeader;
                    lv.DrawItem         -= OnDrawItem;
                    lv.DrawItem         += OnDrawItem;
                    lv.DrawSubItem      -= OnDrawSubItem;
                    lv.DrawSubItem      += OnDrawSubItem;
                    break;

                case StatusStrip ss:
                    ss.BackColor       = Surface;
                    ss.ForeColor       = TextPrimary;
                    ss.SizingGrip      = false;
                    ss.RenderMode      = ToolStripRenderMode.Professional;
                    ss.Renderer        = new DarkToolStripRenderer();
                    foreach (ToolStripItem item in ss.Items)
                        ApplyToToolStripItem(item);
                    break;

                case MenuStrip ms:
                    ms.BackColor  = Surface;
                    ms.ForeColor  = TextPrimary;
                    ms.Renderer   = new DarkToolStripRenderer();
                    foreach (ToolStripItem item in ms.Items)
                        ApplyToToolStripItem(item);
                    break;

                case ContextMenuStrip cms:
                    cms.BackColor = Surface;
                    cms.ForeColor = TextPrimary;
                    cms.Renderer  = new DarkToolStripRenderer();
                    foreach (ToolStripItem item in cms.Items)
                        ApplyToToolStripItem(item);
                    break;

                case TextBox tb:
                    tb.BackColor = SurfaceAlt;
                    tb.ForeColor = TextPrimary;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case RichTextBox rtb:
                    rtb.BackColor = SurfaceAlt;
                    rtb.ForeColor = TextPrimary;
                    break;

                case ComboBox cb:
                    cb.BackColor = SurfaceAlt;
                    cb.ForeColor = TextPrimary;
                    cb.FlatStyle = FlatStyle.Flat;
                    break;

                case Button btn:
                    btn.BackColor = SurfaceAlt;
                    btn.ForeColor = TextPrimary;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = BorderColor;
                    break;

                case CheckBox chk:
                    chk.BackColor = Color.Transparent;
                    chk.ForeColor = TextPrimary;
                    break;

                case Label lbl:
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = TextPrimary;
                    break;

                case Panel pnl:
                    pnl.BackColor = Background;
                    pnl.ForeColor = TextPrimary;
                    break;

                case GroupBox gb:
                    gb.BackColor = Color.Transparent;
                    gb.ForeColor = TextMuted;
                    break;

                default:
                    try
                    {
                        ctrl.BackColor = Background;
                        ctrl.ForeColor = TextPrimary;
                    }
                    catch { /* some controls forbid colour changes */ }
                    break;
            }
        }

        // ── ListView owner-draw ─────────────────────────────────────────────────
        private static void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var bgBrush  = new SolidBrush(SurfaceAlt))
            using (var fgBrush  = new SolidBrush(TextMuted))
            using (var borderPen = new Pen(BorderColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
                e.Graphics.DrawRectangle(borderPen, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));

                var fmt = new StringFormat
                {
                    Alignment     = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming      = StringTrimming.EllipsisCharacter
                };
                var textRect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
                e.Graphics.DrawString(e.Header.Text, e.Font, fgBrush, textRect, fmt);
            }
        }

        private static void OnDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // Let sub-item drawing handle everything; just mark as handled
            e.DrawDefault = false;
        }

        private static void OnDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            bool selected = (e.Item.Selected && e.Item.ListView.Focused) ||
                            (e.Item.Selected && !e.Item.ListView.HideSelection);

            Color bg = selected ? AccentBlue : (e.ItemIndex % 2 == 0 ? Surface : SurfaceAlt);
            Color fg = selected ? Color.White  : TextPrimary;

            using (var bgBrush  = new SolidBrush(bg))
            using (var fgBrush  = new SolidBrush(fg))
            using (var borderPen = new Pen(BorderColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
                e.Graphics.DrawLine(borderPen,
                    new Point(e.Bounds.Left,  e.Bounds.Bottom - 1),
                    new Point(e.Bounds.Right, e.Bounds.Bottom - 1));

                if (e.ColumnIndex == 0 && e.Item.ImageList != null && e.Item.ImageIndex >= 0)
                {
                    int iconSize = 16;
                    int iconY    = e.Bounds.Y + (e.Bounds.Height - iconSize) / 2;
                    e.Item.ImageList.Draw(e.Graphics, e.Bounds.X + 2, iconY, iconSize, iconSize, e.Item.ImageIndex);
                }

                var fmt = new StringFormat
                {
                    Alignment     = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming      = StringTrimming.EllipsisCharacter
                };
                int textPad = (e.ColumnIndex == 0 && e.Item.ImageList != null && e.Item.ImageIndex >= 0) ? 22 : 4;
                var textRect = new Rectangle(e.Bounds.X + textPad, e.Bounds.Y, e.Bounds.Width - textPad, e.Bounds.Height);
                e.Graphics.DrawString(e.SubItem.Text, e.Item.ListView.Font, fgBrush, textRect, fmt);
            }
        }

        // ── ToolStrip item helper ────────────────────────────────────────────────
        private static void ApplyToToolStripItem(ToolStripItem item)
        {
            item.BackColor = Surface;
            item.ForeColor = TextPrimary;

            if (item is ToolStripMenuItem mi)
            {
                foreach (ToolStripItem child in mi.DropDownItems)
                    ApplyToToolStripItem(child);

                if (mi.DropDown != null)
                {
                    mi.DropDown.BackColor = Surface;
                    mi.DropDown.ForeColor = TextPrimary;
                    if (mi.DropDown.Renderer == null ||
                        !(mi.DropDown.Renderer is DarkToolStripRenderer))
                        mi.DropDown.Renderer = new DarkToolStripRenderer();
                }
            }
        }
    }

    // ── Custom renderer (removes the grey gradient on menus/toolstrips) ──────────
    internal sealed class DarkToolStripRenderer : ToolStripProfessionalRenderer
    {
        public DarkToolStripRenderer() : base(new DarkColorTable()) { }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (var pen = new Pen(DarkTheme.BorderColor))
                e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var item = e.Item;
            if (item.Selected || item.Pressed)
            {
                using (var brush = new SolidBrush(DarkTheme.SurfaceAlt))
                    e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, item.Size));
            }
            else
            {
                using (var brush = new SolidBrush(DarkTheme.Surface))
                    e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, item.Size));
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? DarkTheme.TextPrimary : DarkTheme.TextMuted;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            int midY = e.Item.Height / 2;
            using (var pen = new Pen(DarkTheme.BorderColor))
                e.Graphics.DrawLine(pen, 4, midY, e.Item.Width - 4, midY);
        }
    }

    internal sealed class DarkColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected           => DarkTheme.SurfaceAlt;
        public override Color MenuItemBorder             => DarkTheme.BorderColor;
        public override Color MenuBorder                 => DarkTheme.BorderColor;
        public override Color MenuStripGradientBegin     => DarkTheme.Surface;
        public override Color MenuStripGradientEnd       => DarkTheme.Surface;
        public override Color MenuItemSelectedGradientBegin => DarkTheme.SurfaceAlt;
        public override Color MenuItemSelectedGradientEnd   => DarkTheme.SurfaceAlt;
        public override Color MenuItemPressedGradientBegin  => DarkTheme.SurfaceAlt;
        public override Color MenuItemPressedGradientEnd    => DarkTheme.SurfaceAlt;
        public override Color ToolStripDropDownBackground   => DarkTheme.Surface;
        public override Color ImageMarginGradientBegin      => DarkTheme.SurfaceAlt;
        public override Color ImageMarginGradientMiddle     => DarkTheme.SurfaceAlt;
        public override Color ImageMarginGradientEnd        => DarkTheme.SurfaceAlt;
        public override Color SeparatorDark                 => DarkTheme.BorderColor;
        public override Color SeparatorLight                => DarkTheme.BorderColor;
        public override Color StatusStripGradientBegin      => DarkTheme.Surface;
        public override Color StatusStripGradientEnd        => DarkTheme.Surface;
        public override Color CheckBackground               => DarkTheme.AccentBlue;
        public override Color CheckSelectedBackground       => DarkTheme.AccentBlue;
        public override Color CheckPressedBackground        => DarkTheme.AccentBlue;
        public override Color ButtonSelectedBorder          => DarkTheme.BorderColor;
        public override Color ButtonSelectedGradientBegin   => DarkTheme.SurfaceAlt;
        public override Color ButtonSelectedGradientEnd     => DarkTheme.SurfaceAlt;
        public override Color ButtonPressedGradientBegin    => DarkTheme.SurfaceAlt;
        public override Color ButtonPressedGradientEnd      => DarkTheme.SurfaceAlt;
    }
}
