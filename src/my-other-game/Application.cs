using GLFW;

namespace MyOtherGame
{
    internal class Application : IDisposable
    {
        private string _title;
        private bool _disposed;

        private DeviceResources _deviceResources;
        private GameMain _game;

        protected NativeWindow? Window { get; private set; }
        protected int Width { get; private set; }
        protected int Height { get; private set; }

        protected float AspectRatio => (float)Width / (float)Height;
        protected float InverseAspectRatio => (float)Height / (float)Width;

        public Application(string title)
        {
            _title = title;
        }

        public void Run()
        {
            Initialize();
            Load();

            _game.Run(Window);

            Dispose(true);
        }

        protected virtual void Initialize()
        {
            try
            {
                var result = Glfw.Init();
                if (!result)
                {
                    throw new GLFW.Exception("Error while initializing GLFW");
                }

                var videoMode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);
                //Width = (int)(videoMode.Width * 0.8f);
                //Height = (int)(videoMode.Height * 0.8f);

                Width = 1280;
                Height = 720;

                Glfw.WindowHint(Hint.ScaleToMonitor, false);
                Glfw.WindowHint(Hint.ClientApi, ClientApi.None);

                Window = new NativeWindow(Width, Height, _title);

                int windowLeft = videoMode.Width / 2 - Width / 2;
                int windowTop = videoMode.Height / 2 - Height / 2;
                Glfw.SetWindowPosition(Window, windowLeft, windowTop);
                Glfw.SetWindowSizeCallback(Window, ResizeCallback);
                Glfw.SetCloseCallback(Window, CloseCallback);

                // initialize the game main + device resources
                _deviceResources = new DeviceResources();
                _deviceResources.SetWindow(Window.Hwnd, Window.ClientSize.Width, Window.ClientSize.Height);
                _game = new GameMain(_deviceResources, new GameRenderer(_deviceResources));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        protected virtual void Cleanup()
        {
            Glfw.DestroyWindow(Window);
            Glfw.Terminate();
        }

        protected virtual void Load()
        { }

        protected virtual void OnResize()
        { }

        //protected virtual void Render()
        //{ }

        //protected virtual void Update(float mouseX, float mouseY)
        //{ }

        private void ResizeCallback(IntPtr window, int width, int height)
        {
            Width = width;
            Height = height;
            OnResize();
        }

        private void CloseCallback(IntPtr window)
        {
            _game.Stop();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Cleanup();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
