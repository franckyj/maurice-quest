using System.Numerics;
using GLFW;

namespace MyTetris;

internal class Input
{
    private static readonly Action NoOp = () => { };
    private static readonly Action<Vector2> NoOpVector2 = _ => { };
    private static readonly Action<Vector3> NoOpVector3 = _ => { };

    private readonly NativeWindow _glfwWindow;

    private Action _onShowFpsPressed;
    private Action _onLeftPressed;
    private Action _onRightPressed;
    private Action _onUpPressed;
    private Action _onDownPressed;
    private Action _onZoomInPressed;
    private Action _onZoomOutPressed;

    private bool _isDragging;
    private Action<Vector3> _onDragRotating;
    private Vector2 _currentDragMouseReference;

    // glfw stuff
    private readonly KeyCallback _keyCallback;
    private readonly MouseButtonCallback _mouseButtonCallback;
    private readonly MouseCallback _mouseMoveCallback;
    private readonly MouseCallback _mouseScrollCallback;

    public Input(Window window)
    {
        // prevent GC (https://github.com/ForeverZer0/glfw-net/issues/36)
        _keyCallback = KeyCallback;
        _mouseButtonCallback = MouseButtonCallback;
        _mouseMoveCallback = MouseMoveCallback;
        _mouseScrollCallback = MouseScrollCallback;

        if (window.NativeWindow is NativeWindow glfwWindow && glfwWindow != null)
        {
            Glfw.SetKeyCallback(glfwWindow, _keyCallback);
            Glfw.SetMouseButtonCallback(glfwWindow, _mouseButtonCallback);
            Glfw.SetCursorPositionCallback(glfwWindow, _mouseMoveCallback);
            Glfw.SetScrollCallback(glfwWindow, _mouseScrollCallback);

            _glfwWindow = glfwWindow;
        }
        else
        {
            throw new ArgumentException($"{nameof(Input)} doesn't support a non or null GLFW window");
        }

        _onShowFpsPressed = NoOp;
        _onLeftPressed = NoOp;
        _onRightPressed = NoOp;
        _onUpPressed = NoOp;
        _onDownPressed = NoOp;
        _onZoomInPressed = NoOp;
        _onZoomOutPressed = NoOp;
        _onDragRotating = NoOpVector3;

        _isDragging = false;
    }

    public void SetOnShowFpsPressed(Action action) => _onShowFpsPressed = action ?? NoOp;
    public void SetOnLeftPressed(Action action) => _onLeftPressed = action ?? NoOp;
    public void SetOnRightPressed(Action action) => _onRightPressed = action ?? NoOp;
    public void SetOnUpPressed(Action action) => _onUpPressed = action ?? NoOp;
    public void SetOnDownPressed(Action action) => _onDownPressed = action ?? NoOp;
    public void SetOnZoomInPressed(Action action) => _onZoomInPressed = action ?? NoOp;
    public void SetOnZoomOutPressed(Action action) => _onZoomOutPressed = action ?? NoOp;
    public void SetOnDragRotating(Action<Vector3> action) => _onDragRotating = action ?? NoOpVector3;

    public void UpdateInput()
    {
        Glfw.PollEvents();
    }

    private void KeyCallback(IntPtr window, Keys key, int scanCode, InputState state, ModifierKeys mods)
    {
        if (key == Keys.F1 && state == InputState.Press) _onShowFpsPressed();
        if ((key == Keys.Left || key == Keys.A) && (state == InputState.Press || state == InputState.Repeat)) _onLeftPressed();
        if ((key == Keys.Right || key == Keys.D) && (state == InputState.Press || state == InputState.Repeat)) _onRightPressed();
        if ((key == Keys.Up || key == Keys.W) && (state == InputState.Press || state == InputState.Repeat)) _onUpPressed();
        if ((key == Keys.Down || key == Keys.S) && (state == InputState.Press || state == InputState.Repeat)) _onDownPressed();
    }

    private void MouseButtonCallback(IntPtr window, MouseButton button, InputState state, ModifierKeys modifiers)
    {
        if (button == MouseButton.Left)
        {
            _isDragging = state == InputState.Press || state == InputState.Repeat;

            if (!_isDragging)
            {
                _currentDragMouseReference = Vector2.Zero;
            }
            else
            {
                Glfw.GetCursorPosition(_glfwWindow, out double x, out double y);
                _currentDragMouseReference = new Vector2((float)x, (float)y);
            }
        }
    }

    private void MouseMoveCallback(IntPtr window, double x, double y)
    {
        const float Sensitivity = 0.01f;

        if (_isDragging)
        {
            var mousePosition = new Vector2((float)x, (float)y);
            var mouseOffset = mousePosition - _currentDragMouseReference;

            var rotation = new Vector3(
                // move in Y, rotate in X
                mouseOffset.Y * Sensitivity,

                // move in X, rotate in Y
                mouseOffset.X * Sensitivity,
                0.0f);

            // rotate
            _onDragRotating(rotation);

            // store mouse
            _currentDragMouseReference = mousePosition;
        }
    }

    private void MouseScrollCallback(IntPtr window, double x, double y)
    {
        if (y == 1)
            _onZoomInPressed();
        else if (y == -1)
            _onZoomOutPressed();
    }
}

public static class Vector3Extensions
{
    internal static Vector3 RotateX(this Vector3 vector, float angle)
    {
        return new Vector3(
            vector.X,
            (float)(vector.Y * Math.Cos(angle) - vector.Z * Math.Sin(angle)),
            (float)(vector.Y * Math.Sin(angle) + vector.Z * Math.Cos(angle))
        );
    }

    internal static Vector3 RotateY(this Vector3 vector, float angle)
    {
        return new Vector3(
            (float)(vector.X * Math.Cos(angle) - vector.Z * Math.Sin(angle)),
            vector.Y,
            (float)(vector.X * Math.Sin(angle) + vector.Z * Math.Cos(angle))
        );
    }
}
