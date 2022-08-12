// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace bottlenoselabs.Katabasis
{
	public class Game : IDisposable
	{
		private static readonly TimeSpan MaxElapsedTime = TimeSpan.FromMilliseconds(500);
		private readonly GameTime _gameTime;
		private readonly Stopwatch _gameTimer;
		private readonly bool[] _textInputControlDown;
		private TimeSpan _accumulatedElapsedTime;
		private GraphicsAdapter? _currentAdapter;
		private bool _forceElapsedTimeToZero;
		private bool _hasInitialized;

		// must be a power of 2 so we can do a bitmask optimization when checking worst case
		private const int PreviousSleepTimeCount = 128;
		private const int SleepTimeMask = PreviousSleepTimeCount - 1;
		private TimeSpan[] _previousSleepTimes = new TimeSpan[PreviousSleepTimeCount];
		private int _sleepTimeIndex = 0;
		private TimeSpan _worstCaseSleepPrecision = TimeSpan.FromMilliseconds(1);

		private TimeSpan _inactiveSleepTime;
		private bool _isActive;
		private bool _isDisposed;
		private bool _isMouseVisible;
		private long _previousTicks;
		private bool _suppressDraw;
		private TimeSpan _targetElapsedTime;
		private bool _textInputSuppress;
		private int _updateFrameLag;
		internal bool RunApplication;

		public Game()
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
			// Native.SetDllImportResolverCallback(Assembly.GetExecutingAssembly());

			LaunchParameters = new LaunchParameters();
			_currentAdapter = null;

			IsMouseVisible = false;
			IsFixedTimeStep = true;
			TargetElapsedTime = TimeSpan.FromTicks(166667); // 60fps
			InactiveSleepTime = TimeSpan.FromSeconds(0.02);

			for (var i = 0; i < _previousSleepTimes.Length; i += 1)
			{
				_previousSleepTimes[i] = TimeSpan.FromMilliseconds(1);
			}

			_textInputControlDown = new bool[FNAPlatform.TextInputCharacters.Length];

			_hasInitialized = false;
			_suppressDraw = false;
			_isDisposed = false;

			_gameTime = new GameTime();
			_gameTimer = new Stopwatch();

			Window = FNAPlatform.CreateWindow();
			Mouse.WindowHandle = Window.Handle;
			TouchPanel.WindowHandle = Window.Handle;

			FrameworkDispatcher.Update();

			// Ready to run the loop!
			RunApplication = true;

			GraphicsDeviceManager.Instance = new GraphicsDeviceManager(this);
		}

		public GraphicsDevice GraphicsDevice => GraphicsDeviceManager.Instance.GraphicsDevice;

		public TimeSpan InactiveSleepTime
		{
			get => _inactiveSleepTime;
			set
			{
				if (value < TimeSpan.Zero)
				{
					throw new ArgumentOutOfRangeException(
						"The time must be positive.",
						default(Exception));
				}

				_inactiveSleepTime = value;
			}
		}

		public bool IsActive
		{
			get => _isActive;
			internal set
			{
				if (_isActive != value)
				{
					_isActive = value;
					if (_isActive)
					{
						OnActivated(this, EventArgs.Empty);
					}
					else
					{
						OnDeactivated(this, EventArgs.Empty);
					}
				}
			}
		}

		public bool IsFixedTimeStep { get; set; }

		public bool IsMouseVisible
		{
			get => _isMouseVisible;
			set
			{
				if (_isMouseVisible != value)
				{
					_isMouseVisible = value;
					FNAPlatform.OnIsMouseVisibleChanged(value);
				}
			}
		}

		public LaunchParameters LaunchParameters { get; }

		public TimeSpan TargetElapsedTime
		{
			get => _targetElapsedTime;
			set
			{
				if (value <= TimeSpan.Zero)
				{
					throw new ArgumentOutOfRangeException(
						"The time must be positive and non-zero.",
						default(Exception));
				}

				_targetElapsedTime = value;
			}
		}

		public GameWindow Window { get; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
			Disposed?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler<EventArgs>? Activated;

		public event EventHandler<EventArgs>? Deactivated;

		public event EventHandler<EventArgs>? Disposed;

		public event EventHandler<EventArgs>? Exiting;

		~Game() => Dispose(false);

		public void Exit()
		{
			RunApplication = false;
			_suppressDraw = true;
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Not used?")]
		public void ResetElapsedTime()
		{
			/* This only matters the next tick, and ONLY when
			 * IsFixedTimeStep is false!
			 * For fixed time step, this is totally ignored.
			 * -flibit
			 */
			if (!IsFixedTimeStep)
			{
				_forceElapsedTimeToZero = true;
			}
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Not used?")]
		public void SuppressDraw() => _suppressDraw = true;

		public void RunOneFrame()
		{
			if (!_hasInitialized)
			{
				DoInitialize();
				_gameTimer.Restart();
				_hasInitialized = true;
			}

			Tick();
		}

		public void Run()
		{
			AssertNotDisposed();

			if (!_hasInitialized)
			{
				DoInitialize();
				_hasInitialized = true;
			}

			BeginRun();
			BeforeLoop();

			_gameTimer.Restart();
			RunLoop();

			EndRun();
			AfterLoop();
		}

		public void Tick()
		{
			/* NOTE: This code is very sensitive and can break very badly,
			 * even with what looks like a safe change. Be sure to test
			 * any change fully in both the fixed and variable time step
			 * modes across multiple devices and platforms.
			 */

			AdvanceElapsedTime();

			if (IsFixedTimeStep)
			{
				/* If we are in fixed timestep, we want to wait until the next frame,
 				 * but we don't want to oversleep. Requesting repeated 1ms sleeps and
 				 * seeing how long we actually slept for lets us estimate the worst case
 				 * sleep precision so we don't oversleep the next frame.
 				 */
				while (_accumulatedElapsedTime + _worstCaseSleepPrecision < TargetElapsedTime)
				{
					Thread.Sleep(1);
					TimeSpan timeAdvancedSinceSleeping = AdvanceElapsedTime();
					UpdateEstimatedSleepPrecision(timeAdvancedSinceSleeping);
				}

				/* Now that we have slept into the sleep precision threshold, we need to wait
 				 * for just a little bit longer until the target elapsed time has been reached.
 				 * SpinWait(1) works by pausing the thread for very short intervals, so it is
 				 * an efficient and time-accurate way to wait out the rest of the time.
 				 */
				while (_accumulatedElapsedTime < TargetElapsedTime)
				{
					Thread.SpinWait(1);
					AdvanceElapsedTime();
				}
			}

			// Now that we are going to perform an update, let's poll events.
			FNAPlatform.PollEvents(
				this,
				ref _currentAdapter!,
				_textInputControlDown,
				ref _textInputSuppress);

			// Do not allow any update to take longer than our maximum.
			if (_accumulatedElapsedTime > MaxElapsedTime)
			{
				_accumulatedElapsedTime = MaxElapsedTime;
			}

			if (IsFixedTimeStep)
			{
				_gameTime.ElapsedGameTime = TargetElapsedTime;
				var stepCount = 0;

				// Perform as many full fixed length time steps as we can.
				while (_accumulatedElapsedTime >= TargetElapsedTime)
				{
					_gameTime.TotalGameTime += TargetElapsedTime;
					_accumulatedElapsedTime -= TargetElapsedTime;
					stepCount += 1;

					AssertNotDisposed();
					Update(_gameTime);
				}

				// Every update after the first accumulates lag
				_updateFrameLag += Math.Max(0, stepCount - 1);

				/* If we think we are running slowly, wait
				 * until the lag clears before resetting it
				 */
				if (_gameTime.IsRunningSlowly)
				{
					if (_updateFrameLag == 0)
					{
						_gameTime.IsRunningSlowly = false;
					}
				}
				else if (_updateFrameLag >= 5)
				{
					/* If we lag more than 5 frames,
					 * start thinking we are running slowly.
					 */
					_gameTime.IsRunningSlowly = true;
				}

				/* Every time we just do one update and one draw,
				 * then we are not running slowly, so decrease the lag.
				 */
				if (stepCount == 1 && _updateFrameLag > 0)
				{
					_updateFrameLag -= 1;
				}

				/* Draw needs to know the total elapsed time
				 * that occured for the fixed length updates.
				 */
				_gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
			}
			else
			{
				// Perform a single variable length update.
				if (_forceElapsedTimeToZero)
				{
					/* When ResetElapsedTime is called,
					 * Elapsed is forced to zero and
					 * Total is ignored entirely.
					 * -flibit
					 */
					_gameTime.ElapsedGameTime = TimeSpan.Zero;
					_forceElapsedTimeToZero = false;
				}
				else
				{
					_gameTime.ElapsedGameTime = _accumulatedElapsedTime;
					_gameTime.TotalGameTime += _gameTime.ElapsedGameTime;
				}

				_accumulatedElapsedTime = TimeSpan.Zero;
				AssertNotDisposed();
				Update(_gameTime);
			}

			// Draw unless the update suppressed it.
			if (_suppressDraw)
			{
				_suppressDraw = false;
			}
			else
			{
				/* Draw/EndDraw should not be called if BeginDraw returns false.
				 * http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
				 * http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
				 */
				if (BeginDraw())
				{
					Draw(_gameTime);
					EndDraw();
				}
			}
		}

		internal void RedrawWindow()
		{
			/* Draw/EndDraw should not be called if BeginDraw returns false.
			 * http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
			 * http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
			 *
			 * Additionally, if we haven't even started yet, be quiet until we have!
			 * -flibit
			 */
			if (_gameTime.TotalGameTime != TimeSpan.Zero && BeginDraw())
			{
				Draw(new GameTime(_gameTime.TotalGameTime, TimeSpan.Zero));
				EndDraw();
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					// FIXME: Does XNA4 require the GDM to be disposable? -flibit
					(GraphicsDeviceManager.Instance as IDisposable).Dispose();

					FNAPlatform.DisposeWindow(Window);
				}

				AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

				_isDisposed = true;
			}
		}

		protected virtual bool BeginDraw()
		{
			GraphicsDeviceManager.Instance.BeginDraw();
			return true;
		}

		protected virtual void EndDraw() => GraphicsDeviceManager.Instance.EndDraw();

		protected virtual void BeginRun()
		{
		}

		protected virtual void EndRun()
		{
		}

		protected virtual void LoadContent()
		{
		}

		protected virtual void UnloadContent()
		{
		}

		protected virtual void Initialize()
		{
			var graphicsDeviceManager = GraphicsDeviceManager.Instance;
			graphicsDeviceManager.DeviceDisposing += (o, e) => UnloadContent();
			graphicsDeviceManager.DeviceCreated += (o, e) => LoadContent();
			LoadContent();
		}

		protected virtual void Draw(GameTime gameTime)
		{
		}

		protected virtual void Update(GameTime gameTime) => FrameworkDispatcher.Update();

		protected virtual void OnExiting(object sender, EventArgs args) => Exiting?.Invoke(this, args);

		protected virtual void OnActivated(object sender, EventArgs args)
		{
			AssertNotDisposed();
			Activated?.Invoke(this, args);
		}

		protected virtual void OnDeactivated(object sender, EventArgs args)
		{
			AssertNotDisposed();
			Deactivated?.Invoke(this, args);
		}

		protected virtual bool ShowMissingRequirementMessage(Exception exception)
		{
			switch (exception)
			{
				case NoAudioHardwareException _:
					FNAPlatform.ShowRuntimeError(
						Window.Title,
						"Could not find a suitable audio device. Verify that a sound card is\ninstalled, and check the driver properties to make sure it is not disabled.");

					return true;
				case NoSuitableGraphicsDeviceException _:
					FNAPlatform.ShowRuntimeError(
						Window.Title,
						"Could not find a suitable graphics device. More information:\n\n" + exception.Message);

					return true;
				default:
					return false;
			}
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
		{
			var exception = args.ExceptionObject as Exception;
			ShowMissingRequirementMessage(exception!);
		}

		[DebuggerNonUserCode]
		private void AssertNotDisposed()
		{
			if (_isDisposed)
			{
				string name = GetType().Name;
				throw new ObjectDisposedException(
					name,
					$"The {name} object was used after being Disposed.");
			}
		}

		private void DoInitialize()
		{
			AssertNotDisposed();

			/* If this is late, you can still create it yourself.
			 * In fact, you can even go as far as creating the
			 * _manager_ before base.Initialize(), but Begin/EndDraw
			 * will not get called. Just... please, make the service
			 * before calling Run().
			 */
			GraphicsDeviceManager.Instance.CreateDevice();

			Initialize();
		}

		private void BeforeLoop()
		{
			_currentAdapter = FNAPlatform.RegisterGame(this);
			IsActive = true;

			// Perform initial check for a touch device
			TouchPanel.TouchDeviceExists = FNAPlatform.GetTouchCapabilities().IsConnected;
		}

		private void AfterLoop() => FNAPlatform.UnregisterGame(this);

		private void RunLoop()
		{
			/* Some platforms (i.e. Emscripten) don't support
			 * indefinite while loops, so instead we have to
			 * surrender control to the platform's main loop.
			 * -caleb
			 */
			if (FNAPlatform.NeedsPlatformMainLoop())
			{
				/* This breaks control flow and jumps
				 * directly into the platform main loop.
				 * Nothing below this call will be executed.
				 */
				FNAPlatform.RunPlatformMainLoop(this);
			}

			while (RunApplication)
			{
				Tick();
			}

			OnExiting(this, EventArgs.Empty);
		}

		private TimeSpan AdvanceElapsedTime()
		{
			var currentTicks = _gameTimer.Elapsed.Ticks;
			var timeAdvanced = TimeSpan.FromTicks(currentTicks - _previousTicks);
			_accumulatedElapsedTime += timeAdvanced;
			_previousTicks = currentTicks;
			return timeAdvanced;
		}

		/* To calculate the sleep precision of the OS, we take the worst case
		  * time spent sleeping over the results of previous requests to sleep 1ms.
		  */
		private void UpdateEstimatedSleepPrecision(TimeSpan timeSpentSleeping)
		{
			/* It is unlikely that the scheduler will actually be more imprecise than
			  * 4ms and we don't want to get wrecked by a single long sleep so we cap this
			  * value at 4ms for sanity.
			  */
			TimeSpan upperTimeBound = TimeSpan.FromMilliseconds(4);

			if (timeSpentSleeping > upperTimeBound)
			{
				timeSpentSleeping = upperTimeBound;
			}

			/* We know the previous worst case - it's saved in worstCaseSleepPrecision.
			  * We also know the current index. So the only way the worst case changes
			  * is if we either 1) just got a new worst case, or 2) the worst case was
			  * the oldest entry on the list.
			  */
			if (timeSpentSleeping >= _worstCaseSleepPrecision)
			{
				_worstCaseSleepPrecision = timeSpentSleeping;
			}
			else if (_previousSleepTimes[_sleepTimeIndex] == _worstCaseSleepPrecision)
			{
				TimeSpan maxSleepTime = TimeSpan.MinValue;
				for (int i = 0; i < _previousSleepTimes.Length; i += 1)
				{
					if (_previousSleepTimes[i] > maxSleepTime)
					{
						maxSleepTime = _previousSleepTimes[i];
					}
				}

				_worstCaseSleepPrecision = maxSleepTime;
			}

			_previousSleepTimes[_sleepTimeIndex] = timeSpentSleeping;
			_sleepTimeIndex = (_sleepTimeIndex + 1) & SleepTimeMask;
		}
	}
}
