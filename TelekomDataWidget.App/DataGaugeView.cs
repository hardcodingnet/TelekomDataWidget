using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace TelekomDataWidget.App
{
    public class DataGaugeView : View
    {
        #region Fields

        private readonly Typeface _font;

        private readonly Rect _dataTextBounds = new Rect();
        private readonly Rect _dataUnitTextBounds = new Rect();
        private readonly Rect _timeTextBounds = new Rect();
        private readonly Rect _timeUnitTextBounds = new Rect();

        private Color _backgroundColor;
        private Color _dataColor;
        private Color _timeColor;

        private Paint _backgroundPaint;
        private Paint _dataGaugePaint;
        private Paint _dataAmountPaint;
        private Paint _dataUnitPaint;
        private Paint _timeGaugePaint;
        private Paint _timePaint;
        private Paint _timeUnitPaint;

        private string _usedAmountText;
        private string _usedAmountUnitText;
        private string _totalAmountText;
        private string _totalAmountUnitText;

        private float _usedAmountPercent;
        private float _timePercent;

        private int _viewWidth;
        private int _viewHeight;
        private int _viewMargin;
        private int _dataGaugeStrokeWidth;
        private int _timeGaugeStrokeWidth;

        #endregion

        #region Properties

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                _backgroundPaint = new Paint(PaintFlags.AntiAlias) { Color = _backgroundColor };
                _backgroundPaint.SetStyle(Paint.Style.Fill);

                Invalidate();
            }
        }

        public Color DataGaugeColor
        {
            get => _dataColor;
            set
            {
                _dataColor = value;
                SetDataPaints(value);
                Invalidate();
            }
        }

        public Color TimeGaugeColor
        {
            get => _timeColor;
            set
            {
                _timeColor = value;
                SetTimePaints(value);
                Invalidate();
            }
        }

        public string UsedAmoutText
        {
            get => _usedAmountText;
            set
            {
                if (_usedAmountText != value)
                {
                    _usedAmountText = value;
                    Invalidate();
                }
            }
        }

        public string UsedAmoutUnitText
        {
            get => _usedAmountUnitText;
            set
            {
                if (_usedAmountUnitText != value)
                {
                    _usedAmountUnitText = value;
                    Invalidate();
                }
            }
        }

        public string TotalAmoutText
        {
            get => _totalAmountText;
            set
            {
                if (_totalAmountText != value)
                {
                    _totalAmountText = value;
                    Invalidate();
                }
            }
        }

        public string TotalAmoutUnitText
        {
            get => _totalAmountUnitText;
            set
            {
                if (_totalAmountUnitText != value)
                {
                    _totalAmountUnitText = value;
                    Invalidate();
                }
            }
        }

        public float UsedAmountPercent
        {
            get => _usedAmountPercent;
            set
            {
                if (Math.Abs(_usedAmountPercent - value) > 0.001)
                {
                    if (value < 0)
                        _usedAmountPercent = 0;
                    else if (value > 100)
                        _usedAmountPercent = 100;
                    else                       
                        _usedAmountPercent = value;

                    Invalidate();
                }
            }
        }

        public float TimePercent
        {
            get => _timePercent;
            set
            {
                if (Math.Abs(_timePercent - value) > 0.001)
                {
                    if (value < 0)
                        _timePercent = 0;
                    else if (value > 100)
                        _timePercent = 100;
                    else
                        _timePercent = value;

                    Invalidate();
                }
            }
        }

        #endregion

        #region Methods

        public DataGaugeView(Context context) : this(context, null)
        {
        }

        public DataGaugeView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            _font = Typeface.CreateFromAsset(context.Assets, "fonts/20thCenturyFontBold.ttf");

            if (attrs != null)
            {
                var array = context.ObtainStyledAttributes(attrs, Resource.Styleable.DataGaugeView, 0, 0);

                _backgroundColor = array.GetColor(Resource.Styleable.DataGaugeView_bgColor, Color.Black);
                _dataColor = array.GetColor(Resource.Styleable.DataGaugeView_dataGaugeColor, Color.Blue);
                _timeColor = array.GetColor(Resource.Styleable.DataGaugeView_timeGaugeColor, Color.Gray);

                _usedAmountText = array.GetString(Resource.Styleable.DataGaugeView_usedAmount) ?? "0";
                _usedAmountUnitText = array.GetString(Resource.Styleable.DataGaugeView_usedAmountUnit) ?? "MB";
                _totalAmountText = array.GetString(Resource.Styleable.DataGaugeView_totalAmount) ?? "0";
                _totalAmountUnitText = array.GetString(Resource.Styleable.DataGaugeView_totalAmountUnit) ?? "MB";

                _usedAmountPercent = array.GetFloat(Resource.Styleable.DataGaugeView_usedAmountPercent, 0);
                _timePercent = array.GetFloat(Resource.Styleable.DataGaugeView_timePercent, 0);

                array.Recycle();
            }
            else
            {
                _backgroundColor = Color.Black;
                _dataColor = Color.Blue;
                _timeColor = Color.Gray;
            }

            _backgroundPaint = new Paint(PaintFlags.AntiAlias) {Color = _backgroundColor};
            _backgroundPaint.SetStyle(Paint.Style.Fill);
            _dataGaugeStrokeWidth = 10;

            SetDataPaints(_dataColor);
            SetTimePaints(_timeColor);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (MeasureSpec.GetMode(widthMeasureSpec) == MeasureSpecMode.Exactly)
            {
                _viewWidth = MeasureSpec.GetSize(widthMeasureSpec);
                _viewHeight = _viewWidth;
            }
            else if (MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.Exactly)
            {
                _viewHeight = MeasureSpec.GetSize(heightMeasureSpec);
                _viewWidth = _viewHeight;
            }

            _viewMargin = (int) Math.Round(_viewHeight * 0.01);

            _dataGaugeStrokeWidth = (int) Math.Round(_viewWidth * 0.08);
            _timeGaugeStrokeWidth = (int) Math.Round(_dataGaugeStrokeWidth / 3f);
            _dataGaugePaint.StrokeWidth = _dataGaugeStrokeWidth;
            _timeGaugePaint.StrokeWidth = _timeGaugeStrokeWidth;

            _dataAmountPaint.TextSize = _viewHeight / 4f;
            _dataUnitPaint.TextSize = _viewHeight / 8f;

            _timePaint.TextSize = _viewHeight / 6f;
            _timeUnitPaint.TextSize = _viewHeight / 9f;

            SetMeasuredDimension(_viewWidth, _viewHeight);
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (_viewWidth == 0 || _viewHeight == 0)
                return;

            float gaugeMargin = _dataGaugeStrokeWidth * 1.5f;

            float backgroundCircleRadius = (_viewHeight - 2f * _viewMargin) / 2f;

            float dataArcLeft = _viewWidth / 2f - backgroundCircleRadius + gaugeMargin;
            float dataArcRight = _viewWidth / 2f + backgroundCircleRadius - gaugeMargin;
            float dataArcTop = _viewHeight / 2f - backgroundCircleRadius + gaugeMargin;
            float dataArcBottom = _viewHeight / 2f + backgroundCircleRadius - gaugeMargin;

            Path bgPath = new Path();
            bgPath.MoveTo(_viewMargin, _viewMargin);
            bgPath.LineTo(_viewWidth / 2f, _viewMargin);
            bgPath.ArcTo(_viewMargin, _viewMargin, _viewWidth - _viewMargin, _viewHeight - _viewMargin, 270, 270, false);
            bgPath.LineTo(_viewMargin, _viewMargin);
            canvas.DrawPath(bgPath, _backgroundPaint);

            float timeArcDist = (_dataGaugeStrokeWidth + _timeGaugeStrokeWidth) / 2f;
            canvas.DrawArc(dataArcLeft, dataArcTop, dataArcRight, dataArcBottom, 270, 360 * _usedAmountPercent / 100, false, _dataGaugePaint);
            canvas.DrawArc(dataArcLeft + timeArcDist, dataArcTop + timeArcDist, dataArcRight - timeArcDist, dataArcBottom - timeArcDist, 270, 360 * _timePercent / 100, false, _timeGaugePaint);

            float dataUnitDistance = _viewHeight / 10f;
            _dataAmountPaint.GetTextBounds(_usedAmountText, 0, _usedAmountText.Length, _dataTextBounds);
            _dataUnitPaint.GetTextBounds(_usedAmountUnitText, 0, _usedAmountUnitText.Length, _dataUnitTextBounds);

            canvas.DrawText(_usedAmountText, _viewWidth / 2f - _dataTextBounds.ExactCenterX(), _viewHeight / 2f - _dataTextBounds.ExactCenterY(), _dataAmountPaint);
            canvas.DrawText(_usedAmountUnitText, _viewWidth / 2f - _dataUnitTextBounds.ExactCenterX(), _viewHeight / 2f + dataUnitDistance + _dataTextBounds.Height() / 2f - _dataUnitTextBounds.ExactCenterY(), _dataUnitPaint);

            _timePaint.GetTextBounds(_totalAmountText, 0, _totalAmountText.Length, _timeTextBounds);
            _timeUnitPaint.GetTextBounds(_totalAmountUnitText, 0, _totalAmountUnitText.Length, _timeUnitTextBounds);

            canvas.DrawText(_totalAmountText, _viewMargin * 2f, _timeTextBounds.Height() + _viewMargin * 3f, _timePaint);
            canvas.DrawText(_totalAmountUnitText, _viewMargin * 2f + _timeTextBounds.Width() * 1.10f, _timeTextBounds.Height() + _viewMargin * 3f, _timeUnitPaint);
        }

        private void SetDataPaints(Color color)
        {
            _dataGaugePaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = _dataColor,
                StrokeCap = Paint.Cap.Butt
            };
            _dataGaugePaint.SetStyle(Paint.Style.Stroke);

            _dataAmountPaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = _dataColor
            };
            _dataAmountPaint.SetTypeface(_font);

            _dataUnitPaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = _dataColor
            };
            _dataUnitPaint.SetTypeface(_font);
        }

        private void SetTimePaints(Color color)
        {
            _timeGaugePaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = _timeColor,
                StrokeCap = Paint.Cap.Butt
            };
            _timeGaugePaint.SetStyle(Paint.Style.Stroke);

            _timePaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = _timeColor
            };
            _timePaint.SetTypeface(_font);

            _timeUnitPaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = _timeColor
            };
            _timeUnitPaint.SetTypeface(_font);
        }

        #endregion
    }
}