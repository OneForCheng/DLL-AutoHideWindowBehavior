using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ForCheng.Behaviors
{
    [Flags]
    public enum AutoHideDirection
    {
        None = 0,
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8,
        All = 15,
    }

    /// <summary>
    /// 贴边自动隐藏窗口的Behavior
    /// </summary>
    public class AutoHideWindowBehavior : Behavior<Window>
    {
        #region Private enum

        private enum HideState
        {
            None,
            PreviewTopHidden,
            TopHidden,
            PreviewRightHidden,
            RightHidden,
            PreviewBottomHidden,
            BottomHidden,
            PreviewLeftHidden,
            LeftHidden,
        }

        private enum MoveDirection
        {
            Top,
            Right,
            Bottom,
            Left
        }

        #endregion

        #region Private fields
        private HideState _hideStatus;
        private bool _lockTimer;
        private double _autoHideFactor;
        private Win32.Point _curPosition;

        #endregion

        #region Constructor
        public AutoHideWindowBehavior()
        {
            _lockTimer = false;
            _hideStatus = HideState.None;
            _autoHideFactor = 10;
            AutoHideDirection = AutoHideDirection.All;
            var autoHideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            autoHideTimer.Tick += AutoHideTimer_Tick;
            autoHideTimer.Start();
        }

        #endregion

        #region Public properties and methods

        /// <summary>
        /// 自动贴边隐藏的触发距离
        /// </summary>
        public double AutoHideFactor
        {
            get
            {
                return _autoHideFactor;
            }
            set
            {
                if (value > 0)
                {
                    _autoHideFactor = value;
                }
            }
        }

        /// <summary>
        /// 是否处于隐藏状态
        /// </summary>
        public bool IsHide => _hideStatus != HideState.None;

        /// <summary>
        /// 能够自动贴边隐藏的方向，默认All
        /// </summary>
        public AutoHideDirection AutoHideDirection { get; set; }

        /// <summary>
        /// 显示窗口
        /// </summary>
        public void Show()
        {
            AssociatedObject.Show();
            AssociatedObject.Activate();
            if (_hideStatus != HideState.None)
            {
                _lockTimer = true;
                AssociatedObject.Topmost = false;
                switch (_hideStatus)
                {
                    case HideState.PreviewTopHidden:
                        AssociatedObject.Top = _autoHideFactor + 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.TopHidden:
                        AnimationTranslate(MoveDirection.Bottom, AssociatedObject.ActualHeight + _autoHideFactor, () =>
                        {
                            AssociatedObject.Top = _autoHideFactor + 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                    case HideState.PreviewRightHidden:
                        AssociatedObject.Left = SystemParameters.VirtualScreenWidth - AssociatedObject.ActualWidth - _autoHideFactor - 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.RightHidden:
                        AnimationTranslate(MoveDirection.Left, AssociatedObject.ActualWidth + _autoHideFactor, () =>
                        {
                            AssociatedObject.Left = SystemParameters.VirtualScreenWidth - AssociatedObject.ActualWidth - _autoHideFactor - 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                    case HideState.PreviewBottomHidden:
                        AssociatedObject.Top = SystemParameters.VirtualScreenHeight - AssociatedObject.ActualHeight - _autoHideFactor - 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.BottomHidden:
                        AnimationTranslate(MoveDirection.Top, AssociatedObject.ActualHeight + _autoHideFactor, () =>
                        {
                            AssociatedObject.Top = SystemParameters.VirtualScreenHeight - AssociatedObject.ActualHeight - _autoHideFactor - 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                    case HideState.PreviewLeftHidden:
                        AssociatedObject.Left = _autoHideFactor + 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.LeftHidden:
                        AnimationTranslate(MoveDirection.Right, AssociatedObject.ActualWidth + _autoHideFactor, () =>
                        {
                            AssociatedObject.Left = _autoHideFactor + 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                }
            }
        }

        #endregion

        #region Events and Private methods

        private void AutoHideTimer_Tick(object sender, EventArgs e)
        {
            if (!_lockTimer)
            {
                _lockTimer = true;
                if (Win32.GetCursorPos(out _curPosition))
                {
                    switch (_hideStatus)
                    {
                        case HideState.None:
                            if ((AutoHideDirection & AutoHideDirection.Top) == AutoHideDirection.Top && AssociatedObject.Top <= _autoHideFactor)
                            {
                                _hideStatus = HideState.PreviewTopHidden;
                                AssociatedObject.Topmost = true;
                            }
                            else if ((AutoHideDirection & AutoHideDirection.Right) == AutoHideDirection.Right && AssociatedObject.Left + AssociatedObject.ActualWidth >=
                                     SystemParameters.VirtualScreenWidth - _autoHideFactor)
                            {
                                _hideStatus = HideState.PreviewRightHidden;
                                AssociatedObject.Topmost = true;
                            }
                            else if ((AutoHideDirection & AutoHideDirection.Bottom) == AutoHideDirection.Bottom && AssociatedObject.Top + AssociatedObject.ActualHeight >=
                                     SystemParameters.VirtualScreenHeight - _autoHideFactor)
                            {
                                _hideStatus = HideState.PreviewBottomHidden;
                                AssociatedObject.Topmost = true;
                            }
                            else if ((AutoHideDirection & AutoHideDirection.Left) == AutoHideDirection.Left && AssociatedObject.Left <= _autoHideFactor)
                            {
                                _hideStatus = HideState.PreviewLeftHidden;
                                AssociatedObject.Topmost = true;
                            }
                            _lockTimer = false;
                            break;
                        case HideState.PreviewTopHidden:
                            if (AssociatedObject.Top <= _autoHideFactor)
                            {
                                if (_curPosition.X < AssociatedObject.Left ||
                                    _curPosition.X > AssociatedObject.Left + AssociatedObject.ActualWidth ||
                                    _curPosition.Y > AssociatedObject.Top + AssociatedObject.ActualHeight)
                                {
                                    AnimationTranslate(MoveDirection.Top,
                                        AssociatedObject.ActualHeight + _autoHideFactor, () =>
                                        {
                                            AssociatedObject.Top = -(AssociatedObject.ActualHeight + _autoHideFactor);
                                            _hideStatus = HideState.TopHidden;
                                            AssociatedObject.Hide();
                                            _lockTimer = false;
                                        });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                AssociatedObject.Topmost = false;
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.TopHidden:
                            if (_curPosition.Y <= _autoHideFactor &&
                                _curPosition.X >= AssociatedObject.Left &&
                                _curPosition.X <= AssociatedObject.Left + AssociatedObject.ActualWidth)
                            {
                                AssociatedObject.Show();
                                AssociatedObject.Activate();
                                AnimationTranslate(MoveDirection.Bottom, AssociatedObject.ActualHeight + _autoHideFactor,
                                    () =>
                                    {
                                        AssociatedObject.Top = 0;
                                        _hideStatus = HideState.PreviewTopHidden;
                                        _lockTimer = false;
                                    });

                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                        case HideState.PreviewRightHidden:
                            if (AssociatedObject.Left + AssociatedObject.ActualWidth >=
                                SystemParameters.VirtualScreenWidth - _autoHideFactor)
                            {
                                if (_curPosition.X < AssociatedObject.Left ||
                                    _curPosition.Y < AssociatedObject.Top ||
                                    _curPosition.Y > AssociatedObject.Top + AssociatedObject.ActualHeight)
                                {
                                    AnimationTranslate(MoveDirection.Right,
                                        AssociatedObject.ActualHeight + _autoHideFactor, () =>
                                        {
                                            AssociatedObject.Left = (SystemParameters.VirtualScreenWidth +
                                                                     _autoHideFactor);
                                            _hideStatus = HideState.RightHidden;
                                            AssociatedObject.Hide();
                                            _lockTimer = false;
                                        });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                AssociatedObject.Topmost = false;
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.RightHidden:
                            if (_curPosition.X >= SystemParameters.VirtualScreenWidth - _autoHideFactor &&
                                _curPosition.Y >= AssociatedObject.Top &&
                                _curPosition.Y <= AssociatedObject.Top + AssociatedObject.ActualHeight)
                            {
                                AssociatedObject.Show();
                                AssociatedObject.Activate();
                                AnimationTranslate(MoveDirection.Left, AssociatedObject.ActualWidth + _autoHideFactor,
                                    () =>
                                    {
                                        AssociatedObject.Left = SystemParameters.VirtualScreenWidth -
                                                                AssociatedObject.ActualWidth;
                                        _hideStatus = HideState.PreviewRightHidden;
                                        _lockTimer = false;
                                    });

                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                        case HideState.PreviewBottomHidden:
                            if (AssociatedObject.Top + AssociatedObject.ActualHeight >=
                                SystemParameters.VirtualScreenHeight - _autoHideFactor)
                            {
                                if (_curPosition.Y < AssociatedObject.Top ||
                                    _curPosition.X < AssociatedObject.Left ||
                                    _curPosition.X > AssociatedObject.Left + AssociatedObject.ActualWidth)
                                {
                                    AnimationTranslate(MoveDirection.Bottom,
                                        AssociatedObject.ActualHeight + _autoHideFactor, () =>
                                        {
                                            AssociatedObject.Top = (SystemParameters.VirtualScreenHeight +
                                                                    _autoHideFactor);
                                            _hideStatus = HideState.BottomHidden;
                                            AssociatedObject.Hide();
                                            _lockTimer = false;

                                        });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                AssociatedObject.Topmost = false;
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.BottomHidden:
                            if (_curPosition.Y >= SystemParameters.VirtualScreenHeight - _autoHideFactor &&
                                _curPosition.X >= AssociatedObject.Left &&
                                _curPosition.X <= AssociatedObject.Left + AssociatedObject.ActualWidth)
                            {
                                AssociatedObject.Show();
                                AssociatedObject.Activate();
                                AnimationTranslate(MoveDirection.Top, AssociatedObject.ActualHeight + _autoHideFactor,
                                    () =>
                                    {
                                        AssociatedObject.Top = SystemParameters.VirtualScreenHeight -
                                                               AssociatedObject.ActualHeight;
                                        _hideStatus = HideState.PreviewBottomHidden;
                                        _lockTimer = false;
                                    });
                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                        case HideState.PreviewLeftHidden:
                            if (AssociatedObject.Left <= _autoHideFactor)
                            {
                                if (_curPosition.X > AssociatedObject.Left + AssociatedObject.ActualWidth ||
                                    _curPosition.Y < AssociatedObject.Top ||
                                    _curPosition.Y > AssociatedObject.Top + AssociatedObject.ActualHeight)
                                {
                                    AnimationTranslate(MoveDirection.Left,
                                        AssociatedObject.ActualWidth + _autoHideFactor, () =>
                                        {
                                            AssociatedObject.Left = -(AssociatedObject.ActualWidth + _autoHideFactor);
                                            _hideStatus = HideState.LeftHidden;
                                            AssociatedObject.Hide();
                                            _lockTimer = false;
                                        });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                AssociatedObject.Topmost = false;
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.LeftHidden:
                            if (_curPosition.X <= _autoHideFactor &&
                                _curPosition.Y >= AssociatedObject.Top &&
                                _curPosition.Y <= AssociatedObject.Top + AssociatedObject.ActualHeight)
                            {
                                AssociatedObject.Show();
                                AssociatedObject.Activate();
                                AnimationTranslate(MoveDirection.Right, AssociatedObject.ActualWidth + _autoHideFactor,
                                    () =>
                                    {
                                        AssociatedObject.Left = 0;
                                        _hideStatus = HideState.PreviewLeftHidden;
                                        _lockTimer = false;
                                    });
                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                    }
                }
                else
                {
                    _lockTimer = false;
                }
            }
        }

        private void AnimationTranslate(MoveDirection direction, double distance, Action completedEvent = null)
        {
            double fromValue = 0, toValue = 0;
            var dependencyProperty = Window.TopProperty;
            switch (direction)
            {
                case MoveDirection.Top:
                    dependencyProperty = Window.TopProperty;
                    fromValue = AssociatedObject.Top;
                    toValue = fromValue - distance;
                    break;
                case MoveDirection.Right:
                    dependencyProperty = Window.LeftProperty;
                    fromValue = AssociatedObject.Left;
                    toValue = fromValue + distance;
                    break;
                case MoveDirection.Bottom:
                    dependencyProperty = Window.TopProperty;
                    fromValue = AssociatedObject.Top;
                    toValue = fromValue + distance;
                    break;
                case MoveDirection.Left:
                    dependencyProperty = Window.LeftProperty;
                    fromValue = AssociatedObject.Left;
                    toValue = fromValue - distance;
                    break;
            }
            var animation = new DoubleAnimation(fromValue, toValue, new Duration(TimeSpan.FromMilliseconds(500)), FillBehavior.Stop);
            if (completedEvent != null)
            {
                animation.Completed += (sender, args) => { completedEvent.Invoke(); };
            }
            AssociatedObject.BeginAnimation(dependencyProperty, animation);
        }

        #endregion
    }
}
