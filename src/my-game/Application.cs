﻿using System.Runtime.InteropServices;
using GLFW;

namespace MyGame
{
    internal class Application : IDisposable
    {
        private string _title;
        private bool _disposed;

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

            bool running = true;
            while (!Window!.IsClosing && running)
            {
                Glfw.PollEvents();

                var escape = Glfw.GetKey(Window, Keys.Escape);
                if (escape == InputState.Press)
                    running = false;

                Glfw.GetCursorPosition(Window, out double mouseX, out double mouseY);

                Update((float)mouseX, (float)mouseY);
                //Update(0f, 0f);
                Render();
            }
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
                Width = (int)(videoMode.Width * 0.85f);
                Height = (int)(videoMode.Height * 0.85f);

                //Width = 640;
                //Height = 640;

                Glfw.WindowHint(Hint.ScaleToMonitor, false);
                Glfw.WindowHint(Hint.ClientApi, ClientApi.None);

                Window = new NativeWindow(Width, Height, _title);

                int windowLeft = videoMode.Width / 2 - Width / 2;
                int windowTop = videoMode.Height / 2 - Height / 2;
                Glfw.SetWindowPosition(Window, windowLeft, windowTop);
                Glfw.SetWindowSizeCallback(Window, ResizeCallback);
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
        {
            //Console.WriteLine("Window has been resized!");
        }

        protected virtual void Render()
        { }

        protected virtual void Update(float mouseX, float mouseY)
        { }

        private void ResizeCallback(IntPtr window, int width, int height)
        {
            Width = width;
            Height = height;
            OnResize();
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
