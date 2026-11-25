#nullable enable
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AdminImportaPlus.GUI;

/// <summary>
/// Botón moderno con estética Apple/macOS (esquinas redondeadas suavizadas con antialiasing,
/// transiciones sutiles de hover y tipografía limpia). 100% compatible con Visual Studio Designer.
/// </summary>
public class AppleButton : Button
{
    private bool _isHovered;
    private bool _isPressed;
    private int _cornerRadius = 10;
    private Color _borderColor = Color.Transparent;
    private int _borderThickness = 0;
    private Color _hoverBackColor = Color.Empty;

    [Category("Apple Appearance")]
    [DefaultValue(10)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Math.Max(0, value); Invalidate(); }
    }

    [Category("Apple Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    [Category("Apple Appearance")]
    [DefaultValue(0)]
    public int BorderThickness
    {
        get => _borderThickness;
        set { _borderThickness = Math.Max(0, value); Invalidate(); }
    }

    [Category("Apple Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color HoverBackColor
    {
        get => _hoverBackColor;
        set { _hoverBackColor = value; Invalidate(); }
    }

    public AppleButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Regular);
        BackColor = Color.FromArgb(83, 6, 24);
        ForeColor = Color.White;
    }

    protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _isHovered = false; _isPressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs mevent) { _isPressed = true; Invalidate(); base.OnMouseDown(mevent); }
    protected override void OnMouseUp(MouseEventArgs mevent) { _isPressed = false; Invalidate(); base.OnMouseUp(mevent); }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using (var bgParent = new SolidBrush(Parent?.BackColor ?? Color.FromArgb(245, 245, 247)))
        {
            g.FillRectangle(bgParent, ClientRectangle);
        }

        var rect = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
        using var path = CrearRoundedPath(rect, _cornerRadius);

        Color fill = BackColor;
        if (!Enabled)
        {
            fill = Color.FromArgb(180, BackColor);
        }
        else if (_isPressed)
        {
            fill = ControlPaint.Dark(BackColor, 0.08f);
        }
        else if (_isHovered)
        {
            fill = _hoverBackColor != Color.Empty ? _hoverBackColor : MezclarColor(BackColor, Color.White, 0.12f);
        }

        using (var brush = new SolidBrush(fill))
        {
            g.FillPath(brush, path);
        }

        if (_borderThickness > 0 && _borderColor != Color.Transparent)
        {
            using var pen = new Pen(_borderColor, _borderThickness);
            g.DrawPath(pen, path);
        }

        var textRect = Rectangle.Inflate(ClientRectangle, -Padding.Left - 8, 0);
        TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
        if (TextAlign == ContentAlignment.MiddleLeft) flags |= TextFormatFlags.Left;
        else if (TextAlign == ContentAlignment.MiddleRight) flags |= TextFormatFlags.Right;
        else flags |= TextFormatFlags.HorizontalCenter;

        TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, flags);
    }

    private static Color MezclarColor(Color baseColor, Color target, float amount)
    {
        int r = Math.Clamp((int)(baseColor.R + (target.R - baseColor.R) * amount), 0, 255);
        int g = Math.Clamp((int)(baseColor.G + (target.G - baseColor.G) * amount), 0, 255);
        int b = Math.Clamp((int)(baseColor.B + (target.B - baseColor.B) * amount), 0, 255);
        return Color.FromArgb(baseColor.A, r, g, b);
    }

    public static GraphicsPath CrearRoundedPath(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        float d = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
        if (d <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

/// <summary>
/// Tarjeta contenedora estilo Apple (esquinas redondeadas 14px, borde sutil #E5E7EB, sin caja negra XP).
/// </summary>
public class AppleCardPanel : Panel
{
    private int _cornerRadius = 14;
    private Color _borderColor = Color.FromArgb(229, 231, 235);

    [Category("Apple Appearance")]
    [DefaultValue(14)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Math.Max(0, value); Invalidate(); }
    }

    [Category("Apple Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    public AppleCardPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BorderStyle = BorderStyle.None;
        BackColor = Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var bg = new SolidBrush(Parent?.BackColor ?? Color.FromArgb(245, 245, 247)))
        {
            g.FillRectangle(bg, ClientRectangle);
        }

        var rect = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
        using var path = AppleButton.CrearRoundedPath(rect, _cornerRadius);
        using (var fill = new SolidBrush(BackColor))
        {
            g.FillPath(fill, path);
        }
        using (var pen = new Pen(_borderColor, 1f))
        {
            g.DrawPath(pen, path);
        }
    }
}
