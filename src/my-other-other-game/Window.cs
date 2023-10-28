using GLFW;

namespace MyOtherOtherGame;

internal sealed class Window : IDisposable
{
    private readonly string _title;

    private readonly D3D11Application _application;
    private NativeWindow _window;

    private bool _isMaximized;
    private bool _isMinimized;
    private bool _isResizing;

    private bool _disposed;

    public int ClientWindowWidth { get; private set; } = 0;
    public int ClientWindowHeight { get; private set; } = 0;
    public IntPtr WindowHandle => _window.Hwnd;
    public object NativeWindow => _window;

    public Window(D3D11Application application, string title = "Hello!")
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        _title = title;

        Initialize();
    }

    private void Initialize()
    {
        try
        {
            var result = Glfw.Init();
            if (!result)
            {
                throw new GLFW.Exception("Error while initializing GLFW");
            }

            var videoMode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);

            //ClientWindowWidth = videoMode.Width;
            //ClientWindowHeight = videoMode.Height;

            ClientWindowWidth = 1280;
            ClientWindowHeight = 720;

            Glfw.WindowHint(Hint.ScaleToMonitor, false);
            Glfw.WindowHint(Hint.Resizable, Constants.False);
            Glfw.WindowHint(Hint.ClientApi, ClientApi.None);

            _window = new NativeWindow(ClientWindowWidth, ClientWindowHeight, _title);

            int windowLeft = videoMode.Width / 2 - ClientWindowWidth / 2;
            int windowTop = videoMode.Height / 2 - ClientWindowHeight / 2;

            Glfw.SetWindowPosition(_window, windowLeft, windowTop);
            Glfw.SetWindowSizeCallback(_window, ResizeCallback);
            Glfw.SetWindowMaximizeCallback(_window, MaximizedCallback);
            Glfw.SetCloseCallback(_window, CloseCallback);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void ResizeCallback(IntPtr window, int width, int height)
    {
        if (width == ClientWindowWidth && height == ClientWindowHeight) return;

        ClientWindowWidth = width;
        ClientWindowHeight = height;

        _application.Pause();
        _application.Resize();
        _application.Unpause();
    }

    private void MaximizedCallback(IntPtr window, bool maximized)
    {
        _isMinimized = !maximized;
        _isMaximized = maximized;

        if (_isMinimized)
        {
            _application.Pause();
        }
        else
        {
            _application.Resize();
            _application.Unpause();
        }
    }

    private void CloseCallback(IntPtr window)
    {
        _application.Stop();
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Cleanup();
            }

            _disposed = true;
        }
    }

    private void Cleanup()
    {
        Glfw.DestroyWindow(_window);
        Glfw.Terminate();
    }
}
