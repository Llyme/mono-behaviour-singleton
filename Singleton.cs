using System.Collections;
using UnityEngine;

namespace Core
{
	public abstract class Singleton : MonoBehaviour
	{
		protected static int _awoke = 0;
		protected static int _started = 0;

		public bool Started { get; private set; }

		/// <summary>
		/// Check if all Singletons are loaded.
		/// </summary>
		public static bool AllLoaded => _started == _awoke;

		protected virtual void Awake()
		{

		}

		protected virtual IEnumerator Start()
		{
			yield return Start_Internal();

			Started = true;
		}

		protected virtual IEnumerator Start_Internal()
		{
			yield break;
		}
	}

	/// <summary>
	/// MonoBehavior Singleton.
	/// Requires to be added in a scene at least once.
	/// Preceeding singletons with the same type are destroyed.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class Singleton<T> : Singleton where T : Singleton
	{
		private static T self;

		public static T Self
		{
			get
			{
				if (!self)
					self = FindObjectOfType<T>();

				if (self == null)
					Debug.LogWarning($"Singleton<{typeof(T).Name}> is not yet instantiated!");

				return self;
			}
		}

		public static bool Exists => self != null;

		protected override sealed void Awake()
		{
			if (Self == this)
			{
				_awoke++;

				Debug.Log($"Singleton<{typeof(T).Name}> instantiated.");
				AfterAwake();
				return;
			}

			Debug.LogError($"Singleton<{typeof(T).Name}> duplicate found.");
			Destroy(this);
		}

		/// <summary>
		/// Called after the singleton has
		/// called the `Awake` function.
		/// </summary>
		protected virtual void AfterAwake()
		{
		}

		protected override sealed IEnumerator Start() => base.Start();

		protected override sealed IEnumerator Start_Internal()
		{
			_started++;

			Debug.Log($"({_started}/{_awoke}) Singleton<{typeof(T).Name}> loaded.");

			if (AllLoaded)
				_started = _awoke = 0;

			yield return new WaitUntil(() => AllLoaded);
			yield return AfterStart();
		}

		/// <summary>
		/// Called after the singleton has
		/// called the `Start` function.
		/// </summary>
		protected virtual IEnumerator AfterStart()
		{
			yield break;
		}

		private void OnDestroy()
		{
			if (!self)
				return;

			if (self != this)
				return;

			self = null;
			OnDestroyed();
		}

		/// <summary>
		/// Called after the singleton has called
		/// the `OnDestroy` function.
		/// </summary>
		protected virtual void OnDestroyed()
		{
		}
	}
}