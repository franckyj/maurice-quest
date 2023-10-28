using GLFW;

namespace MyOtherOtherGame.inputs
{
    internal class Keyboard
    {
        private Action NoOp = () => { };

        private Action _onShowFpsPressed;
        private Action _onLeftPressed;
        private Action _onRightPressed;
        private Action _onUpPressed;
        private Action _onDownPressed;
        private Action _onZoomInPressed;
        private Action _onZoomOutPressed;

        public Keyboard(Window window)
        {
            if (window.NativeWindow is NativeWindow glfwWindow && glfwWindow != null)
            {
                Glfw.SetKeyCallback(glfwWindow, KeyCallback);
                Glfw.SetMouseButtonCallback(glfwWindow, MouseButtonCallback);
                Glfw.SetScrollCallback(glfwWindow, MouseCallback);
            }
            else
            {
                throw new ArgumentException($"{nameof(Keyboard)} doesn't support a non or null GLFW window");
            }

            _onShowFpsPressed = NoOp;
            _onLeftPressed = NoOp;
            _onRightPressed = NoOp;
            _onUpPressed = NoOp;
            _onDownPressed = NoOp;
            _onZoomInPressed = NoOp;
            _onZoomOutPressed = NoOp;
        }

        public void SetOnShowFpsPressed(Action action) => _onShowFpsPressed = action ?? NoOp;
        public void SetOnLeftPressed(Action action) => _onLeftPressed = action ?? NoOp;
        public void SetOnRightPressed(Action action) => _onRightPressed = action ?? NoOp;
        public void SetOnUpPressed(Action action) => _onUpPressed = action ?? NoOp;
        public void SetOnDownPressed(Action action) => _onDownPressed = action ?? NoOp;
        public void SetOnZoomInPressed(Action action) => _onZoomInPressed = action ?? NoOp;
        public void SetOnZoomOutPressed(Action action) => _onZoomOutPressed = action ?? NoOp;

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
        { }

        private void MouseCallback(IntPtr window, double x, double y)
        {
            if (y == 1)
                _onZoomInPressed();
            else if (y == -1)
                _onZoomOutPressed();
        }
    }
}
